<template>
<div>
    <VueGoodTable :columns="mappedColumns" :rows="mappedRows" :sortOptions="sortOptions" ref="table"
                  :paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped"
                  mode="remote" :totalRows="recordsPage.filteredRecordCount" @on-page-change="onPageChange" :is-loading="false"
                  @on-sort-change="onSortChange" @on-column-filter="onColumnFilter" @on-per-page-change="onPerPageChange">

        <template slot="table-row" slot-scope="props">
            <span v-if="props.column.field == 'type'">
                <span :title="props.formattedRow[props.column.field]" class="typeCell"
                        v-html="emojiFor(props.formattedRow[props.column.field]) + ' ' + props.formattedRow[props.column.field]">
                </span>
            </span>
            <span v-else-if="props.column.field == 'reason'" v-html="props.formattedRow[props.column.field]">
            </span>
            <span v-else-if="props.column.field == 'actions'">
                <span class="level">
                    <button class="button is-link is-small level-left" v-show="props.row.canDelete"
                        v-on:click="onInfractionDelete(props.row)">Delete</button>

                    <button class="button is-link is-small level-right" v-if="props.row.canRescind"
                        v-on:click="onInfractionRescind(props.row)">Rescind</button>
                </span>
            </span>
            <span v-else>{{props.formattedRow[props.column.field]}}</span>
        </template>

        <div slot="emptystate" class="vgt-center-align vgt-text-disabled">
            No infractions found
        </div>

    </VueGoodTable>
</div>
</template>

<script lang="ts">
import * as _ from 'lodash';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import TinyUserView from '@/components/TinyUserView.vue';
import Autocomplete from '@/components/Autocomplete.vue';
import ConfirmationModal from '@/components/ConfirmationModal.vue';
import store from "@/app/Store";
import { VueGoodTable } from 'vue-good-table';
import GuildUserIdentity, { getFullUsername } from '@/models/core/GuildUserIdentity'
import User from '@/models/User';
import GeneralService from '@/services/GeneralService';
import InfractionSummary from '@/models/infractions/InfractionSummary';
import {config, setConfig} from '@/models/PersistentConfig';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import InfractionCreationData from '@/models/infractions/InfractionCreationData';
import RecordsPage from '@/models/RecordsPage';
import TableParameters from '@/models/TableParameters';
import { SortDirection } from '@/models/SortDirection';
import ModixComponent from '@/components/ModixComponent.vue';
import { InfractionType, infractionTypeToEmoji } from '@/models/infractions/InfractionType';

function getSortDirection(direction: string): SortDirection
{
    return (direction == "asc")
        ? SortDirection.Ascending
        : SortDirection.Descending;
}

const guildUserFormat = (subject: GuildUserIdentity) => getFullUsername(subject);

@Component({
    components:
    {
        VueGoodTable
    }
})
export default class InfractionTable extends ModixComponent
{
    @Prop({required: true})
    recordsPage!: RecordsPage<InfractionSummary>;

    @Prop({required: false, default: false})
    showActions!: boolean;

    @Prop({required: false, default: false})
    showState!: boolean;

    @Prop({required: false, default: false})
    showDeleted!: boolean;

    @Prop({required: false, default: {}})
    staticFilters!: { [field: string]: string };

    @Prop({required: false, default: false})
    minimal!: boolean;

    paginationOptions: any =
    {
        enabled: !this.minimal,
        perPage: 10,
        mode: 'pages'
    };

    get sortOptions(): any
    {
        return {
            enabled: !this.minimal,
            initialSortBy: {field: 'created', type: 'desc'}
        };
    }

    get mappedColumns(): Array<any>
    {
        return [
            {
                label: 'Id',
                field: 'id',
                sortFn: (x: number, y: number) => (x < y ? -1 : (x > y ? 1 : 0)),
                type: 'number',
                width: '10%',
                filterOptions:
                {
                    enabled: !this.minimal,
                    placeholder: "Filter",
                    filterValue: this.staticFilters["id"]
                }
            },
            {
                label: 'Type',
                field: 'type',
                width: '10%',
                filterOptions:
                {
                    enabled: !this.minimal,
                    placeholder: "Filter",
                    filterDropdownItems: this.infractionTypes,
                    filterValue: this.staticFilters["type"]
                }
            },
            {
                label: 'Created On',
                field: 'created',
                type: 'date', //Needed to bypass vue-good-table regression
                width: '20%',
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a'
            },
            {
                label: 'Subject',
                field: 'subject',
                type: 'date', //Needed to bypass vue-good-table regression
                filterOptions:
                {
                    enabled: !this.minimal,
                    placeholder: "Filter",
                    filterValue: this.staticFilters["subject"]
                },
                formatFn: guildUserFormat
            },
            {
                label: 'Creator',
                field: 'creator',
                type: 'date', //Needed to bypass vue-good-table regression
                filterOptions:
                {
                    enabled: !this.minimal,
                    placeholder: "Filter",
                    filterValue: this.staticFilters["creator"]
                },
                formatFn: guildUserFormat
            },
            {
                label: 'Reason',
                field: 'reason',
                width: '50%',
                formatFn: this.parseDiscordContent,
                html: true,
                sortable: false
            },
            {
                label: 'State',
                field: 'state',
                hidden: !this.showState,
                sortable: false
            },
            {
                label: 'Actions',
                field: 'actions',
                hidden: !this.canPerformActions,
                width: '32px',
                sortable: false
            }
        ];
    }

    get infractionTypes(): string[]
    {
        return Object.keys(InfractionType);
    }

    get filteredInfractions(): InfractionSummary[]
    {
        return _.filter(this.recordsPage.records, (infraction: InfractionSummary) =>
        {
            if (infraction.rescindAction != null)
            {
                return this.showState;
            }

            if (infraction.deleteAction != null)
            {
                return this.showDeleted;
            }

            return true;
        });
    }

    get mappedRows(): any[]
    {
        return _.map(this.filteredInfractions, infraction =>
        ({
            id: infraction.id,
            subject: infraction.subject,
            creator: infraction.createAction.createdBy,
            created: infraction.createAction.created,
            type: infraction.type,
            reason: infraction.reason,

            state: infraction.rescindAction != null ? "Rescinded"
                : infraction.deleteAction != null ? "Deleted"
                    : "Active",

            canDelete: infraction.canDelete,
            canRescind: infraction.canRescind
        }));
    }

    onSortChange(params: any)
    {
        this.emitTableChange({
            sort: {
                field: params[0].field,
                direction: getSortDirection(params[0].type)
            }
        });
    }

    onPageChange(params: any)
    {
        this.emitTableChange({
            page: params.currentPage - 1
        });
    }

    onColumnFilter(params: any)
    {
        let filters = [];

        for (let prop in params.columnFilters)
        {
            filters.push({ field: prop, value: params.columnFilters[prop] })
        }

        this.emitTableChange({filters: filters});
    }

    onPerPageChange(params: any)
    {
        this.emitTableChange({
            perPage: (params.currentPerPage == "all")
                ? 2147483647
                : params.currentPerPage
        });
    }

    async canRescind(type: InfractionType, state: string, subjectId: string): Promise<boolean>
    {
        return (type == InfractionType.Mute || type == InfractionType.Ban)
            && state != "Rescinded"
            && state != "Deleted"
            && this.hasRescindPermission
            && await GeneralService.doesModeratorOutrankUser(subjectId);
    }

    get hasRescindPermission(): boolean
    {
        return store.userHasClaims(["ModerationRescind"]);
    }

    async canDelete(state: string, subjectId: string): Promise<boolean>
    {
        return state != "Deleted"
            && await GeneralService.doesModeratorOutrankUser(subjectId);
    }

    get hasDeletePermission(): boolean
    {
        return store.userHasClaims(["ModerationDeleteInfraction"]);
    }

    get canPerformActions(): boolean
    {
        return (this.hasRescindPermission || this.hasDeletePermission) && this.showActions;
    }

    applyFilters()
    {
        let urlParams = new URLSearchParams(window.location.search);
        let filters = [];

        for (let i = 0; i < this.mappedColumns.length; i++)
        {
            let currentField: string = this.mappedColumns[i].field;

            if (urlParams.has(currentField.toLowerCase()))
            {
                filters.push({ field: currentField, value: urlParams.get(currentField.toLowerCase()) || "" });
                this.staticFilters[currentField] = urlParams.get(currentField.toLowerCase()) || "";
            }
        }

        this.emitTableChange({
            filters: filters
        });
    }

    emitTableChange(object: any)
    {
        this.$emit('tableChange', object);
    }

    onInfractionRescind(summary: InfractionSummary)
    {
        this.$emit('infractionRescind', summary)
    }

    onInfractionDelete(summary: InfractionSummary)
    {
        this.$emit('infractionDelete', summary);
    }

    emojiFor(type: InfractionType)
    {
        return infractionTypeToEmoji(type);
    }

    mounted()
    {
        this.applyFilters();

        //vue-good-table is really frustrating sometimes...
        (this.$refs.table as any).tableLoading = false;
    }
}

</script>
