<template>

    <div class="control" :title="param.summary">
        <div class="parameter tags has-addons">
            <span class="tag is-dark" :class="{'pointer': param.summary}">{{param.name}}</span>                                    
            <span class="tag is-info">{{param.type}}</span>
            <span v-if="param.isOptional" class="tag is-warning">Optional</span>
        </div>
    </div>

</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { ParameterHelpData } from '@/models/ModuleHelpData';

@Component
export default class ParameterView extends Vue
{
    // @ts-ignore
    @Prop() private param: ParameterHelpData;
}
</script>
