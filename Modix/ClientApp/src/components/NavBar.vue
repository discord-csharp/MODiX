<template>
    <nav class="navbar" role="navigation" aria-label="main navigation">

        <div class="navbar-brand">
            <div class="brand-start">
                <router-link class="navbar-item" to="/" title="Home" exact-active-class="is-active">
                    <img class="logo" :src="logoPath">
                </router-link>

                <a role="button" class="navbar-burger" :class="{'is-active': expanded}" @click="expanded = !expanded">
                    <span aria-hidden="true"></span>
                    <span aria-hidden="true"></span>
                    <span aria-hidden="true"></span>
                </a>
            </div>

            <div class="brand-end is-hidden-desktop">
                <MiniProfile class="navbar-item" />
            </div>
        </div>

        <div class="navbar-menu" :class="{'is-active': expanded}">
            <div class="navbar-start">
                <router-link class="navbar-item link" active-class="is-active"
                    v-for="route in routes" :key="route.routeData.name" :to="route.routeData.path" >
                    {{toTitleCase(route.title)}}
                </router-link>

                <router-link class="navbar-item link is-hidden-desktop" to="/config" active-class="is-active">
                    Configuration
                </router-link>
            </div>

            <div class="navbar-end is-hidden-touch">
                <router-link class="navbar-item link" to="/config" v-tooltip="'Configuration'" active-class="is-active">
                    &#128736;
                </router-link>
                <MiniProfile class="navbar-item" />
            </div>
        </div>

    </nav>
</template>

<style lang="scss" scoped>

@import "../styles/variables";
@import "~bulma/sass/base/_all";
@import "~bulma/sass/layout/_all";

@import "~bulma/sass/elements/container";

.logo
{
    
}

nav
{
    @include mobile()
    {
        
    }
}

.navbar-brand
{
    justify-content: space-between;
}

.brand-start
{
    display: flex;
}

.brand-end
{
    display: flex;
}

.small-logo
{
    filter: invert(100%);
}

nav
{
    background: $primary;
    box-shadow: 0px 2px 8px -4px $background;

    user-select: none;
}

.navbar-menu
{
    @include touch()
    {
        padding: 0;
    }
}

.navbar-burger
{
    color: white;

    &:hover
    {
        color: white;
    }
}

.navbar-brand
{
    a:not(.link)
    {
        margin: 0;
        padding: 0;
    }
    
    img
    {
        height: 52px;
        max-height: 52px;
    }
}

.navbar-item
{
    display: flex;
    align-items: center;

    background-color: $primary;

    color: white;

    &.is-active
    {
        color: $text;
        background-color: $body-background-color !important;

        font-weight: bold;
        pointer-events: none;
        box-shadow: 0px 6px 0px 0px $body-background-color;
    }

    &:hover
    {
        color: white;
        background-color: darken($primary, 3);
    }
}

</style>

<script lang="ts">
import Vue from 'vue';
import {Watch, Component, Prop} from 'vue-property-decorator';
import { Route } from 'vue-router';
import MiniProfile from '@/components/MiniProfile.vue';
import {toTitleCase} from '@/app/Util';
import * as _ from 'lodash';
import store from '@/app/Store';
import User from '@/models/User';
import ModixRoute from '@/app/ModixRoute';
import ApplicationConfiguration from '@/app/ApplicationConfiguration';

@Component({
    components:
    {
        MiniProfile
    },
})
export default class NavBar extends Vue
{
    expanded: boolean = false;

    get primaryLogo(): string
    {
        if (ApplicationConfiguration.isSpoopy)
        {
            return require("../assets/logo_colored_spoopy.png");
        }

        return require("../assets/logo_small_w.png");
    }

    get backgroundLogo(): string
    {
        if (ApplicationConfiguration.isSpoopy)
        {
            return require("../assets/logo_small_spoopy.png");
        }
        
        return require("../assets/logo_small.png");
    }

    get user(): User
    {
        return this.$store.state.modix.user;
    }

    get logoPath(): string
    {
        if (this.$route.name == "home")
        {
            return this.backgroundLogo;
        }

        return this.primaryLogo;
    }

    hasClaimsForRoute(route: ModixRoute): boolean
    {
        return store.userHasClaims(route.routeData.requiredClaims || []);
    }

    get routes(): ModixRoute[]
    {
        let allRoutes = _.map((<any>this.$router).options.routes, route => route.meta as ModixRoute);
        let showInNav = _.filter(allRoutes, (route: ModixRoute) => route.routeData.showInNavbar);
        let authFilter = _.filter(showInNav, (route: ModixRoute) => (route.requiresAuth ? store.isLoggedIn() : true));
        let claimFilter = _.filter(authFilter, (route: ModixRoute) => this.hasClaimsForRoute(route));

        return <any>claimFilter;
    }

    toTitleCase(input: string) 
    {
        return toTitleCase(input);
    }
}
</script>
