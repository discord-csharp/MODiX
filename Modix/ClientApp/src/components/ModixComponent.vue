<script lang="ts">
import Vue from 'vue';
import {Component} from 'vue-property-decorator';
import store from '@/app/Store';
import ModixState from '@/models/ModixState';
import {parseDiscordContent} from '@/app/Util';
@Component({})
export default class ModixComponent extends Vue
{
    get state(): ModixState
    {
        return this.$store.state.modix;
    }

    parseDiscordContent(description: string)
    {
        return parseDiscordContent(this.state, description);
    }
}
</script>
