<template>
    <div>
        <HeroHeader text="Code Pastes">
            Most Recent
        </HeroHeader>

        <section class="section">
            <div class="container">
                <template>
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

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import store from "../app/Store";
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
    getFormattedDate(date: Date): string
    {
        return formatPasteDate(date);
    }

    async created()
    {
        await store.retrievePastes();
    }
}
</script>
