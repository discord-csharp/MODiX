<template>
    <div id="app">
        <div class="container">
            <nav class="navbar" role="navigation" aria-label="main navigation">

                <div class="sidebar-left">
                    <div class="navbar-brand">
                        <router-link class="navbar-item" to="/" title="Home">
                            <img class="is-hidden-mobile" src="./assets/logo_small.png" width="112" height="28">
                            <img class="is-hidden-tablet" src="./assets/icon.png" width="28" height="28">
                        </router-link>
                    </div>

                    <div class="navbar-item link" v-if="$store.state.modix.user">
                        <router-link active-class="is-active" to="/stats">Stats</router-link>
                    </div>

                    <!--
                    <div class="navbar-item link">
                        <router-link active-class="is-active" to="/pastes">Pastes</router-link>
                    </div>
                    -->
                    
                    <div class="navbar-item link">
                        <router-link active-class="is-active" to="/commands">Commands</router-link>
                    </div>
                </div>

                <div class="navbar-item profile">
                    <template v-if="$store.state.modix.user">
                        <img class="avatar-icon" :src="$store.state.modix.user.avatarUrl">
                        <p class="title is-4">
                            {{$store.state.modix.user.name}}
                            <a href="/api/logout" title="Log Out">👋</a>
                        </p>
                    </template>

                    <template v-else>
                        <p class="title is-4">
                            <a href="/api/login">Log In</a>
                        </p>
                    </template>
                </div>
            </nav>
        </div>

        <ErrorView />

        <router-view/>
    </div>
</template>

<style lang="scss">

@import "styles/variables";

@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/base/_all";
@import "~bulma/sass/grid/_all";
@import "~bulma/sass/layout/_all";

@import "~bulma/sass/elements/title";
@import "~bulma/sass/elements/container";
@import "~bulma/sass/elements/notification";
@import "~bulma/sass/elements/button";

@import "~bulma/sass/components/navbar";
@import "~bulma/sass/components/media";


.avatar-icon
{
    margin-right: 0.5em;
}

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

.profile .title.is-4
{
    @include mobile()
    {
        font-size: 1.25rem;
    }
}

</style>

<script lang="ts">
import Vue from 'vue';
import {Watch, Component, Prop} from 'vue-property-decorator';
import { Route } from 'vue-router';
import * as store from "./app/Store";
import User from '@/models/User';
import ErrorView from '@/components/ErrorView.vue';
import HeroHeader from '@/components/HeroHeader.vue';
import {toTitleCase} from './app/Util';

@Component({
    components:
    {
        ErrorView,
        HeroHeader
    },
})
export default class App extends Vue
{
    mounted()
    {
        store.updateUserInfo(this.$store);

        this.onRouteChanged();
    }

    @Watch("$route")
    onRouteChanged()
    {
        if (this.$route.name)
        {
            document.title = "Modix - " + toTitleCase(this.$route.name);
        }
        else
        {
            document.title = "Modix";
        }
        
    }
}
</script>
