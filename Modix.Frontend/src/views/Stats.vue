<template>
    <div class="home">
        <div v-if="$store.state.modix.user">
            <div v-for="(guild, key) in $store.state.modix.guildInfo" v-bind:key="key">
                <HeroHeader :text="key">
                    Member Distribution
                </HeroHeader>
                <section class="section">
                    <PieChart :guildName="key" />
                </section>
            </div>
        </div>

        <section class="section" v-else>
            <div class="container">
                <h1 class="title is-3">Sorry, you need to log in to view stats.</h1>
            </div>
        </section>
    </div>
</template>

<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import PieChart from '@/components/PieChart.vue'; 
import store from "../app/Store";
import HeroHeader from "../components/HeroHeader.vue";

@Component({
    components:
    {
        PieChart,
        HeroHeader
    },
})
export default class Stats extends Vue
{
    created()
    {
        store.retrieveGuildInfo();
    }
}
</script>
