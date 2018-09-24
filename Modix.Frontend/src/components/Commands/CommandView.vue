<template>

    <li class="command box">

        <div :class="{'field is-grouped is-grouped-multiline': overload.parameters.length > 0}" v-for="(overload, index) in commandGroup" :key="index">
            <span class="commandName" v-if="overload == commandGroup[0] || isAlias(overload)">
                !{{overload.alias.toLowerCase()}}
            </span>
            <span class="commandName overload" v-else>
                or
            </span>
            
            <div class="summary" v-if="!isAlias(overload)">
                {{overload.summary}}
            </div>

            <template v-if="!isAlias(overload)">
                <ParameterView v-for="param in overload.parameters" :key="param.name" :param="param" />
            </template>
            <template v-else>
                <div v-if="overload.parameters.length > 0" class="spacer">&nbsp;</div>
            </template>

        </div>

    </li>

</template>

<style lang="scss" scoped>
@import "../../styles/variables";
@import "~bulma/sass/utilities/_all";
@import "~bulma/sass/elements/box";

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
    }

    .overload
    {
        color: gray;
    }
}

.spacer
{
    margin-bottom: 1em;
    color: gray;
}

</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { CommandHelpData } from '@/models/ModuleHelpData';
import ParameterView from '@/components/Commands/ParameterView.vue';

@Component({
    components:
    {
        ParameterView
    }
})
export default class CommandView extends Vue
{
    @Prop() private commandGroup!: CommandHelpData[];

    isAlias(overload: CommandHelpData)
    {
        return (
            overload.alias != this.commandGroup[0].alias &&
            overload.alias != this.commandGroup[0].summary &&
            overload.alias != overload.name
        );
    }
}
</script>
