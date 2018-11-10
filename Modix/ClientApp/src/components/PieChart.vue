<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { Doughnut } from 'vue-chartjs';
import ApplicationConfiguration from '@/app/ApplicationConfiguration'
import GuildInfoResult from '@/models/GuildInfoResult'

import * as store from "@/app/Store";

@Component({
    extends: Doughnut
})
export default class PieChart extends Vue
{
    @Prop() private guildName!: string;

    get currentGuild(): GuildInfoResult[]
    {
        return this.$store.state.modix.guildInfo[this.guildName];
    }

    @Watch("guildName")
    guildChanged(newGuild: string, oldGuild: string)
    {
        this.updateChart();
    }

    updateChart()
    {
        if (!this.currentGuild) { return; }

        (<any>this).renderChart(
        {
            labels: Array.from(this.currentGuild).map(d=> d.name),
            datasets:
            [
                {
                    backgroundColor: Array.from(this.currentGuild).map(d=> d.color),
                    data: Array.from(this.currentGuild).map(d=> d.count)
                }
            ]
        },
        {
            responsive: true,
            maintainAspectRatio: false,
            legend:
            {
                display: true,
                labels:
                {
                    fontColor: (ApplicationConfiguration.isSpoopy ? 'rgb(230,230,230)' : 'black')
                }
            }
        });
    }

    mounted()
    {
        this.updateChart();
    }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">

</style>
