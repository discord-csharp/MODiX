<template>
    <canvas class="chart" ref="chart">

    </canvas>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { Chart } from 'chart.js';

import * as store from "@/app/Store";
import { GuildRoleCount } from '@/models/GuildStatApiData';
import { PieChartItem } from '@/models/PieChart';
import {config, Theme} from '@/models/PersistentConfig';

@Component({
    //extends: Doughnut
})
export default class PieChart extends Vue
{
    private pieChart: any | null = null;

    @Prop({default: []})
    public stats!: PieChartItem[];

    get chartCanvas(): HTMLCanvasElement
    {
        return this.$refs.chart as HTMLCanvasElement;
    }

    @Watch('stats')
    updateChart()
    {
        let array = Array.from(this.stats || []);

        this.pieChart.data = {
            labels: array.map(d=> d.name),
            datasets:
            [
                {
                    backgroundColor: array.map(d=> d.color),
                    data: array.map(d=> d.count)
                }
            ]
        };

        this.pieChart.update();
    }

    updated()
    {
        this.updateChart();
    }

    mounted()
    {
        this.pieChart = new Chart(this.chartCanvas,
        {
            type: 'doughnut',
            options:
            {
                responsive: true,
                aspectRatio: 21/9,
                legend:
                {
                    display: true,
                    labels:
                    {
                        fontColor: (config().theme == Theme.Spoopy ? 'rgb(230,230,230)' : 'black')
                    }
                }
            }
        });

        this.updateChart();
    }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">

.chart
{
    height: 350px;
}
</style>
