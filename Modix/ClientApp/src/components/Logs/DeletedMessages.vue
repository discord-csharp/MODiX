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
                              v-bind:paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped deleted-messages"
                              mode="remote" v-bind:totalRows="recordsPage.filteredRecordCount" v-on:on-page-change="onPageChange"
                              v-on:on-sort-change="onSortChange" v-on:on-column-filter="onColumnFilter" v-on:on-per-page-change="onPerPageChange">

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

<script lang="ts">
    import * as _ from 'lodash';
    import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
    import store from "@/app/Store";
    import { VueGoodTable } from 'vue-good-table';
    import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
    import RecordsPage from '@/models/RecordsPage';
    import DeletedMessage from '@/models/logs/DeletedMessage';
    import LogService from '@/services/LogService';
    import TableParameters from '@/models/TableParameters';
    import { SortDirection } from '@/models/SortDirection';

    const messageResolvingRegex = /<#(\d+)>/gm;

    function getSortDirection(direction: string): SortDirection
    {
        return (direction == "asc")
            ? SortDirection.Ascending
            : SortDirection.Descending;
    }

    @Component({
        components:
        {
            VueGoodTable
        }
    })
    export default class DeletedMessages extends Vue
    {
        paginationOptions: any =
        {
            enabled: true,
            perPage: 10,
            mode: 'pages'
        };

        sortOptions: any =
        {
            enabled: true,
            initialSortBy: { field: 'created', type: 'desc' }
        };
        
        isLoading: boolean = false;

        recordsPage: RecordsPage<DeletedMessage> = new RecordsPage<DeletedMessage>();
        tableParams: TableParameters = new TableParameters();

        channelCache: { [channel: string]: DesignatedChannelMapping } | null = null;

        resolveMentions(description: string): string
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

        staticFilters: { [field: string]: string } = { channel: "", author: "", createdBy: "", content: "", reason: "", batchId: "" };

        get mappedColumns(): Array<any>
        {
            return [
                {
                    label: 'Channel',
                    field: 'channel',
                    width: '10%',
                    filterOptions:
                    {
                        enabled: true,
                        placeholder: "Filter",
                        filterValue: this.staticFilters["channel"]
                    }
                },
                {
                    label: 'Author',
                    field: 'author',
                    width: '10%',
                    filterOptions:
                    {
                        enabled: true,
                        placeholder: "Filter",
                        filterValue: this.staticFilters["author"]
                    }
                },
                {
                    label: 'Deleted On',
                    field: 'created',
                    type: 'date',
                    width: '10%',
                    dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                    dateOutputFormat: 'MM/DD/YY, h:mm:ss a'
                },
                {
                    label: 'Deleted By',
                    field: 'createdBy',
                    width: '10%',
                    filterOptions:
                    {
                        enabled: true,
                        placeholder: "Filter",
                        filterValue: this.staticFilters["createdBy"]
                    }
                },
                {
                    label: 'Content',
                    field: 'content',
                    width: '24%',
                    formatFn: this.resolveMentions,
                    html: true,
                    filterOptions:
                    {
                        enabled: true,
                        placeholder: "Filter",
                        filterValue: this.staticFilters["content"]
                    }
                },
                {
                    label: 'Reason',
                    field: 'reason',
                    width: '24%',
                    formatFn: this.resolveMentions,
                    html: true,
                    filterOptions:
                    {
                        enabled: true,
                        placeholder: "Filter",
                        filterValue: this.staticFilters["reason"]
                    }
                },
                {
                    label: 'Batch ID',
                    field: 'batchId',
                    type: 'number',
                    width: '10%',
                    filterOptions:
                    {
                        enabled: true,
                        placeholder: "#",
                        filterValue: this.staticFilters["batchId"]
                    }
                }
            ];
        }

        async refresh(): Promise<void>
        {
            this.isLoading = true;

            this.recordsPage = await LogService.getDeletedMessages(this.tableParams);

            await store.retrieveChannels();
            this.channelCache = _.keyBy(this.$store.state.modix.channels, channel => channel.id);

            this.isLoading = false;
        }

        applyFilters(): void
        {
            let urlParams = new URLSearchParams(window.location.search);

            for (let i = 0; i < this.mappedColumns.length; i++)
            {
                let currentField: string = this.mappedColumns[i].field;

                if (urlParams.has(currentField.toLowerCase()))
                {
                    this.tableParams.filters.push({ field: currentField, value: urlParams.get(currentField.toLowerCase()) || "" });
                    this.staticFilters[currentField] = urlParams.get(currentField.toLowerCase()) || "";
                }
            }
        }

        async created(): Promise<void>
        {
            this.applyFilters();
            await this.refresh();
        }

        async onPageChange(params: any): Promise<void>
        {
            this.tableParams.page = params.currentPage - 1;

            await this.refresh();
        }

        async onSortChange(params: any): Promise<void>
        {
            this.tableParams.sort.field = this.mappedColumns[params.columnIndex].field;
            this.tableParams.sort.direction = getSortDirection(params.sortType);

            await this.refresh();
        }

        async onColumnFilter(params: any): Promise<void>
        {
            this.tableParams.filters = [];

            for (let prop in params.columnFilters)
            {
                this.tableParams.filters.push({ field: prop, value: params.columnFilters[prop] })
            }

            await this.refresh();
        }

        async onPerPageChange(params: any): Promise<void>
        {
            this.tableParams.perPage = (params.currentPerPage == "all")
                ? 2147483647
                : params.currentPerPage;

            await this.refresh();
        }
    }
</script>
