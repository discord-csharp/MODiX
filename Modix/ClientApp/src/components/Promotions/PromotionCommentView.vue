<template>

    <div class="comment" :style="{'background-color': backgroundColor()}">
        <span class="sentimentIcon" v-html="sentimentIcon(comment.sentiment)"></span>
        <span class="commentBody">
            {{comment.content}} <span class="date">{{formatDate(comment.createAction.created)}}</span>
        </span>
        <span class="button is-primary" :style="{'margin-left': '0.33em'}" v-if="comment.isFromCurrentUser" v-on:click="$emit('comment-edit-modal-opened', comment)">Edit</span>

    </div>

</template>

<style scoped lang="scss">

@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/box";
@import '~bulma/sass/elements/form';

.comment
{
    padding: 0.75em;

    margin-left: -1.5em;

    box-shadow: $box-shadow;

    transform: rotateZ(90deg);
    transform-origin: left center;
    transition: all 0.5s cubic-bezier(0.23, 1, 0.32, 1);
    opacity: 0;

    overflow-wrap: break-word;

    display: flex;
    flex-direction: row;
    align-items: center;
        align-content: stretch;

    &:first-child
    {
        border-radius: 0 1em 0 0;
    }

    &:last-child
    {
        margin-bottom: 1em;
        border-radius: 0 0 1em 0;
    }
    
    &:first-child:last-child
    {
        border-radius: 0 1em 1em 0;
    }

    &.expanded
    {
        transform: none;
        opacity: 1;
    }

    word-wrap: break-word;
}

.commentBody
{
    word-wrap: break-word;
    overflow: hidden; //no zalgo for you
}

.sentimentIcon
{
    display: inline-block;

    line-height: 1;

    min-width: 36px;
    min-height: 24px;

    margin-right: 0.33em;
    margin-left: 0.33em;

    font-size: 1.35em;
}

.date
{
    font-size: 0.8em;
    color: grey;
    margin-left: 0.5em;
}

</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

import {formatDate} from '@/app/Util';
import PromotionComment from '@/models/promotions/PromotionComment';
import { SentimentIcons, PromotionSentiment } from '@/models/promotions/PromotionCampaign';
import PromotionCommentData from '@/models/promotions/PromotionCommentData';

@Component({})
export default class PromotionCommentView extends Vue
{
    loadingUpdate: boolean = false;
    showModal: boolean = false;

    newComment: PromotionCommentData = { body: "", sentiment: PromotionSentiment.Abstain };

    @Prop() private comment!: PromotionComment;

    sentimentIcon(sentiment: PromotionSentiment)
    {
        return SentimentIcons[sentiment];
    }

    formatDate(date: Date): string
    {
        return formatDate(date);
    }

    backgroundColor()
    {
        return this.comment.isFromCurrentUser
            ? 'rgb(240, 210, 240)'
            : 'rgb(245, 245, 245)';
    }

    openModal()
    {
        this.showModal = true;
    }

    cancelModal()
    {
        this.showModal = false;
    }

    async saveComment()
    {
        this.loadingUpdate = true;

        this.loadingUpdate = false;

        this.cancelModal();
    }
}
</script>
