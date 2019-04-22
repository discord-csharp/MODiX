<template>

    <div class="comment" v-bind:class="{'own': this.comment.isFromCurrentUser}">
        <span class="sentimentIcon" v-html="sentimentIcon(comment.sentiment)"></span>
        <span class="commentBody">
            {{comment.content}} <span class="date">{{formatDate(comment.createAction.created)}}</span>
        </span>
        <span class="button is-link edit" v-if="comment.isFromCurrentUser && !hidden" v-on:click="$emit('comment-edit-modal-opened', comment)">Edit</span>

    </div>

</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';
import { formatDate } from '@/app/Util';
import PromotionComment from '@/models/promotions/PromotionComment';
import { SentimentIcons, PromotionSentiment } from '@/models/promotions/PromotionCampaign';
import PromotionCommentData from '@/models/promotions/PromotionCommentData';

@Component({
    methods: { formatDate }
})
export default class PromotionCommentView extends ModixComponent
{
    loadingUpdate: boolean = false;
    showModal: boolean = false;

    newComment: PromotionCommentData = { body: "", sentiment: PromotionSentiment.Abstain };

    @Prop() private comment!: PromotionComment;
    @Prop() private hidden!: boolean;

    sentimentIcon(sentiment: PromotionSentiment)
    {
        return SentimentIcons[sentiment];
    }
}
</script>
