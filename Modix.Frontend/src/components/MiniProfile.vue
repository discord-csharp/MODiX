<template>
    <div class="profile">
        <template v-if="user && user.userId">
            <img class="avatar-icon" :src="user.avatarUrl">
            <p class="title is-4">
                <span class="username">{{user.name}}</span>
                <a href="/api/logout" title="Log Out">👋</a>
            </p>
        </template>

        <template v-else>
            <p class="title is-4">
                <a href="/api/login">Log In</a>
            </p>
        </template>
    </div>
</template>

<style lang="scss">
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/base/_all";
@import "~bulma/sass/components/media";

.avatar-icon
{
    margin-right: 0.5em;
}

.username
{
    margin-right: 0.2em;

    @include mobile()
    {
        display: none;
    }
}
</style>

<script lang="ts">
import Vue from 'vue';
import User from '@/models/User';
import modix from "../app/Store";
import { Component } from 'vue-property-decorator';

@Component({})
export default class MiniProfile extends Vue
{
    get user(): User
    {
        return this.$store.state.modix.user;
    }

    async created()
    {
        //await modix.retrieveUserInfo();
    }
}
</script>
