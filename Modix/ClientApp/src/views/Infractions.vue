<template>
    <div>
        <section class="section">
            <div class="container">

                <div class="level is-mobile">
                    <div class="level-left">
                        <button class="button" v-if="canCreate" v-on:click="showCreateModal = true">Create</button>
                        &nbsp;
                        <button class="button" @click="refresh()" :class="{'is-loading': isLoading}">Refresh</button>
                        &nbsp;
                        <button class="button" disabled title="Coming Soon™" @click="showModal = true">Import</button>
                    </div>
                    <div class="level-right">
                        <label>
                            Show State
                            <input type="checkbox" v-model="showState">
                        </label> &nbsp;&nbsp;
                        <label>
                            Show Deleted
                            <input type="checkbox" v-model="showDeleted">
                        </label>
                    </div>
                </div>

                <VueGoodTable :columns="mappedColumns" :rows="mappedRows" :sortOptions="sortOptions"
                              :paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped"
                              mode="remote" :totalRows="recordsPage.filteredRecordCount" @on-page-change="onPageChange"
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
                                <button class="button is-link is-small level-left" v-show="props.row.canDelete" v-on:click="onInfractionDelete(props.row.id)">
                                    Delete
                                </button>
                                <button class="button is-link is-small level-right" v-if="props.row.canRescind" v-on:click="onInfractionRescind(props.row.id)">
                                    Rescind
                                </button>
                            </span>
                        </span>
                        <span v-else>
                            {{props.formattedRow[props.column.field]}}
                        </span>
                    </template>

                </VueGoodTable>
            </div>
        </section>

        <div class="modal" :class="{'is-active': showModal}">
            <div class="modal-background" @click="showModal = !showModal"></div>
            <div class="modal-card">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Import Rowboat Infractions
                    </p>
                    <button class="delete" aria-label="close" @click="showModal = false"></button>
                </header>
                <section class="modal-card-body">
                    <div class="notification">
                        <label class="label">
                            A JSON backup of your infractions can be retrieved from Rowboat's API. Input your guild ID below to
                            have the download link generated, and make sure you're logged in
                            <a href="https://dashboard.rowboat.party/login">here</a>

                            <input class="input is-small" type="number" placeholder="Guild ID" v-model="importGuildId" />
                        </label>

                        <template v-if="importGuildId">
                            Download the JSON
                            <a :href="rowboatDownloadUrl" target="_blank">here</a>.
                        </template>
                    </div>

                    <input type="file" name="jsonFile" @change="fileChange($event.target)" ref="fileInput" />

                    <br /><br />

                    <div class="notification is-primary" v-if="message">
                        {{message}}
                    </div>

                    <div class="notification is-danger" v-if="loadError">
                        {{loadError.toString()}}
                    </div>
                </section>
                <footer class="modal-card-foot level">
                    <div class="level-left">
                        <button class="button is-success" :disabled="!message" @click="uploadFile()">Import</button>
                    </div>
                    <div class="level-right">
                        <button class="button is-danger" @click="showModal = false">Cancel</button>
                    </div>
                </footer>
            </div>
        </div>

        <div class="modal" v-bind:class="{'is-active': showCreateModal}">
            <div class="modal-background" v-on:click="closeCreateModal"></div>
            <div class="modal-card">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Create Infraction
                    </p>
                    <button class="delete" aria-label="close" v-on:click="closeCreateModal"></button>
                </header>
                <section class="modal-card-body">

                    <div class="field">

                        <label class="label">Subject</label>

                        <div class="control">
                            <Autocomplete v-on:select="newInfractionUser = $event"
                                          v-bind:serviceCall="userServiceCall" placeholder="We have a fancy autocomplete!">
                                <template slot-scope="{entry}">
                                    <TinyUserView :user="entry" />
                                </template>
                            </Autocomplete>
                            <div class="notification is-danger" v-if="!userOutranksSubject">You need to have a higher rank than {{newInfractionUser.name}} in order to add an infraction for them.</div>
                        </div>
                    </div>

                    <div class="field">

                        <label class="label">Infraction</label>

                        <div class="field has-addons">
                            <p class="control">
                                <span class="select">
                                    <select v-model="newInfractionType">
                                        <option v-if="canNote" v-html="emojiFor(noteType) + ' ' + noteType" v-bind:value="noteType" />
                                        <option v-if="canWarn" v-html="emojiFor(warnType) + ' ' + warnType" v-bind:value="warnType" />
                                        <option v-if="canMute" v-html="emojiFor(muteType) + ' ' + muteType" v-bind:value="muteType" />
                                        <option v-if="canBan" v-html="emojiFor(banType) + ' ' + banType" v-bind:value="banType" />
                                    </select>
                                </span>
                            </p>
                            <p class="control is-expanded">
                                <input class="input" type="text" v-model.trim="newInfractionReason" placeholder="Give a reason for the infraction..." />
                            </p>
                        </div>
                    </div>

                    <div class="field" v-if="newInfractionType == muteType">

                        <label class="label">Duration</label>

                        <div class="field has-addons">
                            <p class="control">
                                <input class="input has-text-right" type="text" v-model.number="newInfractionMonths" placeholder="Months" />
                            </p>
                            <p class="control">
                                <input class="input has-text-right" type="text" v-model.number="newInfractionDays" placeholder="Days" />
                            </p>
                            <p class="control">
                                <input class="input has-text-right" type="text" v-model.number="newInfractionHours" placeholder="Hours" />
                            </p>
                            <p class="control">
                                <input class="input has-text-right" type="text" v-model.number="newInfractionMinutes" placeholder="Minutes" />
                            </p>
                            <p class="control">
                                <input class="input has-text-right" type="text" v-model.number="newInfractionSeconds" placeholder="Seconds" />
                            </p>
                        </div>
                    </div>

                </section>
                <footer class="modal-card-foot level">
                    <div class="level-left">
                        <button class="button is-success" v-bind:disabled="!canCreateNewInfraction" v-on:click="onInfractionCreate">Create</button>
                    </div>
                    <div class="level-right">
                        <button class="button is-danger" v-on:click="closeCreateModal">Cancel</button>
                    </div>
                </footer>
            </div>
        </div>

        <ConfirmationModal v-bind:isShown="showRescindConfirmation" v-on:modal-confirmed="confirmRescind" v-on:modal-cancelled="cancelRescind" />
        <ConfirmationModal v-bind:isShown="showDeleteConfirmation" v-on:modal-confirmed="confirmDelete" v-on:modal-cancelled="cancelDelete" />

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
import { InfractionType } from '@/models/infractions/InfractionType'
import GuildUserIdentity from '@/models/core/GuildUserIdentity'
import User from '@/models/User';
import GeneralService from '@/services/GeneralService';
import InfractionSummary from '@/models/infractions/InfractionSummary';
import {config, setConfig} from '@/models/PersistentConfig';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import InfractionCreationData from '@/models/infractions/InfractionCreationData';
import RecordsPage from '@/models/RecordsPage';
import TableParameters from '@/models/TableParameters';
import { SortDirection } from '@/models/SortDirection';

const messageResolvingRegex = /<#(\d+)>/gm;

function getSortDirection(direction: string): SortDirection
{
    return (direction == "asc")
        ? SortDirection.Ascending
        : SortDirection.Descending;
}

const guildUserFormat = (subject: GuildUserIdentity) => subject.displayName;

@Component({
    components:
    {
        HeroHeader,
        VueGoodTable,
        TinyUserView,
        Autocomplete,
        ConfirmationModal
    }
})
export default class Infractions extends Vue
{
    get infractionTypes(): string[]
    {
        return Object.keys(InfractionType).map(c => InfractionType[<any>c]);
    }

    paginationOptions: any =
    {
        enabled: true,
        perPage: 10,
        mode: 'pages'
    };

    sortOptions: any =
    {
        enabled: true,
        initialSortBy: {field: 'created', type: 'desc'}
    };

    showState: boolean = false;
    showDeleted: boolean = false;

    showCreateModal: boolean = false;
    showModal: boolean = false;
    message: string | null = null;
    loadError: string | null = null;
    importGuildId: number | null = null;
    isLoading: boolean = false;

    noteType: InfractionType = InfractionType.Notice;
    warnType: InfractionType = InfractionType.Warning;
    muteType: InfractionType = InfractionType.Mute;
    banType: InfractionType = InfractionType.Ban;

    newInfractionUser: User | null = null;
    newInfractionType: InfractionType | null = null;
    newInfractionReason: string = "";
    newInfractionMonths: number | null = null;
    newInfractionDays: number | null = null;
    newInfractionHours: number | null = null;
    newInfractionMinutes: number | null = null;
    newInfractionSeconds: number | null = null;

    userOutranksSubject: boolean = true;

    showRescindConfirmation: boolean = false;
    showDeleteConfirmation: boolean = false;
    toRescind: number = 0;
    toDelete: number = 0;

    channelCache: { [channel: string]: DesignatedChannelMapping } | null = null;

    recordsPage: RecordsPage<InfractionSummary> = new RecordsPage<InfractionSummary>();
    tableParams: TableParameters = {
        page: 0,
        perPage: 10,
        sort: {
            field: "created",
            direction: SortDirection.Descending
        },
        filters: []
    };

    get canNote(): boolean
    {
        return store.userHasClaims(["ModerationNote"]);
    }

    get canWarn(): boolean
    {
        return store.userHasClaims(["ModerationWarn"]);
    }

    get canMute(): boolean
    {
        return store.userHasClaims(["ModerationMute"]);
    }

    get canBan(): boolean
    {
        return store.userHasClaims(["ModerationBan"]);
    }

    get canCreate(): boolean
    {
        return this.canNote || this.canWarn || this.canMute || this.canBan;
    }

    @Watch('newInfractionUser')
    async setUserOutranksFlag(): Promise<void>
    {
        if (!this.newInfractionUser)
        {
            this.userOutranksSubject = true;
        }
        else
        {
            this.userOutranksSubject = await GeneralService.doesModeratorOutrankUser(this.newInfractionUser!.userId);
        }
    }

    get canCreateNewInfraction(): boolean
    {
        let requiresReason = this.newInfractionType == InfractionType.Notice || this.newInfractionType == InfractionType.Warning;

        return this.newInfractionUser != null && this.newInfractionType != null && (!requiresReason || this.newInfractionReason != "") && this.userOutranksSubject;
    }

    async canRescind(type: InfractionType, state: string, subjectId: string): Promise<boolean>
    {
        return (type == this.muteType || type == this.banType)
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
        return this.hasRescindPermission || this.hasDeletePermission;
    }

    get fileInput(): HTMLInputElement
    {
        return <HTMLInputElement>this.$refs.fileInput;
    }

    async uploadFile()
    {
        let formData = new FormData();
        formData.append("file", this.fileInput.files![0]);

        if (formData)
        {
            try
            {
                let result = await GeneralService.uploadRowboatJson(formData);
                this.message = `${result} rows imported`;
            }
            catch (err)
            {
                this.loadError = err;
            }
        }
    }

    resolveMentions(description: string)
    {
        let replaced = description;

        if (this.channelCache)
        {
            replaced = description.replace(messageResolvingRegex, (sub, args: string) =>
            {
                if (this.channelCache == null || this.channelCache[args] == null)
                {
                    return args;
                }

                let found = (this.channelCache[args] ? this.channelCache[args].name : args);

                return `<span class='channel'>#${found}</span>`;
            });
        }

        return `<span class='pre'>${replaced}</span>`;
    }

    fileChange(input: HTMLInputElement)
    {
        let files = input.files;
        if (!files || files.length == 0) { return; }

        let reader = new FileReader();

        reader.onloadend = () =>
        {
            try
            {
                let data = JSON.parse(<string>reader.result);

                if (!Array.isArray(data))
                {
                    throw Error("JSON was not valid - should be an array of Rowboat infractions.");
                }

                this.loadError = null;
                this.message = `${data.length} infractions found. Ready to import.`;
            }
            catch (err)
            {
                console.log(err);

                this.loadError = err;
                this.message = null;
            }
        };

        reader.readAsText(files[0]);
    }

    staticFilters: { [field: string]: string } = { id: "", type: "", subject: "", creator: "" };

    get mappedColumns(): Array<any>
    {
        return [
            {
                label: 'Id',
                field: 'id',
                sortFn: (x: number, y: number) => (x < y ? -1 : (x > y ? 1 : 0)),
                type: 'number',
                filterOptions:
                {
                    enabled: true,
                    placeholder: "Filter",
                    filterValue: this.staticFilters["id"]
                }
            },
            {
                label: 'Type',
                field: 'type',
                filterOptions:
                {
                    enabled: true,
                    placeholder: "Filter",
                    filterDropdownItems: this.infractionTypes,
                    filterValue: this.staticFilters["type"]
                }
            },
            {
                label: 'Created On',
                field: 'date',
                type: 'date', //Needed to bypass vue-good-table regression
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a',
                width: '160px'
            },
            {
                label: 'Subject',
                field: 'subject',
                type: 'date', //Needed to bypass vue-good-table regression
                filterOptions:
                {
                    enabled: true,
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
                    enabled: true,
                    placeholder: "Filter",
                    filterValue: this.staticFilters["creator"]
                },
                formatFn: guildUserFormat
            },
            {
                label: 'Reason',
                field: 'reason',
                formatFn: this.resolveMentions,
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

    emojiFor(infractionType: InfractionType): string
    {
        switch (infractionType)
        {
            case "Notice":
                return "&#128221;";
            case "Warning":
                return "&#9888;";
            case "Mute":
                return "&#128263;";
            case "Ban":
                return "&#128296;";
            default:
                return infractionType;
        }
    }

    get rowboatDownloadUrl()
    {
        return `https://dashboard.rowboat.party/api/guilds/${this.importGuildId}/infractions`;
    }

    get mappedRows(): any[]
    {
        return _.map(this.filteredInfractions, infraction =>
        ({
            id: infraction.id,
            subject: infraction.subject,
            creator: infraction.createAction.createdBy,
            date: infraction.createAction.created,
            type: infraction.type,
            reason: infraction.reason,

            state: infraction.rescindAction != null ? "Rescinded"
                : infraction.deleteAction != null ? "Deleted"
                    : "Active",

            canDelete: infraction.canDelete,
            canRescind: infraction.canRescind
        }));
    }

    async refresh()
    {
        this.isLoading = true;

        this.recordsPage = await GeneralService.getInfractions(this.tableParams);

        await store.retrieveChannels();

        this.channelCache = _.keyBy(this.$store.state.modix.channels, channel => channel.id);

        this.clearNewInfractionData();

        this.isLoading = false;
    }

    clearNewInfractionData()
    {
        this.newInfractionUser = null;
        this.newInfractionType = null;
        this.newInfractionReason = "";
        this.newInfractionMonths = null;
        this.newInfractionDays = null;
        this.newInfractionHours = null;
        this.newInfractionMinutes = null;
        this.newInfractionSeconds = null;
    }

    applyFilters()
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

    async created()
    {
        await this.refresh();

        this.showState = config().showInfractionState;
        this.showDeleted = config().showDeletedInfractions;

        this.applyFilters();
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

    @Watch('showState')
    async inactiveChanged()
    {
        setConfig(conf => conf.showInfractionState = this.showState);
    }

    @Watch('showDeleted')
    async deletedChanged()
    {
        setConfig(conf => conf.showDeletedInfractions = this.showDeleted);
    }

    get userServiceCall()
    {
        return GeneralService.getUserAutocomplete;
    }

    closeCreateModal(): void
    {
        this.clearNewInfractionData();
        this.showCreateModal = false;
    }

    async onInfractionCreate(): Promise<void>
    {
        let subjectId = this.newInfractionUser!.userId;

        let creationData = new InfractionCreationData();

        creationData.type = this.newInfractionType!;
        creationData.reason = this.newInfractionReason!;
        creationData.durationMonths = this.newInfractionMonths;
        creationData.durationDays = this.newInfractionDays;
        creationData.durationHours = this.newInfractionHours;
        creationData.durationMinutes = this.newInfractionMinutes;
        creationData.durationSeconds = this.newInfractionSeconds;

        await GeneralService.createInfraction(subjectId, creationData);

        await this.refresh();
        await this.closeCreateModal();
    }

    onInfractionRescind(id: number): void
    {
        this.toRescind = id;
        this.showRescindConfirmation = true;
    }

    async confirmRescind(): Promise<void>
    {
        this.showRescindConfirmation = false;
        await GeneralService.rescindInfraction(this.toRescind);
        this.toRescind = 0;
        await this.refresh();
    }

    cancelRescind(): void
    {
        this.showRescindConfirmation = false;
        this.toRescind = 0;
    }

    onInfractionDelete(id: number): void
    {
        this.toDelete = id;
        this.showDeleteConfirmation = true;
    }

    async confirmDelete(): Promise<void>
    {
        this.showDeleteConfirmation = false;
        await GeneralService.deleteInfraction(this.toDelete);
        this.toDelete = 0;
        await this.refresh();
    }

    cancelDelete(): void
    {
        this.showDeleteConfirmation = false;
        this.toDelete = 0;
    }
}
</script>
