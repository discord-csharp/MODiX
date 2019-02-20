<template>
    <div class="modal" v-bind:class="{'is-active': showUpdateModal}">
        <div class="modal-background" v-on:click="showUpdateModal = false" />
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Edit Comment</p>

                <button class="delete" aria-label="close" v-on:click="showUpdateModal = false"></button>
            </header>

            <section class="modal-card-body control">
                <div class="field has-addons">
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
                        <input class="input" type="text" v-model="newComment.body" placeholder="Make a comment...">
                    </p>

                </div>

                <p class="help is-danger" v-if="error">{{error}}</p>
            </section>

            <footer class="modal-card-foot level">
                <div class="level-left">
                    <button class="button" v-on:click="showUpdateModal = false">Cancel</button>
                </div>
                <div class="level-right">
                    <button class="button is-success" v-bind:class="{'is-loading': loadingCommentUpdate}"
                            v-on:click="updateComment()" v-bind:disabled="!allowUpdate">Update</button>
                </div>
            </footer>
        </div>
    </div>

</template>

<style scoped lang="scss">
    .statusIcon
    {
        display: inline-block;
        width: 1.3em;
    }
</style>


<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';

import PromotionComment from '@/models/promotions/PromotionComment';
import * as _ from 'lodash';
import PromotionCommentData from '@/models/promotions/PromotionCommentData';
import { SentimentIcons, PromotionSentiment } from '@/models/promotions/PromotionCampaign';
import PromotionService from '@/services/PromotionService';

@Component({})
export default class PromotionCommentEditModal extends Vue
{
    @Prop() public comment!: PromotionComment;
    @Prop({default: false}) public showUpdateModal!: boolean;

    loadingCommentUpdate: boolean = false;
    newComment: PromotionCommentData = { body: "", sentiment: PromotionSentiment.Abstain };
    error: string = "";

    @Watch('showUpdateModal')
    modalShownOrUnshown()
    {
        if (this.showUpdateModal)
        {
            this.newComment = {
                body: this.comment.content,
                sentiment: this.comment.sentiment
            };

            this.error = "";
        }
        else
        {
            this.$emit('comment-edit-modal-closed')
        }
    }

    get allowUpdate(): boolean
    {
        return this.newComment.body.length > 3;
    }

    async updateComment()
    {
        this.loadingCommentUpdate = true;

        try
        {
            await PromotionService.updateComment(this.comment, this.newComment);
        }
        catch (err)
        {
            this.error = err.response.data;
            console.log(err);
            return;
        }
        finally
        {
            this.loadingCommentUpdate = false;
        }

        this.$emit('comment-edited');
        this.$emit('comment-edit-modal-closed')
    }

    sentimentIcon(sentiment: PromotionSentiment)
    {
        return SentimentIcons[sentiment];
    }
}
</script>
