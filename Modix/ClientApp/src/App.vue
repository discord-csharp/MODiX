<template>
    <div id="app">
        <LoadingSpinner :visible="!ready" />

        <div class="root" :class="{'visible': ready}">
            <ErrorView />
            <NavBar />
            <router-view/>
        </div>
    </div>
</template>

<style>

#app
{
    opacity: 0;
    transition: all 0.05s ease-in-out;
}

</style>

<script lang="ts">
import {Watch, Component, Prop} from 'vue-property-decorator';
import ErrorView from '@/components/ErrorView.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import NavBar from '@/components/NavBar.vue';
import store from '@/app/Store';
import ModixComponent from '@/components/ModixComponent.vue';
import { config, Theme, themeContext } from '@/models/PersistentConfig';

@Component({
    components:
    {
        ErrorView,
        NavBar,
        LoadingSpinner
    },
})
export default class App extends ModixComponent
{
    private initialLoadingComplete: boolean = false;

    get ready()
    {
        return this.initialLoadingComplete && this.state.user;
    }

    async mounted()
    {
        await store.retrieveUserInfo();

        if (store.isLoggedIn())
        {
            await store.retrieveGuilds();
        }

        this.initialLoadingComplete = true;

        themeContext("./" + config().theme.toLowerCase() + ".scss");

        console.log("Using theme: " + config().theme);
    }
}
</script>
