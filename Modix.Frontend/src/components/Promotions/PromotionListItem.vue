<template>

    <div class="campaign box" :class="{'expanded': expanded, 'inactive': campaign.status != 'Active'}" v-if="campaign">
        
        <div class="columns" @click="expanded = !expanded" :title="'Status: '+campaign.status">
            <div class="column">
                <h1 class="title is-size-4">
                    <span class="statusIcon">{{statusIcon}}</span>
                    {{campaign.username}}

                    <span class="mobile-expander">
                        <template v-if="expanded">⯅</template>
                        <template v-else>⯆</template>
                    </span>

                    <small class="date">started {{formatDate(campaign.startDate)}}</small>
                </h1>
            </div>

            <div class="column is-narrow adminButtons" v-if="isStaff" >
                <a class="button is-primary is-fullwidth" :disabled="campaign.status == 'Approved'" @click.stop="showPanel()">Admin</a>
            </div>

            <div class="column ratings is-narrow" v-if="campaign.comments.length > 0">
                <div class="columns is-mobile">
                    <div class="column rating">
                        {{sentimentIcon("For")}} {{campaign.votesFor}}
                    </div>
                    <div class="column rating">
                        {{sentimentIcon("Against")}} {{campaign.votesAgainst}}
                    </div>
                </div>

                <progress class="progress is-small" :class="sentimentColor(campaign)" 
                    :value="campaign.sentimentRatio" max="1" />       
            </div>

            <div class="column is-narrow expander is-hidden-mobile">
                <template v-if="expanded">⯅</template>
                <template v-else>⯆</template>
            </div>
        </div>

        <div>
            <h2 class="heading is-size-5">Comments</h2>

            <div class="commentList">
                <PromotionCommentView v-for="(comment, index) in campaign.comments" :key="comment.id" :comment="comment"
                                      :style="{'transition-delay': (index * 33) + 'ms'}" />
            </div>

            <div class="field has-addons" v-if="campaign.status == 'Active'">
                <p class="control">
                    <span class="select">
                        <select v-model="newComment.sentiment">
                            <option value="Neutral">{{sentimentIcon("Neutral")}}</option>
                            <option value="For">{{sentimentIcon("For")}}</option>
                            <option value="Against">{{sentimentIcon("Against")}}</option>
                        </select>
                    </span>
                </p>

                <p class="control is-expanded" :class="{'is-loading': commentSubmitting}">
                    <input class="input" :class="{'is-danger': error}" type="text" v-model="newComment.body" placeholder="Make a Comment...">
                </p>

                <p class="control">
                    <a class="button is-primary" @click="submitComment()">Submit</a>
                </p>
            </div>

            <p class="help is-danger" v-if="error">{{error}}</p>
            
        </div>

    </div>

</template>

<style lang="scss">

@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/box";
@import "~bulma/sass/components/level";
@import '~bulma/sass/elements/form';

.commentBox
{
    flex-basis: 100%;
}

.date
{
    font-size: 12px;

    font-weight: normal;
    color: gray;

    margin-left: 1em;

    @include mobile()
    {
        display: block;
        margin-left: 0;
    }
}

.ratings 
{
    padding: 0.75rem 0.75rem 0em 0.75em;

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
    margin-top: -0.25em;
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
import PromotionComment from '@/models/PromotionComment';
import PromotionCampaign, {PromotionSentiment, SentimentIcons, StatusIcons} from '@/models/PromotionCampaign';
import * as _ from 'lodash';
import {formatPasteDate} from '@/app/Util';
import { Dictionary } from 'vuex';

import store from '@/app/Store';
import PromotionCommentData from '@/models/PromotionCommentData';
import GeneralService from '@/services/GeneralService';

@Component({
    components: {PromotionCommentView}
})
export default class PromotionListItem extends Vue
{
    @Prop() private campaign!: PromotionCampaign;

    newComment: PromotionCommentData = {body: "", sentiment: "Neutral"};
    expanded: boolean = false;
    error: string = "";
    commentSubmitting: boolean = false;

    formatDate(date: Date): string
    {
        return formatPasteDate(date);
    }

    get isStaff()
    {
        return store.userIsStaff();
    }

    @Watch('newComment.body')
    commentChanged()
    {
        this.error = "";
    }

    resetNewComment()
    {
        this.newComment.sentiment = "Neutral";
        this.newComment.body = "";

        this.error = "";
    }

    created()
    {
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
        return StatusIcons[this.campaign.status];
    }

    async submitComment()
    {
        this.commentSubmitting = true;

        try
        {
            if (this.campaign.status != "Active")
            {
                this.error = "Campaign is not active.";
                return;
            }

            await GeneralService.commentOnCampaign(this.campaign, this.newComment);
            this.$emit("commentSubmitted");
            this.resetNewComment();
        }
        catch (err)
        {
            this.error = err.response.data;
        }
        finally
        {
            this.commentSubmitting = false;
        }       
    }

    showPanel()
    {
        if (this.campaign.status == 'Approved')
        {
            return;
        }
        
        this.$emit('showPanel');
    }
}
</script>
