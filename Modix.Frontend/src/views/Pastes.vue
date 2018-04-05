<template>
    <div>
        <HeroHeader :text="(currentPaste ? 'Code Paste' : 'Code Pastes')">
            <template v-if="currentPaste">
                from <strong>{{currentPaste.creatorUsername}}</strong> in <strong>#{{currentPaste.channelName}}</strong>
                <span class="has-text-grey-light date">{{getFormattedDate(currentPaste.created)}}</span>
            </template>
            <template v-else>
                Most Recent
            </template>
        </HeroHeader>

        <section class="section">
            <div class="container">
                <PasteView v-if="currentPaste" v-bind:includeHeader="false" v-bind:paste="currentPaste" />

                <template v-else>
                    <template v-if="$store.state.modix.pastes.length > 0">
                        <PasteView v-for="paste in $store.state.modix.pastes" v-bind:key="paste.id" v-bind:paste="paste" />
                    </template>
                    <template v-else>
                        <h1 class="title is-3">No pastes available!</h1>
                    </template>
                </template>
            </div>
        </section>
    </div>
</template>

<style lang="scss">

.date
{
    margin-left: 0.5em;
    font-size: 0.85em;
}

.paste
{
    margin-bottom: 2em;
}

</style>


<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import * as store from "../app/Store";
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
            store.getPaste(this.$store, this.pasteId);
        }
        else
        {
            store.setCurrentPaste(this.$store, null);
        }
    }

    mounted()
    {
        this.routeChanged();
    }

    created()
    {
        store.updatePastes(this.$store);
    }
}
</script>
