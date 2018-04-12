<template>
    <div id="app">
        <div class="loader" :class="{'hidden': hasTriedAuth}">
            <div class="spinner"></div>
            <img class="spinnerCenter" src="./assets/icon.png" />
        </div>

        <div class="root" :class="{'shown': hasTriedAuth}">
            <div class="container">
                <NavBar />
            </div>

            <ErrorView />

            <router-view/>
        </div>
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
@import "~bulma/sass/elements/button";

@import "~bulma/sass/components/navbar";

$default-transition: 0.5s cubic-bezier(0.77, 0, 0.175, 1);

@keyframes fadeOut
{
    0%
    {
        opacity: 1;
    }
    99%
    {
        opacity: 0;
    }
    100%
    {
        opacity: 0;
        visibility: hidden;
        z-index: -999;

        animation: none;
    }
}

.root
{
    transition: all $default-transition;
    opacity: 0;

    &.shown
    {
        opacity: 1;
    }
}

.loader.hidden
{
    animation: fadeOut $default-transition forwards;
}

#app .spinner
{
    @include loader();

    position: absolute;

    border-width: 24px;
    top: 10%;
    left: calc(50% - 256px);
    
    width: 512px;
    height: 512px;

    border-bottom-color: $primary;
    border-left-color: $primary;

    animation-iteration-count: 3;
}

.spinnerCenter
{
    position: absolute;

    top: 20%;
    left: calc(50% - 150px);
}

html
{
    //Minireset.css is dumb
    overflow-y: auto !important;
}

</style>

<script lang="ts">
import Vue from 'vue';
import {Watch, Component, Prop} from 'vue-property-decorator';
import ErrorView from '@/components/ErrorView.vue';
import NavBar from '@/components/NavBar.vue';
import {toTitleCase} from './app/Util';
import store from './app/Store';

@Component({
    components:
    {
        ErrorView,
        NavBar
    },
})
export default class App extends Vue
{
    mounted()
    {
        this.onRouteChanged();
    }

    get hasTriedAuth()
    {
        return store.hasTriedAuth();
    }

    @Watch("$route")
    onRouteChanged()
    {
        if (this.$route.name)
        {
            document.title = "Modix - " + toTitleCase(this.$route.meta.title || this.$route.name);
        }
        else
        {
            document.title = "Modix";
        }
        
    }
}
</script>
