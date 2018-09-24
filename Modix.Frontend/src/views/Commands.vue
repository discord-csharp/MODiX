<template>
    <div>
        <section class="section">
            <div class="container">
                <div class="columns">

                    <div class="column is-one-fifth is-hidden-mobile">
                        <CommandMenu :commandModules="commandModules" :highlightedModule="highlightedModule" 
                            @moduleClicked="scrollTo($event)" />
                    </div>
                
                    <div class="column">
                        <ModuleView v-for="commandModule in commandModules" :key="commandModule.name" :commandModule="commandModule"
                            @moduleClicked="scrollTo($event)" />
                    </div>
                    
                </div>
            </div>
        </section>
    </div>
</template>

<style lang="scss">

@import "../styles/variables";
@import "~bulma/sass/utilities/_all";

.hidden
{
    opacity: 0;
}

.parameter
{
    
}

.title
{
    text-transform: capitalize;
}

.summary
{
    display: inline-block;
    margin-right: 1em;
}

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

.highlighted
{
    font-weight: bold;
}

</style>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';
import store from "@/app/Store";
import {ModuleHelpData} from "../models/ModuleHelpData";
import ModuleView from '@/components/Commands/ModuleView.vue';
import CommandMenu from '@/components/Commands/CommandMenu.vue';
import * as _ from 'lodash';

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
        return _.orderBy(this.$store.state.modix.commands, (module: ModuleHelpData) => module.name.toLowerCase());
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
        store.retrieveCommands();
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
