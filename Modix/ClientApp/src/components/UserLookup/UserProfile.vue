<template>
    <div class="message-narrow" v-if="user">
        <div class="message">
            <div class="message-header">
                <h1 class="message-title">
                    {{user.username}}#{{user.discriminator}}
                </h1>
            </div>
            <div class="message-body">
                <img v-if="user.avatarUrl" :src="user.avatarUrl" style="height: 128px; width: 128px; float: right; padding-top: 1em; padding-right: 1em;" />

                <div class="box">
                    <UserProfileSectionTitle :title="'User Information'" />
                    <UserProfileField :fieldName="'ID'" :fieldValue="user.id" />
                    <UserProfileField :fieldName="'Status'" :fieldValue="user.status" />
                    <UserProfileField :fieldName="'First seen'" :fieldValue="formatDate(user.firstSeen)" />
                    <UserProfileField :fieldName="'Last seen'" :fieldValue="formatDate(user.lastSeen)" />
                </div>

                <div class="box">
                    <UserProfileSectionTitle :title="'Guild Participation'" />
                    <UserProfileField :fieldName="'Rank'" :fieldValue="ordinalize(user.rank)" />
                    <UserProfileField :fieldName="'Last 7 days'" :fieldValue="toQuantity(user.last7DaysMessages, 'message', 'messages')" />
                    <UserProfileField :fieldName="'Last 30 days'" :fieldValue="toQuantity(user.last30DaysMessages, ' message', ' messages')" />
                    <UserProfileField :fieldName="'Average per day'" :fieldValue="toQuantity(Math.round(user.averageMessagesPerDay), ' message', ' messages')" />
                    <UserProfileField :fieldName="'Percentile'" :fieldValue="ordinalize(user.percentile)" />
                </div>

                <div class="box">
                    <UserProfileSectionTitle :title="'Member Information'" />
                    <UserProfileField :fieldName="'Nickname'" :fieldValue="user.nickname" v-if="user.nickname && user.nickname.length > 0" />
                    <UserProfileField :fieldName="'Created'" :fieldValue="formatDate(user.createdAt)" />
                    <UserProfileField :fieldName="'Joined'" :fieldValue="formatDate(user.joinedAt)" />
                    <UserProfileField :fieldName="'Roles'" :fieldValue="roles" />
                </div>

            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';
import * as _ from 'lodash';
import UserProfileSectionTitle from '@/components/UserLookup/UserProfileSectionTitle.vue';
import UserProfileField from '@/components/UserLookup/UserProfileField.vue';
import EphemeralUser from '@/models/EphemeralUser';
import { formatDate, ordinalize, toQuantity } from '@/app/Util'
import Role from '@/models/Role';
import store from "@/app/Store";

@Component({
    components:
    {
        UserProfileSectionTitle,
        UserProfileField,
    },
    methods:
    {
        formatDate,
        ordinalize,
        toQuantity,
    },
})
export default class UserProfile extends ModixComponent
{
    @Prop()
    user!: EphemeralUser | null;

    get roles(): string
    {
        return _.map(this.user!.roles, (role: Role): string => this.parseDiscordContent(`<@&${role.id}>`))
                .join(", ");
    }

    async mounted()
    {
        await store.retrieveRoles();
    }
}
</script>
