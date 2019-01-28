<template>
    <section class="content">
        <h1 class="title">Role Designations</h1>

        <div class="tabs is-small">
            <ul>
                <li v-for="designation in possibleDesignations" :key="designation" :class="{'is-active': viewedDesignation == designation}"
                    @click="viewedDesignation = designation">
                    <a>{{designation}}</a>
                </li>
            </ul>
        </div>

        <a v-if="loading" class="button is-loading roleLoader"></a>

        <div v-else class="field is-grouped is-grouped-multiline">
            <div class="control" v-for="role in selectedRoles" :key="role.id">
                <IndividualDesignation :designation="role" :canDelete="canDelete()" @confirm="confirmDelete(role)" />
            </div>
        </div>

        <h1 class="title is-size-4" v-if="selectedRoles.length == 0">
            No roles assigned
        </h1>

        <a class="button is-success is-pulled-right" @click="openModal()" :disabled="!canAssign()">
            Assign Role
        </a>

        <div class="modal" :class="{'is-active': showModal}">
            <div class="modal-background" @click="cancel()"></div>
            <div class="modal-card">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Assign a Role
                    </p>

                    <button class="delete" aria-label="close" @click="cancel()"></button>
                </header>

                <section class="modal-card-body control">
                    <div class="columns">

                        <div class="column">
                            <label class="label">Role Name</label>
                            <Autocomplete :serviceCall="serviceCall" placeholder="@Administrator"
                                @select="selectedAutocomplete = $event" minimumChars="-1" >

                                <template slot-scope="{entry}">
                                    @{{entry.name}}
                                </template>

                            </Autocomplete>
                        </div>

                        <div class="column is-one-third">
                            <label class="label">Designation</label>
                            <div class="select is-multiple is-small">
                                <select multiple v-model="designationCreationData.roleDesignations">
                                    <option v-for="designation in possibleDesignations" :key="designation"
                                            :disabled="roleHasDesignation(designation)">
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

.designation
{
    margin-bottom: 1em;
}

select, .select
{
    margin: 0;
    width: 100%;
}

.roleLoader
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
import Role from '@/models/Role';
import { RoleDesignation } from '@/models/moderation/RoleDesignation';
import DesignatedRoleCreationData from '@/models/moderation/DesignatedRoleCreationData';
import GeneralService from '@/services/GeneralService';
import Autocomplete from '@/components/Autocomplete.vue';
import ConfigurationService from '@/services/ConfigurationService';
import IndividualDesignation from '@/components/Configuration/IndividualDesignation.vue';
import DesignatedRoleMapping from '@/models/moderation/DesignatedRoleMapping';

@Component({
    components:
    {
        Autocomplete,
        IndividualDesignation
    },
})
export default class RoleDesignations extends Vue
{
    selectedAutocomplete: Role | null = null;
    showModal: boolean = false;
    viewedDesignation: RoleDesignation = RoleDesignation.Rank;

    designationCreationData: DesignatedRoleCreationData = {roleId: '', roleDesignations: []};
    createLoading: boolean = false;

    loading: boolean = false;

    get selectedRoles()
    {
        let allDesignations: DesignatedRoleMapping[] = this.$store.state.modix.roleMappings;
        return _.filter(allDesignations, d => d.roleDesignation == this.viewedDesignation);
    }

    get serviceCall()
    {
        return GeneralService.getAllRolesAutocomplete;
    }

    @Watch('selectedAutocomplete')
    selectedChanged()
    {
        if (this.selectedAutocomplete == null)
        {
            this.designationCreationData.roleId = '';
        }
        else
        {
            this.designationCreationData.roleId = this.selectedAutocomplete.id;
        }
    }

    canAssign(): boolean
    {
        return store.userHasClaims(["DesignatedRoleMappingCreate"]);
    }

    canDelete(): boolean
    {
        return store.userHasClaims(["DesignatedRoleMappingDelete"]);
    }

    async confirmDelete(role: DesignatedRoleMapping)
    {
        await ConfigurationService.unassignRole(role.id);
        await this.refresh();
    }

    openModal()
    {
        this.designationCreationData.roleDesignations = [this.viewedDesignation];
        this.showModal = true;
    }

    cancel()
    {
        this.designationCreationData.roleDesignations = [];
        this.showModal = false;
    }

    roleHasDesignation(designation: RoleDesignation): boolean
    {
        if (this.designationCreationData.roleId == '') { return true; }

        console.log(this.$store.state.modix.roleMappings);

        return _.some(this.$store.state.modix.roleMappings, (role: DesignatedRoleMapping) =>
                role.roleId == this.designationCreationData.roleId &&
                role.roleDesignation == designation);
    }

    disableAssignButton(): boolean
    {
        if (this.designationCreationData.roleId == '') { return true; }

        return _.some(this.$store.state.modix.roleMappings, (role: DesignatedRoleMapping) =>
            role.roleId == this.designationCreationData.roleId &&
            this.designationCreationData.roleDesignations.indexOf(role.roleDesignation) > -1);
    }

    async createAssignment()
    {
        this.createLoading = true;

        await ConfigurationService.assignRole(this.designationCreationData);
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
        await store.retrieveRoleDesignations();
        this.loading = false;
    }

    get possibleDesignations() : string[]
    {
        return Object.getOwnPropertyNames(RoleDesignation);
    }
}
</script>
