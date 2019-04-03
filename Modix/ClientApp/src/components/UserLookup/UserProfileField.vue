<template>
    <div class="profileField">
        <label class="label">{{fieldName}}</label>
        <div class="value" v-html="display"></div>
    </div>
</template>

<style lang="scss" scoped>
.profileField
{
    display: flex;
    flex-direction: row;

    .label
    {

    }

    .value
    {
        margin-left: 0.66em;
    }
}
</style>

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

    @Prop({default: ''})
    default!: string;

    get display()
    {
        return this.fieldValue ? this.fieldValue : `<em>${this.default}</em>`;
    }
}
</script>
