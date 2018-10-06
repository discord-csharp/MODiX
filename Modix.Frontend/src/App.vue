<template>
    <div id="app">
        <LoadingSpinner :visible="!hasTriedAuth" />

        <div class="root" :class="{'visible': hasTriedAuth}">
            <ErrorView />
            <NavBar />
            <router-view/>
        </div>
    </div>
</template>

<style lang="scss">

@import "styles/tooltip";
@import "styles/variables";

@import "~bulma/sass/base/_all";
@import "~bulma/sass/grid/_all";
@import "~bulma/sass/layout/_all";

@import "~bulma/sass/components/navbar";
@import "~bulma/sass/elements/title";
@import "~bulma/sass/elements/container";
@import "~bulma/sass/elements/button";
@import "~bulma/sass/components/level";

@import "styles/overrides";

@keyframes fadeIn
{
    0%
    {
        opacity: 0;
    }
    100%
    {
        opacity: 1;
    }
}

.root
{
    opacity: 0;

    &.visible
    {
        animation: fadeIn $default-transition forwards;
        animation-delay: 500ms;
    }
}

.root div > .section
{
    padding-top: 1.5rem;
    padding-bottom: 1.5rem;
}

.delete
{
    @include delete();
}
</style>

<script lang="ts">
import Vue from 'vue';
import {Watch, Component, Prop} from 'vue-property-decorator';
import ErrorView from '@/components/ErrorView.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import NavBar from '@/components/NavBar.vue';
import store from '@/app/Store';

@Component({
    components:
    {
        ErrorView,
        NavBar,
        LoadingSpinner
    },
})
export default class App extends Vue
{
    get hasTriedAuth()
    {
        return store.hasTriedAuth();
    }
}
</script>
