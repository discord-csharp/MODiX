<template>

    <section class="hello hero is-primary" v-if="text">

        <div class="hero-body">
            <div class="container">
                <h1 class="title">
                    {{text}}
                </h1>
                <h2 class="subtitle">
                    <slot />
                </h2>
            </div>
        </div>

    </section>

</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component
export default class HeroHeader extends Vue
{
    // @ts-ignore
    @Prop() private text: string;
}
</script>
