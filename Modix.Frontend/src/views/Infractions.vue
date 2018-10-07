<template>
    <div>
        <section class="section">
            <div class="container">

                <div class="level is-mobile">
                    <div class="level-left">
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
                    :paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped">

                    <template slot="table-row" slot-scope="props">
                        <span v-if="props.column.field == 'type'">
                            <span :title="props.formattedRow[props.column.field]" class="typeCell">
                                {{emojiFor(props.formattedRow[props.column.field])}} {{props.formattedRow[props.column.field]}}
                            </span>
                        </span>
                        <span v-else-if="props.column.field == 'reason'" v-html="props.formattedRow[props.column.field]">
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
                            Download the JSON <a :href="rowboatDownloadUrl" target="_blank">here</a>.
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
    </div>
</template>

<style scoped lang="scss">

@import "../styles/variables";
@import "~bulma/sass/components/modal";
@import "~bulma/sass/elements/notification";
@import "~bulma/sass/elements/form";

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

.typeCell
{
    display: block;
    white-space: nowrap;
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

.channel
{
    font-weight: bold;
}

.pre
{
    white-space: pre-line;
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

const messageResolvingRegex = /<#(\d+)>/gm;

const guildUserFilter = (subject: GuildUserIdentity, filter: string) =>
{
    filter = filter.toLowerCase();
    
    return subject.id.toString().toLowerCase().indexOf(filter) >= 0 ||
            subject.displayName.toLowerCase().indexOf(filter) >= 0;
};

const guildUserFormat = (subject: GuildUserIdentity) => subject.displayName;

@Component({
    components:
    {
        HeroHeader,
        VueGoodTable
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
        perPage: 10
    };

    sortOptions: any = 
    {
        enabled: true,
        initialSortBy: {field: 'date', type: 'desc'}
    };

    showState: boolean = false;
    showDeleted: boolean = false;

    showModal: boolean = false;
    message: string | null = null;
    loadError: string | null = null;
    importGuildId: number | null = null;
    isLoading: boolean = false;

    channelCache: {[channel: string]: DesignatedChannelMapping} | null = null;

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

    staticFilters: {[field: string]: string} = {subject: "", creator: "", id: ""};

    get mappedColumns(): Array<any>
    {
        return [
            {
                label: 'Id',
                field: 'id',
                filterOptions:
                {
                    enabled: true,
                    filterValue: this.staticFilters["id"],
                    placeholder: "Filter"
                }
            },
            {
                label: 'Type',
                field: 'type',
                filterOptions:
                {
                    enabled: true,
                    filterDropdownItems: this.infractionTypes,
                    placeholder: "Filter"
                }
            },
            {
                label: 'Created On',
                field: 'date',
                type: 'date',
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a',
                width: '160px'
            },
            {
                label: 'Subject',
                field: 'subject',
                filterOptions:
                {
                    enabled: true,
                    filterFn: guildUserFilter,
                    filterValue: this.staticFilters["subject"],
                    placeholder: "Filter"
                },
                formatFn: guildUserFormat
            },
            {
                label: 'Creator',
                field: 'creator',
                filterOptions:
                {
                     enabled: true,
                     filterFn: guildUserFilter,
                     filterValue: this.staticFilters["creator"],
                     placeholder: "Filter"
                },
                formatFn: guildUserFormat
            },
            {
                label: 'Reason',
                field: 'reason',
                formatFn: this.resolveMentions,
                html: true
            },
            {
                label: 'State',
                field: 'state',
                hidden: !this.showState
            }
        ];
    }

    get filteredInfractions(): InfractionSummary[]
    {
        return _.filter(this.$store.state.modix.infractions, (infraction: InfractionSummary) =>
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
            id: infraction.id.toString(),
            subject: infraction.subject,
            creator: infraction.createAction.createdBy,
            date: infraction.createAction.created,
            type: infraction.type,
            reason: infraction.reason,

            state: infraction.rescindAction != null ? "Rescinded"
                   : infraction.deleteAction != null ? "Deleted"
                       : "Active"
        }));
    }

    async refresh()
    {
        this.isLoading = true;

        store.clearInfractionData();
        await store.retrieveInfractions();
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

    async created()
    {
        await this.refresh();

        this.showState = config().showInfractionState;
        this.showDeleted = config().showDeletedInfractions;

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
