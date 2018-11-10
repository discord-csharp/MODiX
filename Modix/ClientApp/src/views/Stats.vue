<template>
    <div class="stats">
        <div class="container section">
            <h1 class="title">
                Member Distributions
            </h1>
        </div>

        <div v-for="(guild, key) in $store.state.modix.guildInfo" v-bind:key="key">
            <HeroHeader :text="key" />
            <section class="section">
                <PieChart :guildName="key" />
            </section>
        </div>
    </div>
</template>

<style lang="scss">


</style>


<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import PieChart from '@/components/PieChart.vue'; 
import store from "@/app/Store";
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
