<template>
    <section class="content">
        <h1 class="title">Channel Designations</h1>

        <div class="tabs is-small">
            <ul>
                <li v-for="designation in possibleDesignations" :key="designation" :class="{'is-active': viewedDesignation == designation}"
                    @click="viewedDesignation = designation">
                    <a>{{designation}}</a>
                </li>
            </ul>
        </div>

        <a v-if="loading" class="button is-loading channelLoader"></a>

        <div v-else class="field is-grouped is-grouped-multiline">
            <div class="control" v-for="channel in selectedChannels" :key="channel.id">
                <IndividualDesignation :designation="channel" :canDelete="canDelete()" @confirm="confirmDelete(channel)" />
            </div>
        </div>

        <h1 class="title is-size-4" v-if="selectedChannels.length == 0">
            No channels assigned
        </h1>

        <a class="button is-success is-pulled-right" @click="openModal()" :disabled="!canAssign()">
            Assign Channel
        </a>

        <div class="modal" :class="{'is-active': showModal}">
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
                                    <option v-for="designation in possibleDesignations" :key="designation" 
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

<style scoped lang="scss">

@import "../../styles/variables";
@import "~bulma/sass/base/_all";
@import "~bulma/sass/components/tabs";
@import "~bulma/sass/components/modal";
@import '~bulma/sass/elements/form';

.designation
{
    margin-bottom: 1em;
}

select, .select
{
    margin: 0;
    width: 100%;
}

.channelLoader
{
    width: 100%;
    height: 64px;
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
import { ChannelDesignation } from '@/models/moderation/ChannelDesignation';
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
    viewedDesignation: ChannelDesignation = ChannelDesignation.ModerationLog;

    designationCreationData: DesignatedChannelCreationData = {channelId: '', channelDesignations: []};
    createLoading: boolean = false;

    loading: boolean = false;

    get selectedChannels()
    {
        let allDesignations: DesignatedChannelMapping[] = this.$store.state.modix.channelDesignations;
        return _.filter(allDesignations, d => d.channelDesignation == this.viewedDesignation);
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
        await ConfigurationService.unassignChannel(channel.id);
        await this.refresh();
    }

    openModal()
    {
        this.designationCreationData.channelDesignations = [this.viewedDesignation];
        this.showModal = true;
    }

    cancel()
    {
        this.designationCreationData.channelDesignations = [];
        this.showModal = false;
    }

    channelHasDesignation(designation: ChannelDesignation): boolean
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
        await store.retrieveChannelDesignations();
        this.loading = false;
    }

    get possibleDesignations() : string[]
    {
        return _.map(ChannelDesignation, d => ChannelDesignation[d]);
    }
}
</script>
