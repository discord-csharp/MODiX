<template>
    <div>
        <section class="section">
            <div class="container">

                <div class="level is-mobile">
                    <div class="level-left">
                        <button class="button" v-if="canCreate" v-on:click="showCreateModal = true">Create</button>
                        &nbsp;
                        <button class="button" v-on:click="refresh()" v-bind:class="{'is-loading': isLoading}">Refresh</button>
                    </div>
                </div>

                <VueGoodTable v-bind:columns="mappedColumns" v-bind:rows="mappedRows" v-bind:sortOptions="sortOptions"
                              v-bind:paginationOptions="paginationOptions" styleClass="vgt-table condensed bordered striped">

                    <template slot="table-row" slot-scope="props">
                        <span v-if="props.column.field == 'actions'">
                            <span class="level">
                                <button class="button is-link is-small level-left" v-if="props.row.canMaintain" v-on:click="onTagEdit(props.row)">
                                    Edit
                                </button>
                                &nbsp;
                                <button class="button is-link is-small level-right" v-if="props.row.canMaintain" v-on:click="onTagDelete(props.row)">
                                    Delete
                                </button>
                            </span>
                        </span>
                        <span v-else-if="props.column.html" v-html="props.formattedRow[props.column.field]" />
                        <span v-else>
                            {{props.formattedRow[props.column.field]}}
                        </span>
                    </template>

                </VueGoodTable>
            </div>
        </section>

        <TagCreationModal :shown="showCreateModal" :selectedTag="tagToEdit" :allTags="tags" @close="closeCreateModal" @submit="createModalSubmit"></TagCreationModal>

        <ConfirmationModal v-bind:isShown="showDeleteConfirmation" v-on:modal-confirmed="confirmDelete" v-on:modal-cancelled="cancelDelete"
            :mainText="(tagToDelete ? `Are you sure you want to delete tag '${tagToDelete.name}'?` : '')" />

    </div>
</template>

<script lang="ts">
import * as _ from 'lodash';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import TinyUserView from '@/components/TinyUserView.vue';
import Autocomplete from '@/components/Autocomplete.vue';
import ConfirmationModal from '@/components/ConfirmationModal.vue';
import TagCreationModal from '@/components/Tags/TagCreationModal.vue';
import store from "@/app/Store";
import { VueGoodTable } from 'vue-good-table';
import GuildUserIdentity from '@/models/core/GuildUserIdentity';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import TagSummary from '@/models/Tags/TagSummary';
import TagService from '@/services/TagService';
import TagCreationData from '../../models/Tags/TagCreationData';
import TagMutationData from '../../models/Tags/TagMutationData';
import ModixComponent from '@/components/ModixComponent.vue';
import { GuildRoleBrief } from '@/models/promotions/PromotionCampaign';

type TagOwner = GuildUserIdentity & GuildRoleBrief;

const guildUserFilter = (subject: TagOwner, filter: string) =>
{
    filter = _.lowerCase(filter);

    return subject.id.toString().startsWith(filter) ||
        _.lowerCase(subject.displayName).indexOf(filter) >= 0;
};

const guildUserSort = (x: TagOwner, y: TagOwner, col: any, rowX: any, rowY: any) =>
{
    return (x.id < y.id ? -1 : (x.id > y.id ? 1 : 0));
};

@Component({
    components:
    {
        HeroHeader,
        VueGoodTable,
        TinyUserView,
        Autocomplete,
        ConfirmationModal,
        TagCreationModal
    }
})
export default class Tags extends ModixComponent
{
    paginationOptions: any =
    {
        enabled: true,
        perPage: 10
    };

    sortOptions: any =
    {
        enabled: true,
        initialSortBy: {field: 'name', type: 'asc'}
    };

    isLoading: boolean = false;

    tags: TagSummary[] = [];

    showCreateModal: boolean = false;
    tagToEdit: TagSummary | null = null;

    showDeleteConfirmation: boolean = false;
    tagToDelete: TagSummary | null = null;

    get canCreate(): boolean
    {
        return store.userHasClaims(["CreateTag"]);
    }

    staticFilters: {[field: string]: string} = {name: "", creator: "", content: ""};

    get mappedColumns(): any[]
    {
        return [
            {
                label: 'Name',
                field: 'name',
                filterOptions:
                {
                    enabled: true,
                    placeholder: "Filter"
                }
            },
            {
                label: 'Last Modified',
                field: 'date',
                type: 'date',
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a',
                width: '160px'
            },
            {
                label: 'Owner',
                field: 'owner',
                type: 'date',
                sortFn: guildUserSort,
                formatFn: this.formatTagOwner,
                html: true,
                filterOptions:
                {
                     enabled: true,
                     filterFn: guildUserFilter,
                     filterValue: this.staticFilters["owner"],
                     placeholder: "Filter"
                }
            },
            {
                label: 'Content',
                field: 'content',
                formatFn: this.parseDiscordContent,
                html: true,
                filterOptions:
                {
                    enabled: true,
                    placeholder: "Filter"
                }
            },
            {
                label: 'Uses',
                field: 'uses',
                type: 'number'
            },
            {
                label: 'Actions',
                field: 'actions',
                width: '32px',
                sortable: false
            }
        ];
    }

    get mappedRows(): any[]
    {
        return _.map(this.tags, tag =>
        ({
            name: tag.name,
            date: tag.created,
            owner: (tag.ownerRole != null ? tag.ownerRole : tag.ownerUser),
            content: tag.content,
            uses: tag.uses,
            isOwnedByRole: tag.isOwnedByRole,
            canMaintain: tag.canMaintain,
        }));
    }

    formatTagOwner(owner: TagOwner)
    {
        let mention = "";

        if (owner.position != undefined) //is a role
        {
            mention = `<@&${owner.id}>`;
        }
        else //is a user
        {
            mention = `${owner.displayName}`;
        }

        return this.parseDiscordContent(mention);
    }

    async refresh()
    {
        this.isLoading = true;

        this.tags = await TagService.getTags();
        await store.retrieveChannels();

        this.clearSelectedTag();
        this.applyFilters();

        this.isLoading = false;
    }

    clearSelectedTag(): void
    {
        this.tagToEdit = null;
    }

    applyFilters(): void
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
    }

    closeCreateModal(): void
    {
        this.clearSelectedTag();
        this.showCreateModal = false;
    }

    async createModalSubmit()
    {
        this.closeCreateModal();
        await this.refresh();
    }

    async onTagEdit(tag: TagSummary)
    {
        this.tagToEdit = tag;
        this.showCreateModal = true;
    }

    onTagDelete(tag: TagSummary)
    {
        this.tagToDelete = tag;
        this.showDeleteConfirmation = true;
    }

    async confirmDelete(): Promise<void>
    {
        this.showDeleteConfirmation = false;
        await TagService.deleteTag(this.tagToDelete!.name);
        this.tagToDelete = null;
        await this.refresh();
    }

    cancelDelete(): void
    {
        this.showDeleteConfirmation = false;
        this.tagToDelete = null;
    }

    async mounted()
    {
        await store.retrieveRoles();
        await this.refresh();
    }
}
</script>
