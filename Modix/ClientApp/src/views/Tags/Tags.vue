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
                                <button class="button is-primary is-small level-left" v-if="canMaintainTag(props.row.creator)" v-on:click="onTagEdit(props.row.name, props.row.content)">
                                    Edit
                                </button>
                                <button class="button is-primary is-small level-right" v-if="canMaintainTag(props.row.creator)" v-on:click="onTagDelete(props.row.name)">
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

        <div class="modal" v-bind:class="{'is-active': showCreateModal}">
            <div class="modal-background" v-on:click="closeCreateModal"></div>
            <div class="modal-card">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Create Tag
                    </p>
                    <button class="delete" aria-label="close" v-on:click="closeCreateModal"></button>
                </header>
                <section class="modal-card-body">

                    <div class="field">
                        <label class="label">Tag name</label>

                        <div class="control">
                            <p class="control is-expanded">
                                <input class="input" type="text" v-model.trim="newTagName" placeholder="Enter the name of the tag"
                                       v-bind:class="{'is-success': newTagNameIsValid,
                                                      'is-danger': newTagNameIsError}" />
                            </p>
                            <div class="help is-danger" v-if="newTagNameContainsSpaces">Tag names cannot contain spaces.</div>
                            <div class="help is-danger" v-if="newTagNameAlreadyExists">That tag already exists.</div>
                        </div>
                    </div>

                    <div class="field">
                        <label class="label">Tag content</label>

                        <p class="control is-expanded">
                            <input class="input" type="text" v-model.trim="newTagContent" placeholder="Enter the content that the tag will display when used"
                                   v-bind:class="{'is-success': newTagContentIsValid}" />
                        </p>
                    </div>

                </section>

                <footer class="modal-card-foot level">
                    <div class="level-left">
                        <button class="button is-success" v-bind:disabled="!canCreateNewTag" v-on:click="onTagCreate">Create</button>
                    </div>
                    <div class="level-right">
                        <button class="button is-danger" v-on:click="closeCreateModal">Cancel</button>
                    </div>
                </footer>
            </div>
        </div>

        <div class="modal" v-bind:class="{'is-active': showEditModal}">
            <div class="modal-background" v-on:click="closeEditModal"></div>
            <div class="modal-card">
                <header class="modal-card-head">
                    <p class="modal-card-title">
                        Edit Tag
                    </p>
                    <button class="delete" aria-label="close" v-on:click="closeEditModal"></button>
                </header>
                <section class="modal-card-body">

                    <div class="field">
                        <label class="label">Tag name</label>
                        <div class="input" typeof="text" v-bind:disabled="true">{{toEditName}}</div>
                    </div>

                    <div class="field">
                        <label class="label">Tag content</label>

                        <p class="control is-expanded">
                            <input class="input" type="text" v-model.trim="toEditContent" placeholder="Enter the content that the tag will display when used"
                                   v-bind:class="{'is-success': toEditContentIsValid}" />
                        </p>
                    </div>

                </section>

                <footer class="modal-card-foot level">
                    <div class="level-left">
                        <button class="button is-success" v-bind:disabled="!toEditContentIsValid" v-on:click="confirmEdit">Save</button>
                    </div>
                    <div class="level-right">
                        <button class="button is-danger" v-on:click="closeEditModal">Cancel</button>
                    </div>
                </footer>
            </div>
        </div>

        <ConfirmationModal v-bind:isShown="showDeleteConfirmation" v-on:modal-confirmed="confirmDelete" v-on:modal-cancelled="cancelDelete" />

    </div>
</template>

<script lang="ts">
import * as _ from 'lodash';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import TinyUserView from '@/components/TinyUserView.vue';
import Autocomplete from '@/components/Autocomplete.vue';
import ConfirmationModal from '@/components/ConfirmationModal.vue';
import store from "@/app/Store";
import { VueGoodTable } from 'vue-good-table';
import GuildUserIdentity from '@/models/core/GuildUserIdentity';
import DesignatedChannelMapping from '@/models/moderation/DesignatedChannelMapping';
import TagSummary from '@/models/Tags/TagSummary';
import TagService from '@/services/TagService';
import TagCreationData from '../../models/Tags/TagCreationData';
import TagMutationData from '../../models/Tags/TagMutationData';

const messageResolvingRegex = /<#(\d+)>/gm;

const guildUserFilter = (subject: GuildUserIdentity, filter: string) =>
{
    filter = _.lowerCase(filter);

    return subject.id.toString().startsWith(filter) ||
        _.lowerCase(subject.displayName).indexOf(filter) >= 0;
};

const guildUserSort = (x: GuildUserIdentity, y: GuildUserIdentity, col: any, rowX: any, rowY: any) =>
{
    return (x.id < y.id ? -1 : (x.id > y.id ? 1 : 0));
};

const guildUserFormat = (user: GuildUserIdentity) => user.displayName;

@Component({
    components:
    {
        HeroHeader,
        VueGoodTable,
        TinyUserView,
        Autocomplete,
        ConfirmationModal
    }
})
export default class Tags extends Vue
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
    newTagName: string = "";
    newTagContent: string = "";

    showEditModal: boolean = false;
    toEditName: string = "";
    toEditContent: string = "";

    showDeleteConfirmation: boolean = false;
    toDelete: string = "";

    channelCache: { [channel: string]: DesignatedChannelMapping } | null = null;

    get canCreate(): boolean
    {
        return store.userHasClaims(["CreateTag"]);
    }

    canMaintainTag(creator: GuildUserIdentity): boolean
    {
        return store.currentUser()!.userId == creator.id.toString()
            || store.userHasClaims(["MaintainOtherUserTag"]);
    }

    resolveMentions(description: string)
    {
        let replaced = description;

        if (this.channelCache)
        {
            replaced = description.replace(messageResolvingRegex, (sub, args: string) =>
            {
                if (this.channelCache == null || this.channelCache[args] == null)
                {
                    return args;
                }

                let found = (this.channelCache[args] ? this.channelCache[args].name : args);

                return `<span class='channel'>#${found}</span>`;
            });
        }

        return `<span class='pre'>${replaced}</span>`;
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
                label: 'Created On',
                field: 'date',
                type: 'date',
                dateInputFormat: 'YYYY-MM-DDTHH:mm:ss',
                dateOutputFormat: 'MM/DD/YY, h:mm:ss a',
                width: '160px'
            },
            {
                label: 'Creator',
                field: 'creator',
                type: 'date', //Needed to bypass vue-good-table regression
                sortFn: guildUserSort,
                filterOptions:
                {
                     enabled: true,
                     filterFn: guildUserFilter,
                     filterValue: this.staticFilters["creator"],
                     placeholder: "Filter"
                },
                formatFn: guildUserFormat
            },
            {
                label: 'Content',
                field: 'content',
                formatFn: this.resolveMentions,
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
            },
            {
                label: 'Actions',
                field: 'actions',
                width: '32px'
            }
        ];
    }

    get mappedRows(): any[]
    {
        return _.map(this.tags, tag =>
        ({
            name: tag.name,
            date: tag.createAction.created,
            creator: tag.createAction.createdBy,
            content: tag.content,
            uses: tag.uses
        }));
    }

    async refresh(): Promise<void>
    {
        this.isLoading = true;

        this.tags = await TagService.getTags();
        await store.retrieveChannels();

        this.channelCache = _.keyBy(this.$store.state.modix.channels, channel => channel.id);

        this.clearNewTagData();

        this.isLoading = false;
    }

    clearNewTagData(): void
    {
        this.newTagName = "";
        this.newTagContent = "";
    }

    clearToEditData(): void
    {
        this.toEditName = "";
        this.toEditContent = "";
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

    async created(): Promise<void>
    {
        await this.refresh();
        this.applyFilters();
    }

    get newTagNameContainsSpaces(): boolean
    {
        return this.newTagName.indexOf(" ") >= 0;
    }

    get newTagNameAlreadyExists(): boolean
    {
        return _.some(this.tags, (tag: TagSummary) => tag.name == this.newTagName);
    }

    get newTagNameIsValid(): boolean
    {
        return this.newTagName.length > 0
            && !this.newTagNameContainsSpaces
            && !this.newTagNameAlreadyExists;
    }

    get newTagNameIsError(): boolean
    {
        return this.newTagNameContainsSpaces
            || this.newTagNameAlreadyExists;
    }

    get newTagContentIsValid(): boolean
    {
        return this.newTagContent.length > 0;
    }

    get canCreateNewTag(): boolean
    {
        return this.newTagNameIsValid
            && this.newTagContentIsValid;
    }

    closeCreateModal(): void
    {
        this.clearNewTagData();
        this.showCreateModal = false;
    }

    closeEditModal(): void
    {
        this.clearToEditData();
        this.showEditModal = false;
    }

    async onTagCreate(): Promise<void>
    {
        let newTag = new TagCreationData();
        newTag.content = this.newTagContent;

        await TagService.createTag(this.newTagName, newTag);

        await this.refresh();
        this.closeCreateModal();
    }

    onTagEdit(name: string, content: string): void
    {
        this.toEditName = name;
        this.toEditContent = content;
        this.showEditModal = true;
    }

    get toEditContentIsValid(): boolean
    {
        return this.toEditContent.length > 0;
    }

    async confirmEdit(): Promise<void>
    {
        let editedTag = new TagMutationData();
        editedTag.content = this.toEditContent;

        await TagService.updateTag(this.toEditName, editedTag);

        await this.refresh();
        this.closeEditModal();
    }

    cancelEdit(): void
    {
        this.showEditModal = false;
        this.toEditName = "";
        this.toEditContent = "";
    }

    onTagDelete(name: string): void
    {
        this.toDelete = name;
        this.showDeleteConfirmation = true;
    }

    async confirmDelete(): Promise<void>
    {
        this.showDeleteConfirmation = false;
        await TagService.deleteTag(this.toDelete);
        this.toDelete = "";
        await this.refresh();
    }

    cancelDelete(): void
    {
        this.showDeleteConfirmation = false;
        this.toDelete = "";
    }
}
</script>
