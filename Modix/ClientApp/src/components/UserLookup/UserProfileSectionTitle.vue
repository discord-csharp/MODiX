<template>
    <div class="section-title">&#x276f {{title}}</div>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import ModixComponent from '@/components/ModixComponent.vue';

@Component({})
export default class UserProfile extends ModixComponent
{
    @Prop()
    title!: string;
}
</script>
