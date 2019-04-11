<template>

    <div class="campaign box" :class="{'expanded': expanded, 'inactive': campaign.closeAction}" v-if="campaign">

        <div class="columns is-mobile is-multiline titleBar" @click="expandWithSentiment('Abstain')">
            <div class="column is-12-mobile columns is-gapless is-mobile nameBar">

                <div class="leftSide">
                    <span class="statusIcon" v-html="statusIcon" v-tooltip="'Status: ' + (campaign.outcome ? campaign.outcome : 'Active')"></span>
                    <h2 class="title is-size-4 displayName">
                        {{campaign.subject.fullUsername}}
                    </h2>
                </div>

                <div class="roleColumn">
                    <span class="toRole" :style="roleStyle(campaign.targetRole.id)">&#10149; {{campaign.targetRole.name}}</span>

                    <span class="mobile-expander">
                        <template v-if="expanded">
                            &#9650;
                        </template>
                        <template v-else>
                            &#9660;
                        </template>
                    </span>
                </div>

            </div>

            <div class="column is-narrow-tablet adminButtons">
                <a class="button is-primary is-small is-fullwidth" :class="{'is-loading': dialogLoading}" @click.stop="showPanel()">Info</a>
            </div>

            <div class="column is-narrow-tablet ratings">
                <div class="columns is-mobile">
                    <div class="column rating" @click.stop="expandWithSentiment('Approve')">
                        <span v-html="sentimentIcon('Approve')"></span> {{forCurrentUser ? '?' : campaign.votesFor}}
                    </div>
                    <div class="column rating" @click.stop="expandWithSentiment('Oppose')">
                        <span v-html="sentimentIcon('Oppose')"></span> {{forCurrentUser ? '?' : campaign.votesAgainst}}
                    </div>
                </div>

                <progress class="progress is-small" :class="sentimentColor(campaign)"
                    :value="forCurrentUser ? 1 : campaign.sentimentRatio" max="1" />
            </div>

            <div class="column is-narrow expander is-hidden-mobile">
                <template v-if="expanded">
                    &#9650;
                </template>
                <template v-else>
                    &#9660;
                </template>
            </div>
        </div>

        <div class="details">
            <small class="date">Campaign started <span class="has-text-weight-bold">{{formatDate(campaign.startDate)}}</span></small>

            <div class="commentList">
                <div v-if="forCurrentUser" class="commentNotification">
                    Sorry, you aren't allowed to see comments on your own campaign.
                </div>
                <PromotionCommentView v-else v-for="(comment, index) in comments" :key="comment.promotionCampaignId" :comment="comment" :hidden="!shouldShowEdit(campaign, comment)"
                                      :style="{'transition-delay': (index * 33) + 'ms'}" v-on:comment-edit-modal-opened="onCommentEditModalOpened"/>
            </div>

            <div class="field has-addons" v-if="!campaign.closeAction && !userAlreadyCommented(campaign) && !forCurrentUser">
                <p class="control">
                    <span class="select">
                        <select v-model="newComment.sentiment">
                            <option value="Abstain" v-html="sentimentIcon('Abstain')"></option>
                            <option value="Approve" v-html="sentimentIcon('Approve')"></option>
                            <option value="Oppose" v-html="sentimentIcon('Oppose')"></option>
                        </select>
                    </span>
                </p>

                <p class="control is-expanded">
                    <input class="input" :class="{'is-danger': error}" type="text" v-model="newComment.body" placeholder="Make a Comment...">
                </p>

                <p class="control">
                    <a class="button is-primary" :class="{'is-loading': commentSubmitting}" @click="submitComment()">Submit</a>
                </p>
            </div>

            <p class="help is-danger" v-if="error">{{error}}</p>

        </div>

    </div>

</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import PromotionCommentView from './PromotionCommentView.vue';
import PromotionComment from '@/models/promotions/PromotionComment';
import PromotionCampaign, {PromotionSentiment, SentimentIcons, StatusIcons, CampaignOutcome} from '@/models/promotions/PromotionCampaign';
import * as _ from 'lodash';
import {formatDate} from '@/app/Util';
import { Dictionary } from 'vuex';
import Role from '@/models/Role';
import store from '@/app/Store';
import PromotionCommentData from '@/models/promotions/PromotionCommentData';
import PromotionService from '@/services/PromotionService';
import ModixComponent from '@/components/ModixComponent.vue';


@Component({
    components: {PromotionCommentView}
})
export default class PromotionListItem extends ModixComponent
{
    @Prop() private campaign!: PromotionCampaign;
    @Prop({default: false}) private dialogLoading!: boolean;

    newComment: PromotionCommentData = { body: "", sentiment: PromotionSentiment.Abstain };
    expanded: boolean = false;
    error: string = "";
    commentSubmitting: boolean = false;
    comments: PromotionComment[] | null = null;

    formatDate(date: Date): string
    {
        return formatDate(date);
    }

    get canClose()
    {
        return store.userHasClaims(["PromotionsCloseCampaign"]);
    }

    get forCurrentUser()
    {
        return this.campaign.subject && this.state.user && this.campaign.subject.id.toString() == this.state.user.userId;
    }

    @Watch('newComment.body')
    commentChanged()
    {
        this.error = "";
    }

    resetNewComment()
    {
        this.newComment.sentiment = PromotionSentiment.Abstain;
        this.newComment.body = "";

        this.error = "";
    }

    async created()
    {
        this.resetNewComment();
    }

    sentimentColor(campaign: PromotionCampaign)
    {
        if (this.forCurrentUser)
        {
            return 'is-default';
        }

        if (this.campaign.sentimentRatio > 0.67)
        {
            return 'is-success';
        }

        if (this.campaign.sentimentRatio > 0.34)
        {
            return 'is-warning';
        }

        return 'is-danger';
    }

    sentimentIcon(sentiment: PromotionSentiment)
    {
        return SentimentIcons[sentiment];
    }

    get statusIcon()
    {
        return (this.campaign.outcome ? StatusIcons[this.campaign.outcome] : "&#128499;");
    }

    async submitComment()
    {
        this.commentSubmitting = true;

        try
        {
            if (this.campaign.closeAction)
            {
                this.error = "Campaign is not active.";
                return;
            }

            await PromotionService.commentOnCampaign(this.campaign, this.newComment);
            this.$emit("commentSubmitted");
            this.resetNewComment();
        }
        catch (err)
        {
            console.log(err);
            this.error = err.response.data;
        }
        finally
        {
            this.commentSubmitting = false;
        }
    }

    showPanel()
    {
        this.$emit('showPanel');
    }

    roleStyle(id: string)
    {
        let roles = this.$store.state.modix.roles as Role[];
        let found = _.find(roles, (role: Role) => role.id == id) as Role;

        return { color: found.fgColor };
    }

    async expandWithSentiment(sentiment: PromotionSentiment)
    {
        this.newComment.sentiment = sentiment;

        if (!this.expanded && !this.forCurrentUser && this.comments == null)
        {
            try
            {
                this.comments = await PromotionService.getComments(this.campaign.id);
            }
            finally
            {
                this.$nextTick(() => this.expanded = true);
            }
        }
        else
        {
            this.expanded = !this.expanded;
        }
    }

    onCommentEditModalOpened(comment: PromotionComment)
    {
        this.$emit('comment-edit-modal-opened', comment);
    }

    userAlreadyCommented()
    {
        return _.some(this.comments, comment => comment.isFromCurrentUser);
    }

    shouldShowEdit(campaign: PromotionCampaign, comment: PromotionComment): boolean
    {
        return campaign.closeAction == null;
    }
}
</script>
