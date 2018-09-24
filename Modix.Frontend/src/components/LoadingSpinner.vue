<template>
    <div class="loader" :class="{'complete': !visible}">
        <div class="spinner"></div>
        <img class="spinnerCenter" src="../assets/icon.png" />
    </div>
</template>

<style lang="scss">

@import "../styles/variables";
@import "~bulma/sass/utilities/_all";

@keyframes fadeOut
{
    0%
    {
        transform: scale(1);
    }
    100%
    {
        transform: scale(0);

        visibility: hidden;
        pointer-events: none;
    }
}

.loader
{
    background: $primary;
    position: fixed;

    width: 6000px;
    height: 6000px;

    top: 50%;
    left: 50%;

    margin-top: -3000px;
    margin-left: -3000px;

    border-radius: 9999em;

    transform-origin: 50% 50%;
    z-index: 999;
}

.loader.complete
{
    animation: fadeOut $default-transition forwards;
    animation-delay: 300ms;
}

#app .spinner
{
    @include loader();

    background: $primary;

    position: relative;

    border-width: 48px;
    top: calc(50% - 300px);
    left: calc(50% - 300px);
    
    width: 600px;
    height: 600px;

    border-color: transparent;

    animation-duration: 0.5s;
    animation-iteration-count: 4;

    box-shadow: -12px 12px 24px -10px rgba(0, 0, 0, 0.25), 6px -6px 32px -16px inset rgba(255, 255, 255, 0.33);
}

.spinnerCenter
{
    position: absolute;

    top: calc(50% - 150px);
    left: calc(50% - 150px);
}

</style>

<script lang="ts">
import Vue from 'vue';
import {Watch, Component, Prop} from 'vue-property-decorator';

@Component({})
export default class App extends Vue
{
    @Prop()
    visible!: boolean;
}
</script>
