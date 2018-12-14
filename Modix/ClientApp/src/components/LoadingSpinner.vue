<template>
    <div class="loaderRoot" :class="{'complete': !visible}">
        <div class="spinner"></div>
        <img class="spinnerCenter" src="../assets/icon.png" />
        <div class="loadingText" :class="{'hasError': hasError}" v-html="displayText"></div>
    </div>
</template>

<script lang="ts">
import {Component, Prop} from 'vue-property-decorator';
import store from '@/app/Store';
import ModixComponent from '@/components/ModixComponent.vue';
import * as _ from 'lodash';

const messages: string[] = require('../data/loadingMessages.json');

@Component({})
export default class LoadingSpinner extends ModixComponent
{
    @Prop()
    visible!: boolean;

    currentText: string = "Loading...";

    mounted()
    {
        this.currentText = messages[Math.floor(Math.random() * messages.length)];
    }

    get hasError() : boolean
    {
        return this.state.errors.length > 0;
    }

    get displayText(): string
    {
        return this.state.errors.length > 0
            ? "⚠️ Error connecting to backend ⚠️" + (location.host.startsWith("localhost") ? "<br />Did you forget to start the Modix server?" : "")
            : this.currentText;
    }
}
</script>
