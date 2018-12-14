<template>
    <section class="errors">
        <div class="notification is-danger" v-for="(error, index) in errors" v-bind:key="index">
            <button class="delete" v-on:click="removeError(error)"></button>
            <span v-html="error"></span>
        </div>
    </section>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import store from "@/app/Store";

@Component
export default class ErrorView extends Vue
{
    removeError(error: string)
    {
        store.removeErrorMessage(error);
    }

    get errors()
    {
        return this.$store.state.modix.errors;
    }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">

.notification.is-danger strong
{
    color: white !important;
}

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
