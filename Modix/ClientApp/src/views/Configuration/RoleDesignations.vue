<template>
    <section class="content">
        <h1 class="title">Role Designations</h1>

        <a v-if="loading" class="button is-loading roleLoader"></a>

        <div v-else class="panel">
            <div class="panel-block designationRow is-active" v-for="designation in $store.state.modix.roleDesignationTypes" :key="designation" :class="{'is-active': viewedDesignation == designation}">
                <div class="designationGroup">
                    <div class="title is-6">
                        {{designation}}
                        <small class="heading" v-if="!designationHasRoles(designation)">None Assigned</small>
                    </div>

                    <div class="designationList" v-if="designationHasRoles(designation)">
                        <IndividualDesignation class="designation" v-for="role in rolesForDesignation(designation)" :key="role.id" :designation="role" :canDelete="canDelete()" @confirm="confirmDelete(role)" />
                    </div>
                </div>
                <div>
                    <button class="button assign is-success" @click="openModal(designation)" title="Assign" :disabled="!canAssign() || loading">+</button>
                </div>
            </div>
        </div>

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
                                    <option v-for="designation in $store.state.modix.roleDesignationTypes" :key="designation"
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

    viewedDesignation: string | null = null;

    designationCreationData: DesignatedRoleCreationData = {roleId: '', roleDesignations: []};
    createLoading: boolean = false;

    loading: boolean = false;

    get selectedRoles()
    {
        let allDesignations: DesignatedRoleMapping[] = this.$store.state.modix.roleMappings;
        return _.filter(allDesignations, d => d.roleDesignation == this.viewedDesignation);
    }

    rolesForDesignation(designation: string)
    {
        let allDesignations: DesignatedRoleMapping[] = this.$store.state.modix.roleMappings;
        return _.filter(allDesignations, d => d.roleDesignation == designation);
    }

    designationHasRoles(designation: string): boolean
    {
        return this.rolesForDesignation(designation).length > 0;
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
        this.loading = true;
        await ConfigurationService.unassignRole(role.id);
        await this.refresh();
    }

    openModal(designation: string)
    {
        this.viewedDesignation = designation;
        this.designationCreationData.roleDesignations = [designation];
        this.showModal = true;
    }

    cancel()
    {
        this.designationCreationData.roleDesignations = [];
        this.designationCreationData.roleId = '';

        this.showModal = false;
    }

    roleHasDesignation(designation: string): boolean
    {
        if (this.designationCreationData.roleId == '') { return true; }

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
        await store.retrieveRoleDesignationTypes();

        if (!this.viewedDesignation)
        {
            this.viewedDesignation = this.$store.state.modix.roleDesignationTypes[0];
        }

        await store.retrieveRoleDesignations();
        this.loading = false;
    }
}
</script>
