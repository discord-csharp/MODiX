<template>

    <div class="campaign box" :class="{'expanded': expanded, 'inactive': campaign.closeAction}" v-if="campaign">
        
        <div class="columns is-mobile is-multiline" @click="expandWithSentiment('Abstain')">
            <div class="column is-12-mobile columns is-gapless is-mobile">

                <div class="column is-narrow leftSide">
                    <h1 class="title is-size-4">
                        
                        <span class="statusIcon" v-tooltip="'Status: ' + (campaign.outcome ? campaign.outcome : 'Active')">{{statusIcon}}</span>
                        <span class="displayName">{{campaign.subject.displayName}}</span>
                        <span class="toRole" :style="roleStyle(campaign.targetRole.id)">&#10149; {{campaign.targetRole.name}}</span>
                        
                    </h1>
                </div>

                <div class="column">
                    <span class="mobile-expander">
                        <template v-if="expanded">&#9650;</template>
                        <template v-else>&#9660;</template>
                    </span>
                </div>

            </div>

            <div class="column is-narrow-tablet adminButtons" v-if="canClose" >
                <a class="button is-primary is-small is-fullwidth" :class="{'is-loading': dialogLoading}"
                    :disabled="campaign.outcome == 'Accepted'" @click.stop="showPanel()">More…</a>
            </div>

            <div class="column is-narrow-tablet ratings">
                <div class="columns is-mobile">
                    <div class="column rating" @click.stop="expandWithSentiment('Approve')">
                        {{sentimentIcon("Approve")}} {{campaign.votesFor}}
                    </div>
                    <div class="column rating" @click.stop="expandWithSentiment('Oppose')">
                        {{sentimentIcon("Oppose")}} {{campaign.votesAgainst}}
                    </div>
                </div>

                <progress class="progress is-small" :class="sentimentColor(campaign)" 
                    :value="campaign.sentimentRatio" max="1" /> 
            </div>

            <div class="column is-narrow expander is-hidden-mobile">
                <template v-if="expanded">&#9650;</template>
                <template v-else>&#9660;</template>
            </div>
        </div>

        <div>
            <small class="date">Campaign started <span class="has-text-weight-bold">{{formatDate(campaign.startDate)}}</span></small>

            <div class="commentList">
                <PromotionCommentView v-for="(comment, index) in comments" :key="comment.promotionCampaignId" :comment="comment"
                                      :style="{'transition-delay': (index * 33) + 'ms'}" />
            </div>

            <div class="field has-addons" v-if="!campaign.closeAction">
                <p class="control">
                    <span class="select">
                        <select v-model="newComment.sentiment">
                            <option value="Abstain">{{sentimentIcon("Abstain")}}</option>
                            <option value="Approve">{{sentimentIcon("Approve")}}</option>
                            <option value="Oppose">{{sentimentIcon("Oppose")}}</option>
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

<style scoped lang="scss">

@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/box";
@import '~bulma/sass/elements/form';
@import "~bulma/sass/elements/progress";

.commentBox
{
    flex-basis: 100%;
}

.campaign .columns.is-gapless:not(:last-child)
{
    margin-bottom: 0;

    @include mobile()
    {
        margin-bottom: 0.5em;
    }
}

.date, .role
{
    font-size: 14px;

    font-weight: normal;
    color: grey;

    margin-bottom: -10px;

  
}

.displayName
{
    position: relative;
    left: 8px;

    @include mobile()
    {
        left: 16px;
        top: -12px;
    }
}

.toRole
{
    font-size: 14px;
    font-weight: 400 !important;
    padding: 4px 8px;

    border: 2px solid black;
    border-radius: 3px;

    position: relative;
    top: -2px;
    left: 20px;

    @include mobile()
    {
        display: table;

        margin-left: 2em;
        margin-top: -0.6em;
    }
}

.ratings 
{
    padding: 0.85rem 0.75rem 0em 0.75em;

    .rating
    {
        padding: 0 1em 0.5em 1em;
    }

    .columns
    {
        margin-bottom: 0;
    }

    @include mobile()
    {
        margin-top: 0.5em;
    }
}

.campaign
{
    position: relative;
    padding: 1.35em 1.2em 1.2em 1.2em;
    
    max-height: 70px;
    overflow: hidden;

    & > .columns
    {
        cursor: pointer;
    }

    transition: box-shadow 0.33s cubic-bezier(0.23, 1, 0.32, 1);

    &.expanded
    {
        box-shadow: 0px 0px 24px rgba(0, 0, 0, 0.33);
        max-height: none;

        //overflow-y: auto;

        & .comment
        {
            transform: rotateZ(0deg);
            opacity: 1;
        }
    }

    &.inactive
    {
        background-color: $light;
    }

    @include mobile()
    {
        padding: 1em;
    }
}

.level.is-mobile .level-item:last-child
{
    margin-right: 0;
}

.statusIcon
{
    display: inline-block;
    width: 1.3em;
}

.commentList
{
    margin-top: 0.5em;
}

.adminButtons
{
    margin-top: 0.1em;
}

.mobile-expander
{
    display: block;
    float: right;
    margin-right: 0.5em;

    @include tablet()
    {
        display: none;
    }
}

</style>


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


@Component({
    components: {PromotionCommentView}
})
export default class PromotionListItem extends Vue
{
    @Prop() private campaign!: PromotionCampaign;
    @Prop({default: false}) private dialogLoading!: boolean;

    newComment: PromotionCommentData = { body: "", sentiment: PromotionSentiment.Abstain };
    expanded: boolean = false;
    error: string = "";
    commentSubmitting: boolean = false;
    comments: PromotionComment[] = [];

    formatDate(date: Date): string
    {
        return formatDate(date);
    }

    get canClose()
    {
        return store.userHasClaims(["PromotionsCloseCampaign"]);
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
        this.comments = await PromotionService.getComments(this.campaign.id);
        this.resetNewComment();
    }

    sentimentColor(campaign: PromotionCampaign)
    {
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
        if (this.campaign.outcome == CampaignOutcome.Accepted)
        {
            return;
        }
        
        this.$emit('showPanel');
    }

    roleStyle(id: string)
    {
        let roles = this.$store.state.modix.roles as Role[];
        let found = _.find(roles, (role: Role) => role.id == id) as Role;

        return { color: found.fgColor, borderColor: found.fgColor };
    }

    expandWithSentiment(sentiment: PromotionSentiment)
    {
        this.newComment.sentiment = sentiment;
        this.expanded = !this.expanded;
    }
}
</script>
