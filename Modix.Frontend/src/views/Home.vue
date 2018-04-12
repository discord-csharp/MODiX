<template>
    <div>
        <HeroHeader text="The Best Bot on Discord" />
        <section class="section">
            <div class="container columns">

                <div class="column">
                    <p class="subtitle">
                        Modix (stylized MODiX) is a general purpose bot written for use on the 
                        <a :href="inviteUrl">C# Discord Server</a>. It includes administrative features, a C#
                        compiler / eval module, documentation search, and most importantly: cat pictures. 
                    </p>
                    <p class="subtitle">
                        Modix is constantly in development - if you'd like to contribute, stop by <a :href="inviteUrl">the server</a>,
                        and check out our <a href="https://github.com/discord-csharp/MODiX">GitHub repo</a>!
                    </p>
                </div>

                <!--
                <div class="column">
                    <iframe src="https://discordapp.com/widget?id=143867839282020352&amp;theme=light" allowtransparency="true" frameborder="0"></iframe>
                </div>
                -->
                
            </div>
        </section>
    </div>
</template>

<style>

iframe
{
    width: 100%;
    height: 640px;
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
    inviteUrl: string = "http://aka.ms/csharp-discord";
}
</script>
