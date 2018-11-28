<template>
    <section class="content">
        <h1 class="title">Mention Settings</h1>

        <div v-if="allRoles.length == 0" class="button is-loading claimLoader"></div>

        <div class="columns">

            <div class="column is-one-third">
                <aside class="menu">
                    <ul class="menu-list">

                        <li v-for="role in allRoles" v-bind:key="role.id">
                            <a v-on:click="selectedRole = role" v-bind:style="roleStyle(role)" v-bind:class="{'toRole': role == selectedRole, 'is-active': role == selectedRole}">
                                {{role.name}}
                            </a>
                        </li>

                    </ul>
                </aside>
            </div>

            <div class="column">
                <template v-if="selectedRole">
                    <div class="field">
                        <label class="label">Mentionability</label>
                        <div class="control">
                            <div class="select is-primary">
                                <select v-model="selectedMentionability">
                                    <option v-for="mentionability in possibleMentionabilityTypes" v-bind:value="mentionability">
                                        {{mentionability.description}}
                                    </option>
                                </select>
                            </div>
                        </div>
                    </div>
                    <div class="field" v-if="selectedMentionability.name === 'ModixCommand'">
                        <label class="label">Minimum rank required to mention</label>
                        <div class="control">
                            <div class="select is-primary">
                                <select v-model="selectedRank">
                                    <option v-for="rank in possibleRanks" v-bind:value="rank">
                                        {{rank.name}}
                                    </option>
                                </select>
                            </div>
                        </div>
                    </div>
                </template>
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
    @import "~bulma/sass/components/menu";

    .claim.box
    {
        padding: 15px;

    }

    .claimLoader
    {
        width: 100%;
        height: 64px;
    }

    .menu-list li a
    {
        border: 2px solid transparent;

        &.is-active
        {
            border: 2px solid black;
            background: transparent;
        }
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
import Role from '@/models/Role';
import { ChannelDesignation } from '@/models/moderation/ChannelDesignation';
import DesignatedChannelCreationData from '@/models/moderation/DesignatedChannelCreationData';
import GeneralService from '@/services/GeneralService';
import Autocomplete from '@/components/Autocomplete.vue';
import TriStateCheckbox from '@/components/TriStateCheckbox.vue';
import ConfigurationService from '@/services/ConfigurationService';
import ClaimMapping, { ClaimMappingType, MappingTypeFromBoolean } from '@/models/ClaimMapping';
import MentionabilityType from '@/models/configuration/MentionabilityType';
import Rank from '@/models/configuration/Rank';

@Component({
    components:
    {
        Autocomplete,
        TriStateCheckbox
    },
})
export default class MentionDesignations extends Vue
{
    allRoles: Role[] = [];
    selectedRole: Role | null = null;
    mappedClaims: ClaimMapping[] = [];

    changingClaim: string | null = null;

    @Prop()
    selectedMentionability: MentionabilityType = { name: "", description: "" };
    selectedRank: Rank | null = null;

    possibleMentionabilityTypes: MentionabilityType[] = [];
    possibleRanks: Rank[] = [];

    mappedClaim(claim: string): ClaimMapping | undefined
    {
        return _.find(this.mappedClaims, (mapping: ClaimMapping) => mapping.roleId == this.selectedRole!.id && mapping.claim == claim);
    }

    mappedClaimValue(claim: string): boolean | null
    {
        let foundClaim = this.mappedClaim(claim);

        if (!foundClaim) { return null; }

        switch (foundClaim.type)
        {
            case ClaimMappingType.Granted:
                return true;
            case ClaimMappingType.Denied:
                return false;
            default:
                return null;
        }
    }

    get roleClaims()
    {
        var filtered = _.filter(this.mappedClaims, (claim: ClaimMapping) => claim.isRoleMapping);
        var grouped = _.groupBy(filtered, (claim: ClaimMapping) => claim.claim);

        return grouped;
    }

    roleStyle(role: Role)
    {
        if (role == this.selectedRole)
        {
            return { color: role.fgColor, borderColor: role.fgColor };
        }

        return {};
    }

    toTitleCase(input: string)
    {
        return input
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, function(str){ return str.toUpperCase(); });
    }

    get userClaims(): ClaimMapping[]
    {
        return _.filter(this.mappedClaims, (claim: ClaimMapping) => claim.isUserMapping);
    }

    async created()
    {
        this.allRoles = _.sortBy(await GeneralService.getGuildRolesExceptEveryone(), role => role.name);

        this.possibleRanks = _.sortBy(await GeneralService.getRanks(), rank => rank.position).reverse();
        this.possibleMentionabilityTypes = await ConfigurationService.getMentionabilityTypes();

        await store.retrieveClaims();
        await store.retrieveRoles();
        this.mappedClaims = await ConfigurationService.getMappedClaims();

        if (this.selectedRole == null)
        {
            this.selectedRole = this.allRoles[0];
        }
    }

    async claimChanged(key: string, newValue: boolean | null)
    {
        console.log(`${key} changed to ${newValue}`);

        this.changingClaim = key;

        try
        {
            await ConfigurationService.modifyRoleClaim(
            {
                claim: key,
                mappingType: MappingTypeFromBoolean(newValue),
                roleId: this.selectedRole!.id
            });
        }
        catch (err)
        {
            store.pushErrorMessage(`Error when updating claims: <strong>${err}</strong>`);
        }
        finally
        {
            this.mappedClaims = await ConfigurationService.getMappedClaims();
            this.changingClaim = null;
        }
    }
}
</script>
