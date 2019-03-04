<template>
    <div class="section">

        <div class="columns">
            <div class="column is-narrow">
                <aside class="menu">
                    <div>
                        <p class="menu-label">
                            Logs
                        </p>
                        <ul class="menu-list">
                            <li v-for="route in routes" v-bind:key="route.path">
                                <router-link v-bind:class="{ 'is-disabled': !hasClaimsForRoute(route) }" v-tooltip.right-end="claimsFor(route)"
                                            v-bind:key="route.routeData.name" v-bind:to="{ 'name': route.routeData.name }" exact-active-class="is-active"
                                            v-bind:event="!hasClaimsForRoute(route) ? '' : 'click'">
                                    {{toTitleCase(route.title)}}
                                </router-link>
                            </li>
                        </ul>
                    </div>
                </aside>
            </div>

            <div class="column">
                <router-view />
            </div>

        </div>

    </div>
</template>

<script lang="ts">

import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import ModixRoute from '@/app/ModixRoute';
import { toTitleCase } from '@/app/Util';
import store from "@/app/Store";
import * as _ from 'lodash';

@Component({})
export default class Tabs extends Vue
{
    @Prop({ default: "" })
    public title!: string;

    @Prop({ default: "" })
    public routeName!: string;

    get routes(): ModixRoute[]
    {
        let allRoutes = (<any>this.$router).options.routes;
        let currentChildren = _.find(allRoutes, route => route.name == this.routeName).children;

        return <any>_.map(currentChildren, child => child.meta as ModixRoute);
    }

    hasClaimsForRoute(route: ModixRoute): boolean
    {
        return store.userHasClaims(route.requiredClaims);
    }

    toTitleCase(input: string): string
    {
        return toTitleCase(input);
    }

    claimsFor(route: ModixRoute): string
    {
        if (this.hasClaimsForRoute(route))
        {
            return "";
        }

        if (route.requiredClaims.length == 0)
        {
            return "Disallowed";
        }

        return "Required Claims: " + route.requiredClaims.toString();
    }

    mounted()
    {

    }
}

</script>
