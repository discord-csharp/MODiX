<template>
    <div class="modal tagCreation" v-bind:class="{'is-active': shown}">
        <div class="modal-background" @click="emitClose"></div>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">
                    <template v-if="isNewTag">Create Tag</template>
                    <template v-else-if="selectedTag">Edit <strong>{{name}}</strong></template>
                </p>
                <button class="delete" aria-label="close" @click="emitClose"></button>
            </header>
            <section class="modal-card-body">

                <div class="field" v-if="isNewTag">
                    <label class="label">Name</label>
                    <input class="input" type="text" v-model="name" />
                </div>

                <div class="field">
                    <label class="label">Content</label>

                    <p class="control is-expanded">
                        <textarea class="textarea" type="text" v-model.trim="content" placeholder="Enter the content that the tag will display when used">
                        </textarea>
                    </p>
                </div>

                <div class="field">
                    <label class="label">Preview</label>

                    <p class="control is-expanded markdownPreview" v-html="markdownPreview"></p>
                </div>

                <p class="help is-danger" v-if="error">{{error}}</p>

            </section>

            <footer class="modal-card-foot level">
                <div class="level-left">
                    <button class="button is-success" :class="{'is-loading': loading}" :disabled="!isValid" @click="save">Save</button>
                </div>
                <div class="level-right">
                    <button class="button is-danger" @click="emitClose">Cancel</button>
                </div>
            </footer>
        </div>
    </div>
</template>

<script lang="ts">
import * as _ from 'lodash';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import TagSummary from '@/models/Tags/TagSummary';
import TagService from '@/services/TagService';
import ModixComponent from '@/components/ModixComponent.vue';
import { AxiosError } from 'axios';

@Component({})
export default class TagCreationModal extends ModixComponent
{
    @Prop({required: true, default: false})
    shown!: boolean;

    @Prop({required: true, default: null})
    selectedTag!: TagSummary | null;

    @Prop({required: true, default: []})
    allTags!: TagSummary[];

    name: string = "";
    content: string = "";
    loading: boolean = false;
    error: string | null = null;

    @Watch('name')
    nameChange()
    {
        this.checkErrors();
    }

    @Watch('content')
    contentChange()
    {
        this.checkErrors();
    }

    @Watch('shown')
    onShow()
    {
        if (this.selectedTag)
        {
            this.name = this.selectedTag.name;
            this.content = this.selectedTag.content;
        }
        else
        {
            this.reset();
        }
    }

    get isValid(): boolean
    {
        return this.name.length > 0 && this.content.length > 0 && this.error == null;
    }

    get isNewTag(): boolean
    {
        return this.selectedTag == null;
    }

    get markdownPreview()
    {
        return this.parseDiscordContent(this.content);
    }

    checkErrors()
    {
        this.error = null;

        this.checkNameErrors();
        this.checkContentErrors();
    }

    checkNameErrors()
    {
        if (this.name.length <= 0)
        {
            this.error = "Name needs to be specified.";
            return;
        }

        if (this.name.indexOf(" ") >= 0)
        {
            this.error = "Name cannot contain spaces.";
            return;
        }

        if (this.isNewTag && this.allTags.some(tag => tag.name == this.name))
        {
            this.error = `A tag already exists with the name ${this.name}`;
            return;
        }
    }

    checkContentErrors()
    {
        if (this.content.length <= 0)
        {
            this.error = "Content needs to be specified.";
            return;
        }
    }

    async save()
    {
        if (!this.isValid) { return; }

        this.loading = true;

        try
        {
            await this.commitSave();
            this.$emit('submit');
        }
        catch (err)
        {
            let error: AxiosError = err;
            this.error = error.response!.data;
        }
        finally
        {
            this.loading = false;
        }
    }

    async commitSave()
    {
        if (this.isNewTag)
        {
            await TagService.createTag(this.name, {
                content: this.content
            });
        }
        else
        {
            await TagService.updateTag(this.name, {
                content: this.content
            });
        }
    }

    emitClose()
    {
        this.$emit('close');
        this.reset();
    }

    reset()
    {
        this.name = "";
        this.content = "";
        this.error = null;
    }

    mounted()
    {

    }
}

</script>
