<template>
    <nav class="navbar" role="navigation" aria-label="main navigation">

        <div class="sidebar-left">
            <div class="navbar-brand">
                <router-link class="navbar-item" to="/" title="Home">
                    <img class="is-hidden-mobile" src="../assets/logo_small.png" width="112" height="28">
                    <img class="is-hidden-tablet" src="../assets/icon.png" width="28" height="28">
                </router-link>
            </div>

            <div class="navbar-item link" v-for="route in routes" :key="route.name">
                <router-link active-class="is-active" :to="route.path" >
                    {{toTitleCase(route.meta.title || route.name)}}
                </router-link>
            </div>

        </div>

        <MiniProfile class="navbar-item" />
    </nav>
</template>

<style lang="scss">

@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/base/_all";

nav
{
    display: flex;
    justify-content: space-between;
}

.sidebar-left
{
    display: flex;
    
    justify-content: flex-start;
}

.navbar-item
{
    display: flex;
    align-items: center;

    @include mobile()
    {
        font-size: 0.9em;
        padding: 0.25rem 0.7rem;
    }
}

.is-active
{
    font-weight: bold;
    pointer-events: none;
    color: black;
}

</style>

<script lang="ts">
import Vue from 'vue';
import {Watch, Component, Prop} from 'vue-property-decorator';
import { Route } from 'vue-router';
import MiniProfile from '@/components/MiniProfile.vue';
import {toTitleCase} from '../app/Util';
import * as _ from 'lodash';
import store from '@/app/Store';
import User from '@/models/User';

@Component({
    components:
    {
        MiniProfile
    },
})
export default class NavBar extends Vue
{
    get user(): User
    {
        return this.$store.state.modix.user;
    }

    get routes(): Route[]
    {
        let allRoutes = (<any>this.$router).options.routes as Route[];
        let showInNav = _.filter(allRoutes, route => route.meta.showNav);
        let authFilter = _.filter(showInNav, route => (route.meta.requiresAuth ? store.isLoggedIn() : true));

        return authFilter;
    }

    toTitleCase(input: string) 
    {
        return toTitleCase(input);
    }
}
</script>
