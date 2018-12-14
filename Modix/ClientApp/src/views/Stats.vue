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
                    <div class="first">
                        🥇 <strong>{{entryAt(0).name}}</strong><small>{{entryAt(0).count}} messages</small>
                    </div>
                    <div class="second">
                        🥈 <strong>{{entryAt(1).name}}</strong><small>{{entryAt(1).count}} messages</small>
                    </div>
                    <div class="third">
                        🥉 <strong>{{entryAt(2).name}}</strong><small>{{entryAt(2).count}} messages</small>
                    </div>

                    <ol class="remaining" start="4">
                        <li v-for="entry in entriesStartingAt(3)" v-bind:key="entry.name">
                            {{entry.name}}&nbsp;<small>{{entry.count}} messages</small>
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
    margin-left: 1.5em;
    margin-top: 0.5em;
}

</style>


<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import PieChart from '@/components/PieChart.vue';
import store from "@/app/Store";
import HeroHeader from "../components/HeroHeader.vue";
import GuildStatApiData from '@/models/GuildStatApiData';
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
    private index: number = 1;

    get guildName(): string
    {
        let currentGuild = store.currentGuild();

        if (currentGuild != null)
        {
            return currentGuild.name;
        }

        return "Unknown Guild";
    }

    private entryAt(index: number): {name: string, count: number}
    {
        if (!this.stats) { return { name: "", count: 0} };

        var keys = Object.keys( this.stats.topUserMessageCounts );
        let name = keys[index];
        let count = this.stats.topUserMessageCounts[name];

        return { name, count }
    }

    private entriesStartingAt(index: number): {name: string, count: number}[]
    {
        if (!this.stats) { return [] };

        var keys = Object.keys( this.stats.topUserMessageCounts );

        if (index >= keys.length) { return [] };

        keys = keys.slice(index, keys.length);

        return _.map(keys, key => {
            let count = this.stats!.topUserMessageCounts[key];
            return { name: key, count };
        });
    }

    async mounted()
    {
        this.stats = await GeneralService.getGuildStats();
    }
}
</script>
