<template>
    <div class="message-narrow userProfile" v-if="user">
        <div class="message">
            <div class="message-header">
                <h1 class="message-title">
                    {{user.username}}#{{user.discriminator}}
                </h1>
            </div>
            <div class="message-body">

                <div class="section-title">User Information</div>
                <div class="userInfoSection">
                    <div class="box info">
                        <UserProfileField fieldName="ID" :fieldValue="user.id" />
                        <UserProfileField fieldName="Status" :fieldValue="user.status" />
                        <UserProfileField fieldName="First seen" :fieldValue="formatDate(user.firstSeen)" default="Never" />
                        <UserProfileField fieldName="Last seen" :fieldValue="formatDate(user.lastSeen)" default="Never" />
                    </div>
                    <div class="box avatar" v-if="user.avatarUrl">
                        <img :src="user.avatarUrl" />
                    </div>
                </div>

                <template v-if="user.isGuildMember">
                    <div class="section-title">Guild Participation</div>
                    <div class="box">
                        <UserProfileField fieldName="Rank" :fieldValue="ordinalize(user.rank)" />
                        <UserProfileField fieldName="Last 7 days" :fieldValue="toQuantity(user.last7DaysMessages, 'message', 'messages')" />
                        <UserProfileField fieldName="Last 30 days" :fieldValue="toQuantity(user.last30DaysMessages, ' message', ' messages')" />
                        <UserProfileField fieldName="Average per day" :fieldValue="toQuantity(Math.round(user.averageMessagesPerDay), ' message', ' messages')" />
                        <UserProfileField fieldName="Percentile" :fieldValue="ordinalize(user.percentile)" />
                    </div>

                    <div class="section-title">Member Information</div>
                    <div class="box">
                        <UserProfileField fieldName="Nickname" :fieldValue="user.nickname" default="No Nickname" />
                        <UserProfileField fieldName="Created" :fieldValue="formatDate(user.createdAt)" />
                        <UserProfileField fieldName="Joined" :fieldValue="formatDate(user.joinedAt)" default="Never" />
                        <UserProfileField fieldName="Roles" :fieldValue="roles" allowHtml="true" />
                    </div>

                    <div :v-if="messages">
                        <div class="section-title">Messages by Channel</div>
                        <div class="box">
                            <PieChart :stats="messages" />
                        </div>
                    </div>
                </template>

            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Component, Prop, Watch } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';
import PieChart from '@/components/PieChart.vue';
import * as _ from 'lodash';
import UserProfileSectionTitle from '@/components/UserLookup/UserProfileSectionTitle.vue';
import UserProfileField from '@/components/UserLookup/UserProfileField.vue';
import EphemeralUser from '@/models/EphemeralUser';
import UserMessagePerChannelCount from '@/models/UserMessagePerChannelCount'
import { formatDate, ordinalize, toQuantity } from '@/app/Util'
import Role from '@/models/Role';
import store from "@/app/Store";
import UserService from '@/services/UserService';

@Component({
    components:
    {
        UserProfileField,
        PieChart
    },
    methods:
    {
        formatDate,
        ordinalize,
        toQuantity,
    }
})
export default class UserProfile extends ModixComponent
{
    @Prop()
    user!: EphemeralUser | null;
    
    messages: UserMessagePerChannelCount[] | null = null;

    get roles(): string
    {
        if (this.user == null || this.user.roles.length == 0)
        {
            return "<em>No roles assigned</em>";
        }

        return _.map(this.user.roles, (role: Role): string => this.parseDiscordContent(`<@&${role.id}>`))
                .join(", ");
    }

    @Watch('user')
    async setMessages(): Promise<void>
    {
        if (this.user && this.user.id)
        {
            this.messages = await UserService.getMessageCountPerChannel(this.user.id);
        }
    }

    async mounted()
    {
        await store.retrieveRoles();
    }
}
</script>
