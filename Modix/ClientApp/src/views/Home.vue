<template>
    <section class="container section">
        <p class="subtitle">
            Modix (stylized MODiX) is a general purpose Discord bot written for use on the
            <a href="http://aka.ms/csharp-discord">C# Discord Server</a>
            by
            <a href="https://github.com/discord-csharp/MODiX/graphs/contributors">developers like you</a>.
            It includes administrative features, a C# compiler / eval module, documentation search, and more.
        </p>
        <p class="subtitle">
            Modix is constantly in development - if you'd like to contribute, stop by the server,
            and check out our <a href="https://github.com/discord-csharp/MODiX">GitHub repo</a>!
        </p>

        <a href="http://aka.ms/csharp-discord" class="inviteButton">
            <img src="https://discord.com/api/guilds/143867839282020352/widget.png?style=banner1" />
        </a>
    </section>
</template>

<style scoped>

.inviteButton
{
    display: block;
    width: 320px;
    margin: 0 auto;
}

</style>


<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import HeroHeader from '@/components/HeroHeader.vue';

@Component({
    components:
    {
        HeroHeader
    },
})
export default class Home extends Vue
{

}
</script>
