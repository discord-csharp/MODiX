<template>
    <section class="content">
        <h1 class="title">Claim Assignments</h1>

        <!--
        <div class="tabs is-small">
            <ul>
                <li><a>Role Claims</a></li>
                <li><a>User Claims</a></li>
            </ul>
        </div>
        -->

        <div v-if="allRoles.length == 0" class="button is-loading claimLoader"></div>

        <div class="columns">

            <div class="column is-one-third">
                <aside class="menu">                   
                    <ul class="menu-list">

                        <li v-for="role in allRoles" :key="role.id">
                            <a @click="selectedRole = role" :style="roleStyle(role)" :class="{'toRole': role == selectedRole, 'is-active': role == selectedRole}">
                                {{role.name}}
                            </a>
                        </li>

                    </ul>
                </aside>
            </div>
        
            <div class="column">
                <template v-if="selectedRole">
                    <div class="" v-for="(claims, category) in groupedPossibleClaims" :key="category">
                        <h1 class="small-title">{{toTitleCase(category)}}</h1>

                        <div class="claim box" v-for="claim in claims" :key="claim.name">
                            <strong>{{toTitleCase(claim.name)}}</strong>
                            <TriStateCheckbox @input="claimChanged(claim.name, $event)" :value="mappedClaimValue(claim.name)" :loading="changingClaim == claim.name" />
                            <p>{{claim.description}}</p>
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
import Claim from '@/models/Claim';

@Component({
    components:
    {
        Autocomplete,
        TriStateCheckbox
    },
})
export default class Claims extends Vue
{
    allRoles: Role[] = [];
    selectedRole: Role | null = null;
    mappedClaims: ClaimMapping[] = [];

    changingClaim: string | null = null;

    get possibleClaims(): Claim[]
    {
        return this.$store.state.modix.claims;
    }

    get groupedPossibleClaims(): {[category: string]: Claim[]}
    {
        let sorted = _.sortBy(this.possibleClaims, (claim: Claim) => claim.name);
        return _.groupBy(sorted, (claim: Claim) => claim.category);
    }

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
        await store.retrieveClaims();
        await store.retrieveRoles();
        this.mappedClaims = await ConfigurationService.getMappedClaims();

        this.allRoles = _.sortBy(this.$store.state.modix.roles, role => role.name);

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
