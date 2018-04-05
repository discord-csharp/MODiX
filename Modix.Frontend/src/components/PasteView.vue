<template>

    <div class="paste">
        <template v-if="includeHeader">
            <router-link :to="{ name: 'pastes', params: { routePasteId: paste.id }}">
                <strong>{{paste.creatorUsername}}</strong> in <strong>#{{paste.channelName}}</strong>
                <span class="has-text-grey-light date">{{getFormatedDate(paste.created)}}</span>
            </router-link>
        </template>

        <pre>{{paste.content}}</pre>
    </div>

</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

import UserCodePaste from '@/models/UserCodePaste';
import {formatPasteDate} from '../app/Util';

@Component
export default class PasteView extends Vue
{
    // @ts-ignore
    @Prop() private paste: UserCodePaste;
    // @ts-ignore
    @Prop({default: true}) private includeHeader: boolean;

    getFormatedDate(date: Date)
    {
        return formatPasteDate(date);
    }
}
</script>
