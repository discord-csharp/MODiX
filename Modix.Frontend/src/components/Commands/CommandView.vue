<template>

    <li class="command box">

        <div :class="{'field is-grouped is-grouped-multiline': overload.parameters.length > 0}" v-for="(overload, index) in commandGroup" :key="index">
            <strong class="commandName" v-if="overload == commandGroup[0]">
                !{{overload.name.toLowerCase()}}
            </strong>
            <span class="commandName overload" v-else>
                or
            </span>
            
            <div class="summary" v-if="overload.summary">
                {{overload.summary}}
            </div>

            <ParameterView v-for="param in overload.parameters" :key="param.name" :param="param" />
        </div>

    </li>

</template>

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
    // @ts-ignore
    @Prop() private commandGroup: CommandHelpData[];
}
</script>
