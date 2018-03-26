<template>
    <section class="hello">
        <div class="notification is-danger" v-for="(error, index) in $store.state.modix.errors" v-bind:key="index">
            <button class="delete" v-on:click="removeError(error)"></button>
            {{error}}
        </div>
    </section>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import * as store from "../app/Store";

@Component
export default class ErrorView extends Vue
{
    removeError(error: string)
    {
        store.removeError(this.$store, error);
    }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">

.delete
{
    cursor: pointer;

    line-height: 0;

    width: 24px;
    height: 24px;

    background: rgba(0, 0, 0, 0.5);
    border: 0;

    color: white;

    border-radius: 8em;

    &::after
    {
        content: 'X';
    }
}

</style>
