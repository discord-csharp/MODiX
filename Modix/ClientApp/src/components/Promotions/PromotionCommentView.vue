<template>

    <div class="comment" :style="{'background-color': backgroundColor()}">
        <span class="sentimentIcon" v-html="sentimentIcon(comment.sentiment)"></span>
        <span class="commentBody">
            {{comment.content}} <span class="date">{{formatDate(comment.createAction.created)}}</span>
        </span>
        <span class="button is-primary" :style="{'margin-left': '0.33em'}" v-if="comment.isFromCurrentUser && !isCampaignClosed" v-on:click="$emit('comment-edit-modal-opened', comment)">Edit</span>

    </div>

</template>

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
    @Prop() private isCampaignClosed!: boolean;

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
