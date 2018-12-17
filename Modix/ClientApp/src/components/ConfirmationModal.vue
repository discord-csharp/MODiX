<template>

    <div class="modal" v-bind:class="{ 'is-active': isShown }">
        <div class="modal-background" v-on:click="cancelModal" />
        <div class="modal-card">

            <header class="modal-card-head">
                <p class="modal-card-title">
                    {{title}}
                </p>
                <button class="delete" aria-label="close" v-on:click="cancelModal" />
            </header>

            <section class="modal-card-body">
                <label class="label">{{mainText}}</label>
            </section>

            <footer class="modal-card-foot level">
                <div class="level-left">
                    <button class="button is-success" v-on:click="confirmModal">{{confirmButtonText}}</button>
                </div>
                <div class="level-right">
                    <button class="button is-danger" v-on:click="cancelModal">{{cancelButtonText}}</button>
                </div>
            </footer>

        </div>
    </div>

</template>

<script lang="ts">
    import { Component, Prop, Vue } from 'vue-property-decorator';
    import * as _ from 'lodash';

    @Component({})
    export default class ConfirmationModal extends Vue
    {
        @Prop({ default: false })
        public isShown!: boolean;

        @Prop({ default: "Confirmation" })
        public title!: string;

        @Prop({ default: "Are you sure you want to perform this action?" })
        public mainText!: string;

        @Prop({ default: "Confirm" })
        public confirmButtonText!: string;

        @Prop({ default: "Cancel" })
        public cancelButtonText!: string;

        confirmModal(): void
        {
            this.$emit('modal-confirmed');
        }

        cancelModal(): void
        {
            this.$emit('modal-cancelled');
        }
    }
</script>
