<template>
    <div class="stats">

        <div class="container section">
            <h1 class="title">
                Statistics for {{guildName}}
            </h1>
        </div>

        <div class="columns is-multiline" v-if="stats">

            <div class="column is-full is-half-desktop">
                <HeroHeader text="Role Distribution" />

                <section class="section">
                    <PieChart :stats="stats.guildRoleCounts" />
                </section>
            </div>

            <div class="column is-full is-half-desktop">
                <HeroHeader text="Most Active Users">
                    of the last 30 days
                </HeroHeader>
                <section class="section userList">
                    <ol style="list-style-type: none">
                        <li v-for="entry in stats.topUserMessageCounts" :key="entry.rank">
                            <div class="first" v-if="entry.rank == 1">
                                🥇 <strong>{{getEntryName(entry)}}</strong><small>{{entry.messageCount}} messages</small>
                            </div>
                            <div class="second" v-else-if="entry.rank == 2">
                                🥈 <strong>{{getEntryName(entry)}}</strong><small>{{entry.messageCount}} messages</small>
                            </div>
                            <div class="third" v-else-if="entry.rank == 3">
                                🥉 <strong>{{getEntryName(entry)}}</strong><small>{{entry.messageCount}} messages</small>
                            </div>
                            <div class="remaining" v-else>
                                {{entry.rank}}.&nbsp;{{getEntryName(entry)}}&nbsp;<small>{{entry.messageCount}} messages</small>
                            </div>
                        </li>
                    </ol>
                </section>
            </div>

        </div>

        <section class="section" v-else>
            <div class="button is-loading statLoader"></div>
        </section>
    </div>
</template>

<style lang="scss">

@import "~bulma/sass/utilities/_all";

.statLoader
{
    width: 100%;
}

.userList
{
    font-size: 1.5em;
    padding: 1em;

    .first strong
    {
        font-size: 1.2em;
    }

    .second strong
    {
        font-size: 1.1em;
    }

    .third
    {
        margin-bottom: 0.5em;
    }

    strong
    {
        margin-right: 0.33em;
    }

    small
    {
        font-size: 0.75em;
    }
}

.userList div small
{
    @include mobile()
    {
        display: block;
    }
}

.remaining
{
    font-size: 0.8em;
    margin-left: 0.5em;
}

</style>


<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import PieChart from '@/components/PieChart.vue';
import store from "@/app/Store";
import HeroHeader from "../components/HeroHeader.vue";
import GuildStatApiData, { PerUserMessageCount } from '@/models/GuildStatApiData';
import GeneralService from '@/services/GeneralService';
import ModixComponent from '@/components/ModixComponent.vue';
import * as _ from 'lodash';

@Component({
    components:
    {
        PieChart,
        HeroHeader
    },
})
export default class Stats extends ModixComponent
{
    private stats: GuildStatApiData | null = null;

    get guildName(): string
    {
        let currentGuild = store.currentGuild();

        if (currentGuild != null)
        {
            return currentGuild.name;
        }

        return "Unknown Guild";
    }

    getEntryName(entry: PerUserMessageCount): string
    {
        return entry.username + "#" + entry.discriminator;
    }

    async mounted()
    {
        this.stats = await GeneralService.getGuildStats();
    }
}
</script>
