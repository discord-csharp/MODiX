<template>
    <div>
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
                <span v-else-if="props.column.field == 'actions'">
                    <span class="level">
                        <button class="button is-link is-small level-left" :class="{'is-loading': isLoadingBatch}" v-on:click="showModalForBatch(props.row.batchId)">
                            Context
                        </button>
                    </span>
                </span>
                <span v-else>
                    {{props.formattedRow[props.column.field]}}
                </span>
            </template>

        </VueGoodTable>

        <div class="modal" :class="{'is-active': showBatchModal}">
            <div class="modal-background" @click="showBatchModal = !showBatchModal"></div>
            <div class="modal-card wide">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Batch Deletion Context
                    </p>

                    <button class="delete" aria-label="close" @click="showBatchModal = false"></button>
                </header>
                <section class="modal-card-body">
                    <BatchDeleteContext :deletedMessages="contextDeletedMessages"></BatchDeleteContext>
                </section>
                <footer class="modal-card-foot level">

                </footer>
            </div>
        </div>
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
import GeneralService from '@/services/GeneralService';
import BatchDeleteContext from '@/components/Logs/BatchDeleteContext.vue';
import DeletedMessageAbstraction from '@/models/logs/DeletedMessageAbstraction';
import ModixComponent from '@/components/ModixComponent.vue';
import { AxiosError } from 'axios';

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
        VueGoodTable,
        BatchDeleteContext
    }
})
export default class DeletedMessages extends ModixComponent
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

    isLoadingBatch: boolean = false;
    contextDeletedMessages: DeletedMessageAbstraction[] = [];
    showBatchModal: boolean = false;

    recordsPage: RecordsPage<DeletedMessage> = new RecordsPage<DeletedMessage>();
    tableParams: TableParameters = new TableParameters();

    channelCache: { [channel: string]: DesignatedChannelMapping } | null = null;

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
                formatFn: this.parseDiscordContent,
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
                formatFn: this.parseDiscordContent,
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
            },
            {
                label: 'Actions',
                field: 'actions',
                //hidden: !this.canPerformActions,
                width: '32px',
                sortable: false
            }
        ];
    }

    async refresh(): Promise<void>
    {
        if (this.isLoading) { return; }

        this.isLoading = true;

        this.recordsPage = await LogService.getDeletedMessages(this.tableParams);

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

        await store.retrieveChannels();
        this.channelCache = _.keyBy(this.$store.state.modix.channels, channel => channel.id);
    }

    async onPageChange(params: any): Promise<void>
    {
        this.tableParams.page = params.currentPage - 1;

        await this.refresh();
    }

    async onSortChange(params: any): Promise<void>
    {
        this.tableParams.sort.field = params[0].field;
        this.tableParams.sort.direction = getSortDirection(params[0].type);

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

    async showModalForBatch(batchId: number)
    {
        this.isLoadingBatch = true;

        try
        {
            this.contextDeletedMessages = await LogService.getDeletionContext(batchId);
            this.showBatchModal = true;
        }
        catch (err)
        {
            let error = err as AxiosError;
            store.pushErrorMessage(error.response!.data);
        }
        finally
        {
            this.isLoadingBatch = false;
        }
    }
}
</script>
