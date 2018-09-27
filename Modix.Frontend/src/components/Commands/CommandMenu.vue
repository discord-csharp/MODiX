<template>

    <aside class="menu">
        <div v-for="commandModule in commandModules" :key="commandModule.name">
            <p class="menu-label">
                <strong v-if="commandModule.name == highlightedModule">{{commandModule.name}}</strong>
                <a v-else v-on:click="$emit('moduleClicked', commandModule)">{{commandModule.name}}</a>
            </p>
        </div>
    </aside>

</template>

<style lang="scss" scoped>
@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";

@import "~bulma/sass/components/menu";

.menu
{
    position: sticky;
    top: 1px;

    overflow-y: auto;
    max-height: 100vh;
}

.menu-label
{
    margin: 1em 0em;
}
</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { ModuleHelpData, CommandHelpData } from '@/models/ModuleHelpData';
import CommandView from '@/components/Commands/CommandView.vue';
import * as _ from 'lodash';
import { Module } from 'vuex';

@Component({
    components:
    {
        CommandView
    },
})
export default class CommandMenu extends Vue
{
    @Prop() private commandModules!: ModuleHelpData[];
    @Prop() private highlightedModule!: string;
}
</script>
