<template>
    <div class="batchDeleteContext">
        <div class="startDate">
            Starting <strong>{{startDate}}</strong>
        </div>

        <div class="deletedMessage" :id="msg.messageId" v-for="msg in deletedMessages" :key="msg.messageId" :class="{'deleted': msg.sentTime == null}"
            :title="msg.sentTime == null ? 'This was deleted' : msg.sentTime">
            <span class="front">
                <span class="sentTime">
                    <template v-if="msg.sentTime != null">
                        <a :href="msg.url" target="_blank">
                            {{formatDate(msg.sentTime)}}
                        </a>
                    </template>
                    <template v-else>
                        🚫
                    </template>
                </span>
                <span class="username">{{msg.username}}</span>
            </span>
            <span class="content">
                <template v-if="msg.content">{{msg.content}}</template>
                <template v-else>
                    <em class="noContent">No Content</em>
                </template>
            </span>
        </div>
    </div>
</template>

<script lang="ts">
import * as _ from 'lodash';
import * as dateformat from "dateformat";
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import store from "@/app/Store";
import LogService from '@/services/LogService';
import DeletedMessageAbstraction from '@/models/logs/DeletedMessageAbstraction';

@Component({
    components:
    {

    }
})
export default class BatchDeleteContext extends Vue
{
    @Prop({required: true, default: []})
    deletedMessages!: DeletedMessageAbstraction [];

    formatDate(date: Date)
    {
        return dateformat(date, "hh:MM");
    }

    get startDate()
    {
        if (this.deletedMessages.length > 0)
        {
            return dateformat(this.deletedMessages[0].sentTime, "dddd, mmmm dS, yyyy");
        }

        return "No messages";
    }

    mounted()
    {
        let firstDeleted = this.deletedMessages.find(msg => msg.sentTime == null);

        if (firstDeleted)
        {
            location.hash = "#" + firstDeleted.messageId;
        }
    }
}
</script>
