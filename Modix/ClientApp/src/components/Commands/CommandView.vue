<template>

    <li class="command box">

        <div :class="{'field is-grouped is-grouped-multiline': command.parameters.length > 0}" v-for="(alias, index) in command.aliases" :key="index">
            <span class="commandName">
                !{{alias.toLowerCase()}}
            </span>

            <div class="summary" v-if="alias == command.aliases[0]">
                {{command.summary}}
            </div>

            <template v-if="alias == command.aliases[0]">
                <ParameterView v-for="param in command.parameters" :key="param.name" :param="param" />
            </template>
            <template v-else>
                <div v-if="command.parameters.length > 0" class="spacer">&nbsp;</div>
            </template>

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
    @Prop() private command!: CommandHelpData;
}
</script>
