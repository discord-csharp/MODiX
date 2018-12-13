<template>
    <div>
        <Sidebar v-bind:title="'Logs'" v-bind:routeName="'logs'" />
    </div>
</template>

<script lang="ts">

    import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
    import * as _ from 'lodash';
    import Sidebar from '@/components/Logs/Sidebar.vue';

    @Component({
        components:
        {
            Sidebar
        }
    })
    export default class Logs extends Vue
    {
    }

</script>
