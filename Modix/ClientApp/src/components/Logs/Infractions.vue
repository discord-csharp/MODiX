<template>
    <div class="infractions">
        <div class="level is-mobile">
            <div class="level-left">
                <button class="button" v-if="canCreate" v-on:click="showCreateModal = true">Create</button>
                &nbsp;
                <button class="button" @click="refresh()" :class="{'is-loading': isLoading}">Refresh</button>
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

        <InfractionTable @tableChange="tableChange" @infractionRescind="onInfractionRescind" @infractionDelete="onInfractionDelete"
            :recordsPage="recordsPage" :showActions="true"
            :showState="showState" :showDeleted="showDeleted" :staticFilters="staticFilters">
        </InfractionTable>

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

        <ConfirmationModal v-bind:isShown="showRescindConfirmation" v-on:modal-confirmed="confirmRescind" v-on:modal-cancelled="cancelRescind"
            :mainText="`Are you sure you want to rescind infraction #${toRescind}?`" />
        <ConfirmationModal v-bind:isShown="showDeleteConfirmation" v-on:modal-confirmed="confirmDelete" v-on:modal-cancelled="cancelDelete"
            :mainText="`Are you sure you want to delete infraction #${toDelete}?`" />

    </div>
</template>

<script lang="ts">
import * as _ from 'lodash';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import TinyUserView from '@/components/TinyUserView.vue';
import Autocomplete from '@/components/Autocomplete.vue';
import ConfirmationModal from '@/components/ConfirmationModal.vue';
import InfractionTable from '@/components/Logs/InfractionTable.vue';
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
        HeroHeader,
        TinyUserView,
        Autocomplete,
        ConfirmationModal,
        InfractionTable
    }
})
export default class Infractions extends ModixComponent
{
    get infractionTypes(): string[]
    {
        return Object.values(InfractionType);
    }

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

    staticFilters: { [field: string]: string } = { id: "", type: "", subject: "", creator: "" };

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

    async tableChange(object: any)
    {
        Object.assign(this.tableParams, object);
        await this.refresh();
    }

    async refresh()
    {
        if (this.isLoading) { return; }

        this.isLoading = true;

        this.recordsPage = await GeneralService.getInfractions(this.tableParams);

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

    async created()
    {
        await this.refresh();

        this.showState = config().showInfractionState;
        this.showDeleted = config().showDeletedInfractions;

        await store.retrieveChannels();
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

    onInfractionRescind(summary: InfractionSummary): void
    {
        this.toRescind = summary.id;
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

    onInfractionDelete(summary: InfractionSummary): void
    {
        this.toDelete = summary.id;
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

    emojiFor(type: InfractionType)
    {
        return infractionTypeToEmoji(type);
    }
}
</script>
