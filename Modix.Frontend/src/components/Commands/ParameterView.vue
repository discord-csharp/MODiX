<template>

    <div class="control" :class="{'pointer': hasDescription}">
        <div class="parameter tags has-addons" @click="toggle()">
            <span class="tag is-dark">
                {{param.name}}
            </span>
            <span class="tag description" v-tooltip.top-center="{content: description, trigger: 'hover click'}" v-if="hasDescription">…</span>                          
            <span class="tag is-info">{{param.type}}</span>
            <span v-if="param.isOptional" class="tag pointer is-warning" 
                v-tooltip.top-center="{content: 'Optional', trigger: 'hover click'}">?</span>
        </div>
    </div>

</template>

<style scoped lang="scss">
@import "../../styles/variables";
@import "~bulma/sass/elements/tag";

.tag:not(body).is-dark
{
    color: white !important;
}

.tooltip-inner 
{
    font-size: 14px;

    strong
    {
        margin-left: 0.5em;
        padding-left: 0.5em;

        border-left: 1px solid lightgrey;
    }
}

</style>

<style scoped lang="scss">
@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/tag";
@import "~bulma/sass/elements/form";

.pointer
{
    cursor: pointer;
}
</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { ParameterHelpData } from '@/models/ModuleHelpData';

@Component
export default class ParameterView extends Vue
{
    @Prop() private param!: ParameterHelpData;

    showSummary: boolean = false;

    toggle()
    {
        if (this.hasDescription)
        {
            this.showSummary = !this.showSummary;
        }
    }

    get hasDescription()
    {
        return (this.param.summary || this.param.options.length > 0);
    }

    get description(): string
    {
        let ret = this.param.summary;

        if (this.param.options.length > 0)
        {
            let optionsJoined = this.param.options.join(', ');
            ret += ` <span class='has-text-weight-bold'>${optionsJoined}</span>`;
        }

        return ret;
    }
}
</script>
