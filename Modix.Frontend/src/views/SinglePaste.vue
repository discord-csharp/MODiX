<template>
    <div>
        <HeroHeader text="Code Paste">
            <template v-if="currentPaste">
                from <strong>{{currentPaste.creatorUsername}}</strong> in <strong>#{{currentPaste.channelName}}</strong>
                <span class="has-text-grey-light date">{{getFormattedDate(currentPaste.created)}}</span>
            </template>
        </HeroHeader>

        <section class="section">
            <div class="container">
                <PasteView v-bind:includeHeader="false" v-bind:paste="currentPaste" />
            </div>
        </section>
    </div>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
//import * as store from "../app/Store";
import UserCodePaste from '@/models/UserCodePaste';
import PasteView from '../components/PasteView.vue';
import {formatPasteDate} from '../app/Util';
import { Route } from 'vue-router';

@Component({
    components:
    {
        PasteView,
        HeroHeader
    },
})
export default class Pastes extends Vue
{
    get pasteId(): number
    {
        return Number.parseInt(this.$route.params.routePasteId);
    }

    get currentPaste(): UserCodePaste
    {
        return this.$store.state.modix.currentPaste;
    }

    getFormattedDate(date: Date): string
    {
        return formatPasteDate(date);
    }

    @Watch('$route')
    routeChanged()
    {
        if (!Number.isNaN(this.pasteId))
        {
            //TODO: Make work
            //store.getPaste(this.$store, this.pasteId);
        }
    }

    mounted()
    {
        this.routeChanged();
    }

    created()
    {
        
    }
}
</script>
