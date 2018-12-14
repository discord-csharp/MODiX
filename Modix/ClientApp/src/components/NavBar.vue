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
import themeAsset from '@/app/ThemeConfiguration';

@Component({
    components:
    {
        MiniProfile
    },
})
export default class NavBar extends Vue
{
    expanded: boolean = false;

    private primaryLogo: string = themeAsset("logo_small.png");
    private backgroundLogo: string = themeAsset("logo_small_selected.png");

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
