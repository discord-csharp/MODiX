<template>
    <div>
        <section class="section">
            <div class="container">

                <div class="level is-mobile">
                    <div class="level-left">
                        <button class="button" v-on:click="refresh()" v-bind:class="{ 'is-loading': isLoading }">Refresh</button>
                    </div>
                </div>

                <VueGoodTable v-bind:columns="mappedColumns" v-bind:rows="recordsPage.records" v-bind:sortOptions="sortOptions"
                              v-bind:paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped">

                    <template slot="table-row" slot-scope="props">
                        <span v-if="props.column.html" v-html="props.formattedRow[props.column.field]" />
                        <span v-else-if="props.column.field == 'channel'">
                            #{{props.formattedRow[props.column.field]}}
                        </span>
                        <span v-else>
                            {{props.formattedRow[props.column.field]}}
                        </span>
                    </template>

                </VueGoodTable>
            </div>
        </section>
    </div>
</template>

<style lang="scss">

@import "../../styles/variables";
@import "~vue-good-table/dist/vue-good-table.css";

.vgt-table.bordered
{
    font-size: 14px;

    select
    {
        font-size: 12px;
    }

    th
    {
        text-align: center;
        padding: 0.33em;
    }
}

.vgt-responsive
{
    @include fullwidth-desktop();
}

.vgt-input, .vgt-select
{
    padding: 0px 4px;
    height: 28px;
}

@include mobile()
{
    .vgt-table.bordered
    {
        font-size: initial;

        select
        {
            font-size: initial;
        }
    }
}

.channel
{
    font-weight: bold;
}

.pre
{
    white-space: pre-line;
}

</style>

<style scoped lang="scss">

@import "../../styles/variables";
@import "~bulma/sass/components/modal";
@import "~bulma/sass/elements/notification";
@import "~bulma/sass/elements/form";

.typeCell
{
    display: block;
    white-space: nowrap;
}
</style>

<script lang="ts">
import * as _ from 'lodash';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import store from "@/app/Store";
import { Route } from 'vue-router';
import { VueGoodTable } from 'vue-good-table';
import { InfractionType } from '@/models/infractions/InfractionType'
import GuildUserIdentity from '@/models/core/GuildUserIdentity'

import GeneralService from '@/services/GeneralService';
import InfractionSummary from '@/models/infractions/InfractionSummary';
import {config, setConfig} from '@/models/PersistentConfig';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import GuildInfoResult from '@/models/GuildInfoResult';
import RecordsPage from '@/models/RecordsPage';
import DeletedMessage from '@/models/log/DeletedMessage';
import LogService from '@/services/LogService';

const messageResolvingRegex = /<#(\d+)>/gm;

@Component({
    components:
    {
        HeroHeader,
        VueGoodTable
    }
})
export default class DeletedMessages extends Vue
{
    paginationOptions: any = 
    {
        enabled: true,
        perPage: 10
    };

    sortOptions: any = 
    {
        enabled: true,
        initialSortBy: {field: 'created', type: 'desc'}
    };

    showState: boolean = false;
    showDeleted: boolean = false;

    showModal: boolean = false;
    message: string | null = null;
    loadError: string | null = null;
    importGuildId: number | null = null;
    isLoading: boolean = false;

    channelCache: {[channel: string]: DesignatedChannelMapping} | null = null;

    resolveMentions(description: string)
    {
        let replaced = description;

        if (this.channelCache)
        {
            replaced = description.replace(messageResolvingRegex, (sub, args: string) =>
            {
                let found = this.channelCache![args].name;

                if (!found)
                {
                    found = args;
                }

                return `<span class='channel'>#${found}</span>`;
            });
        }

        return `<span class='pre'>${replaced}</span>`;
    }

    staticFilters: {[field: string]: string} = {subject: "", creator: "", id: ""};

    get mappedColumns(): Array<any>
    {
        return [
            {
                label: 'Channel',
                field: 'channel',
                sortFn: (x: string, y: string) => (x < y ? -1 : (x > y ? 1 : 0)),
                width: '60px',
                filterOptions:
                {
                    enabled: true,
                    filterFn: (channel: string, filter: string) => channel.includes(filter),
                    placeholder: "Filter"
                }
            },
            {
                label: 'Author',
                field: 'author',
                sortFn: (x: string, y: string) => (x.toLowerCase() < y.toLowerCase() ? -1 : (x.toLowerCase() > y.toLowerCase() ? 1 : 0)),
                width: '60px',
                filterOptions:
                {
                    enabled: true,
                    placeholder: "Filter"
                }
            },
            {
                label: 'Deleted On',
                field: 'created',
                type: 'date',
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a',
                width: '120px'
            },
            {
                label: 'Deleted By',
                field: 'createdBy',
                width: '60px',
                filterOptions:
                {
                    enabled: true,
                    placeholder: "Filter"
                }
            },
            {
                label: 'Content',
                field: 'content',
                formatFn: this.resolveMentions,
                html: true,
                width: '240px',
            },
            {
                label: 'Reason',
                field: 'reason',
                formatFn: this.resolveMentions,
                html: true,
                width: '240px',
            },
            {
                label: 'Batch ID',
                field: 'batchId',
                type: 'number',
                width: '60px',
            }
        ];
    }

    async refresh()
    {
        this.isLoading = true;

        this.recordsPage = await LogService.getDeletedMessages(10, 0);

        await store.retrieveChannels();
        this.channelCache = _.keyBy(this.$store.state.modix.channels, channel => channel.id);

        this.isLoading = false;
    }

    applyFilters()
    {
        let urlParams = new URLSearchParams(window.location.search);

        for (let i = 0; i < this.mappedColumns.length; i++)
        {
            let currentField: string = this.mappedColumns[i].field;

            if (urlParams.has(currentField))
            {
                this.staticFilters[currentField] = urlParams.get(currentField) || "";
            }
        }

        console.log(this.mappedColumns);
    }

    recordsPage: RecordsPage<DeletedMessage> = new RecordsPage<DeletedMessage>();

    async created()
    {
        await this.refresh();
        this.applyFilters();
    }

    @Watch('showState')
    inactiveChanged()
    {
        setConfig(conf => conf.showInfractionState = this.showState);
    }

    @Watch('showDeleted')
    deletedChanged()
    {
        setConfig(conf => conf.showDeletedInfractions = this.showDeleted);
    }
}
</script>
