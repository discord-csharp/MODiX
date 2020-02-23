<template>
    <section class="container section">
        <h1 class="title">User Lookup</h1>
        <div>
            <UserSearch @userSelected="loading = true; selectedUser = $event" @userLoaded="selectedUser = $event; loading = false" />
        </div>
        <template v-if="loading">
            <h1 class="subtitle">Loading details for {{selectedUser.name}}...</h1>
            <div class="sk-cube-grid">
                <div class="sk-cube sk-cube1"></div>
                <div class="sk-cube sk-cube2"></div>
                <div class="sk-cube sk-cube3"></div>
                <div class="sk-cube sk-cube4"></div>
                <div class="sk-cube sk-cube5"></div>
                <div class="sk-cube sk-cube6"></div>
                <div class="sk-cube sk-cube7"></div>
                <div class="sk-cube sk-cube8"></div>
                <div class="sk-cube sk-cube9"></div>
            </div>
        </template>

        <div>
            <UserProfile v-if="selectedUser && !loading" :user="selectedUser" />
        </div>
    </section>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';
import UserSearch from '@/components/UserLookup/UserSearch.vue';
import UserProfile from '@/components/UserLookup/UserProfile.vue';
import * as _ from 'lodash';
import EphemeralUser from '@/models/EphemeralUser';
import UserService from '@/services/UserService';

@Component(
{
    components:
    {
        UserSearch,
        UserProfile,
    }
})
export default class UserLookup extends ModixComponent
{
    selectedUser: EphemeralUser | null = null;
    loading: boolean = false;

    async mounted()
    {
        this.selectedUser = (await UserService.getUserInformation(this.$store.state.modix.user.userId))!;
    }
}
</script>
