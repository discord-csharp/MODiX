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
                            Show Inactive &amp; Deleted
                            <input type="checkbox" v-model="showInactive">
                        </label>
                    </div>
                </div>

                <VueGoodTable :columns="mappedColumns" :rows="mappedRows" 
                    :paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped">

                    <template slot="table-row" slot-scope="props">
                        <span v-if="props.column.field == 'type'">
                            <span :title="props.formattedRow[props.column.field]">
                                {{emojiFor(props.formattedRow[props.column.field])}} {{props.formattedRow[props.column.field]}}
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

<style lang="scss">

@import "../styles/variables";
@import "~bulma/sass/components/level";
@import "~bulma/sass/components/modal";
@import "~bulma/sass/elements/notification";
@import "~bulma/sass/elements/form";


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

.vgt-select
{
    padding: 0px;
}

.vgt-responsive
{
    @include fullwidth-desktop();
}

.modal-card-foot.level
{
    justify-content: space-between;
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

import 'vue-good-table/dist/vue-good-table.css'
import GeneralService from '@/services/GeneralService';
import InfractionSummary from '@/models/infractions/InfractionSummary';
import {config, setConfig} from '@/models/PersistentConfig';


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

    showInactive: boolean = false;
    showModal: boolean = false;
    message: string | null = null;
    loadError: string | null = null;
    importGuildId: number | null = null;
    isLoading: boolean = false;

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

    get mappedColumns(): Array<any>
    {
        return [
            {
                label: 'Id',
                field: 'id'
            },
            {
                label: 'Type',
                field: 'type',
                filterOptions: { enabled: true, filterDropdownItems: this.infractionTypes }
            },
            {
                label: 'Created On',
                field: 'date',
                type: 'date',
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a'
            },
            {
                label: 'Subject',
                field: 'subject',
                filterOptions: { enabled: true }
            },
            {
                label: 'Creator',
                field: 'creator',
                filterOptions: { enabled: true }
            },
            {
                label: 'Reason',
                field: 'reason'
            },
            {
                label: 'State',
                field: 'state',
                hidden: !this.showInactive
            }
        ];
    }

    get filteredInfractions(): InfractionSummary[]
    {
        var self = this;

        return _.filter(this.$store.state.modix.infractions, (infraction: InfractionSummary) =>
        {
            if (infraction.rescindAction != null || infraction.deleteAction != null)
            {
                return self.showInactive;
            }
            
            return true;
        });
    }

    emojiFor(infractionType: InfractionType): string
    {
        switch (infractionType)
        {
            case "Notice":
                return "📝";
            case "Warning":
                return "⚠️";
            case "Mute":
                return "🔇";
            case "Ban":
                return "🔨";
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
            subject: infraction.subject.displayName,
            creator: infraction.createAction.createdBy.displayName,
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

        this.isLoading = false;
    }

    async created()
    {
        await this.refresh();
        this.showInactive = config().showInactiveInfractions;
    }

    @Watch('showInactive')
    inactiveChanged()
    {
        setConfig(conf => conf.showInactiveInfractions = this.showInactive);
    }
}
</script>
