<template>
    <div>
        <Tabs v-bind:title="'Logs'" v-bind:routeName="'logs'" />
    </div>
</template>

<script lang="ts">

import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import * as _ from 'lodash';
import Tabs from '@/components/Logs/Tabs.vue';

@Component({
    components:
    {
        Tabs
    }
})
export default class Logs extends Vue
{
}

</script>
