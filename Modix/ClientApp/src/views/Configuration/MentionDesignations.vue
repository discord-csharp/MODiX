<template>
    <section class="content">
        <h1 class="title">Mention Settings</h1>

        <div v-if="allRoles.length == 0" class="button is-loading claimLoader"></div>

        <div class="columns">

            <div class="column is-one-third">
                <aside class="menu">
                    <ul class="menu-list">

                        <li v-for="role in allRoles" v-bind:key="role.id">
                            <a v-on:click="selectedRole = role" v-bind:style="roleStyle(role)" v-bind:class="{'is-active': role == selectedRole}">
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
                                <select v-model="selectedMentionability" v-bind:key="mentionability">
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

                            <button class="delete" v-if="selectedRank" v-on:click="selectedRank = null"></button>
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

    .menu-list li a
    {
        border: 2px solid transparent;

        &.is-active
        {
            border: 2px solid black;
            background: transparent;
        }
    }

    .delete
    {
        vertical-align: middle;
        margin: 8px;
    }

</style>

<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import * as _ from 'lodash';
import Role from '@/models/Role';
import GeneralService from '@/services/GeneralService';
import ConfigurationService from '@/services/ConfigurationService';
import MentionabilityType from '@/models/configuration/MentionabilityType';
import Rank from '@/models/configuration/Rank';
import MentionData from '@/models/configuration/MentionData';

@Component({})
export default class MentionDesignations extends Vue
{
    allRoles: Role[] = [];

    selectedRole: Role | null = null;
    selectedMentionability: MentionabilityType = { name: "", description: "" };
    selectedRank: Rank | null = null;

    possibleMentionabilityTypes: MentionabilityType[] = [];
    possibleRanks: Rank[] = [];

    mentionData: MentionData | null = null;
    newMentionData: MentionData | null = null;

    @Watch('selectedRole')
    async selectedRoleChanged()
    {
        this.newMentionData = await ConfigurationService.getMentionData(this.selectedRole!.id);

        this.selectedMentionability = _.find(this.possibleMentionabilityTypes, x => x.name == this.newMentionData!.mentionability)!;
        this.selectedRank = this.newMentionData!.minimumRank;

        this.mentionData = this.newMentionData;
    }

    @Watch('selectedMentionability')
    async selectedMentionabilityChanged()
    {
        if (this.mentionData != this.newMentionData)
        {
            return;
        }

        await ConfigurationService.updateMentionability(this.selectedRole!.id, this.selectedMentionability.name);
    }

    @Watch('selectedRank')
    async selectedRankChanged()
    {
        if (this.mentionData != this.newMentionData)
        {
            return;
        }

        let newRank = this.selectedRank == null
            ? null
            : this.selectedRank.id;

        await ConfigurationService.updateMinimumRank(this.selectedRole!.id, newRank);
    }

    roleStyle(role: Role)
    {
        if (role == this.selectedRole)
        {
            return { color: role.fgColor, borderColor: role.fgColor };
        }

        return {};
    }

    async created()
    {
        this.allRoles = _.sortBy(await GeneralService.getGuildRolesExceptEveryone(), role => role.name);

        this.possibleRanks = _.sortBy(await GeneralService.getRanks(), rank => rank.position).reverse();
        this.possibleMentionabilityTypes = await ConfigurationService.getMentionabilityTypes();
    }
}
</script>
