<template>
    <div>
        <HeroHeader text="The Best Bot on Discord" />
        <section class="section">
            <div class="container">
                <p class="subtitle">
                    Modix (stylized MODiX) is a general purpose bot written for use on the 
                    <a href="http://aka.ms/discord-csharp">C# Discord Server</a>. It includes administrative features, a C#
                    compiler / eval module, documentation search, and most importantly: cat pictures. 
                </p>
                <p class="subtitle">
                    Modix is constantly in development - if you'd like to contribute, stop by <a href="http://aka.ms/csharp-discord">the server</a>,
                    and check out our <a href="https://github.com/discord-csharp/MODiX">GitHub repo</a>!
                </p>

            </div>
        </section>
    </div>
</template>

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
