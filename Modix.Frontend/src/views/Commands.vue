<template>
    <div>
        <HeroHeader text="Commands" />
    
        <section class="section">
            <div class="container">
                <div class="columns">

                    <div class="column is-one-fifth">
                        <CommandMenu :commandModules="commandModules" :highlightedModule="highlightedModule" @moduleClicked="scrollTo($event)" />
                    </div>
                
                    <div class="column">
                        <ModuleView v-for="commandModule in commandModules" :key="commandModule.name" :commandModule="commandModule" />
                    </div>
                    
                </div>
            </div>
        </section>
    </div>
</template>

<style lang="scss">

@import "../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/box";
@import "~bulma/sass/elements/tag";
@import "~bulma/sass/elements/form";
@import "~bulma/sass/components/menu";

.command
{
    background: $light;
    padding: 0.5em 1em;

    .commandName
    {
        display: inline-block;
        font-family: "Consolas", monospace;
        font-size: 1.15em;
        margin-right: 1em;
        font-weight: bold;

        &.overload
        {
            text-align: right;
        }
    }
}

.hidden
{
    opacity: 0;
}

.parameter
{
    
}

.summary
{
    display: inline-block;
    margin-right: 1em;
}

.menu
{
    position: sticky;
    top: 20px;
}

.menu-label
{
    margin: 1em 0em;
}

.highlighted
{
    font-weight: bold;
}

.pointer
{
    cursor: pointer;
    
    &::after
    {
        content: '…';
    }
}

</style>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import * as store from "../app/Store";
import {ModuleHelpData} from "../models/ModuleHelpData";
import ModuleView from '@/components/Commands/ModuleView.vue';
import CommandMenu from '@/components/Commands/CommandMenu.vue';

@Component({
    components:
    {
        HeroHeader,
        ModuleView,
        CommandMenu
    },
})
export default class Commands extends Vue
{
    highlightedModule: string | null = null;

    get commandModules(): ModuleHelpData[]
    {
        return this.$store.state.modix.commands;
    }

    get hash()
    {
        return decodeURIComponent(this.$route.hash.substring(1));
    }

    scrollTo(commandModule: ModuleHelpData)
    {
        location.hash = "#" + commandModule.name;
        this.highlightedModule = commandModule.name;
    }

    created()
    {
        store.updateCommands(this.$store);
    }

    updated()
    {
        let nodes = document.getElementsByName(this.hash);

        if (nodes.length > 0)
        {
            nodes[0].scrollIntoView();
            this.highlightedModule = nodes[0].getAttribute("name");
        }
    }
}
</script>
