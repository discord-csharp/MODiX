<template>
    <div class="field is-horizontal" style="margin-top: 0em; margin-bottom: -0.25em;">
        <label class="label">{{fieldName}}</label>
        &nbsp;&nbsp;
        <div v-html="fieldValue" />
    </div>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';

@Component({})
export default class UserProfile extends ModixComponent
{
    @Prop()
    fieldName!: string;

    @Prop()
    fieldValue!: string;
}
</script>
