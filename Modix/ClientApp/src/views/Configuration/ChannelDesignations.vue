<template>
    <section class="content">
        <h1 class="title">Channel Designations</h1>

        <a v-if="loading" class="button is-loading channelLoader"></a>

        <div v-else class="panel">
            <div class="panel-block designationRow is-active" v-for="designation in $store.state.modix.channelDesignationTypes" :key="designation" :class="{'is-active': viewedDesignation == designation}">
                <div class="designationGroup">
                    <div class="title is-6">
                        {{designation}}
                        <small class="heading" v-if="!designationHasChannels(designation)">None Assigned</small>
                    </div>

                    <div class="designationList" v-if="designationHasChannels(designation)">
                        <IndividualDesignation class="designation" v-for="channel in channelsForDesignation(designation)" :key="channel.id" :designation="channel" :canDelete="canDelete()" @confirm="confirmDelete(channel)" />
                    </div>
                </div>
                <div>
                    <button class="button assign is-success" @click="openModal(designation)" title="Assign" :disabled="!canAssign() || loading">+</button>
                </div>
            </div>
        </div>

        <div class="modal" v-if="showModal" :class="{'is-active': showModal}">
            <div class="modal-background" @click="cancel()"></div>
            <div class="modal-card">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Assign a Channel
                    </p>

                    <button class="delete" aria-label="close" @click="cancel()"></button>
                </header>

                <section class="modal-card-body control">
                    <div class="columns">

                        <div class="column">
                            <label class="label">Channel Name</label>
                            <Autocomplete :serviceCall="serviceCall" placeholder="#general"
                                @select="selectedAutocomplete = $event" minimumChars="1" >

                                <template slot-scope="{entry}">
                                    #{{entry.name}}
                                </template>

                            </Autocomplete>
                        </div>

                        <div class="column is-one-third">
                            <label class="label">Designation</label>
                            <div class="select is-multiple is-small">
                                <select multiple v-model="designationCreationData.channelDesignations">
                                    <option v-for="designation in $store.state.modix.channelDesignationTypes" :key="designation"
                                            :disabled="channelHasDesignation(designation)">
                                        {{designation}}
                                    </option>
                                </select>
                            </div>
                        </div>

                    </div>
                </section>

                <footer class="modal-card-foot level">
                    <div class="level-left">
                        <button class="button" @click="cancel()">Cancel</button>
                    </div>
                    <div class="level-right">
                        <button class="button is-success" :class="{'is-loading': createLoading}" @click="createAssignment()" :disabled="disableAssignButton()">Assign</button>
                    </div>
                </footer>
            </div>
        </div>

    </section>
</template>

<style lang="scss" scoped>
select, .select
{
    margin: 0;
    width: 100%;
}
</style>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import ModixRoute from '@/app/ModixRoute';
import {toTitleCase} from '@/app/Util';
import store from "@/app/Store";
import * as _ from 'lodash';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import Channel from '@/models/Channel';
import DesignatedChannelCreationData from '@/models/moderation/DesignatedChannelCreationData';
import GeneralService from '@/services/GeneralService';
import Autocomplete from '@/components/Autocomplete.vue';
import ConfigurationService from '@/services/ConfigurationService';
import IndividualDesignation from '@/components/Configuration/IndividualDesignation.vue';

@Component({
    components:
    {
        Autocomplete,
        IndividualDesignation
    },
})
export default class ChannelDesignations extends Vue
{
    selectedAutocomplete: Channel | null = null;
    showModal: boolean = false;

    viewedDesignation: string | null = null;

    designationCreationData: DesignatedChannelCreationData = {channelId: '', channelDesignations: []};
    createLoading: boolean = false;

    loading: boolean = false;

    get selectedChannels()
    {
        let allDesignations: DesignatedChannelMapping[] = this.$store.state.modix.channelDesignations;
        return _.filter(allDesignations, d => d.channelDesignation == this.viewedDesignation);
    }

    channelsForDesignation(designation: string)
    {
        let allDesignations: DesignatedChannelMapping[] = this.$store.state.modix.channelDesignations;
        return _.filter(allDesignations, d => d.channelDesignation == designation);
    }

    designationHasChannels(designation: string): boolean
    {
        return this.channelsForDesignation(designation).length > 0;
    }

    get serviceCall()
    {
        return GeneralService.getChannelAutocomplete;
    }

    @Watch('selectedAutocomplete')
    selectedChanged()
    {
        if (this.selectedAutocomplete == null)
        {
            this.designationCreationData.channelId = '';
        }
        else
        {
            this.designationCreationData.channelId = this.selectedAutocomplete.id;
        }
    }

    canAssign(): boolean
    {
        return store.userHasClaims(["DesignatedChannelMappingCreate"]);
    }

    canDelete(): boolean
    {
        return store.userHasClaims(["DesignatedChannelMappingDelete"]);
    }

    async confirmDelete(channel: DesignatedChannelMapping)
    {
        this.loading = true;
        await ConfigurationService.unassignChannel(channel.id);
        await this.refresh();
    }

    openModal(designation: string)
    {
        this.viewedDesignation = designation;
        this.designationCreationData.channelDesignations = [designation];
        this.showModal = true;
    }

    cancel()
    {
        this.designationCreationData.channelDesignations = [];
        this.designationCreationData.channelId = '';

        this.showModal = false;
    }

    channelHasDesignation(designation: string): boolean
    {
        if (this.designationCreationData.channelId == '') { return true; }

        return _.some(this.$store.state.modix.channelDesignations, channel =>
                channel.channelId == this.designationCreationData.channelId &&
                channel.channelDesignation == designation);
    }

    disableAssignButton(): boolean
    {
        if (this.designationCreationData.channelId == '') { return true; }

        return _.some(this.$store.state.modix.channelDesignations, (channel: DesignatedChannelMapping) =>
            channel.channelId == this.designationCreationData.channelId &&
            this.designationCreationData.channelDesignations.indexOf(channel.channelDesignation) > -1);
    }

    async createAssignment()
    {
        this.createLoading = true;

        await ConfigurationService.assignChannel(this.designationCreationData);
        await this.refresh();

        this.createLoading = false;

        this.cancel();
    }

    async created()
    {
        await this.refresh();
    }

    async refresh()
    {
        this.loading = true;
        await store.retrieveChannelDesignationTypes();

        if (this.viewedDesignation)
        {
            this.viewedDesignation = this.$store.state.modix.channelDesignationTypes[0];
        }

        await store.retrieveChannelDesignations();
        this.loading = false;
    }
}
</script>
