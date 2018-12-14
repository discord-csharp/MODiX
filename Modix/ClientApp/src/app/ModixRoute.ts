import Vue, { ComponentOptions, AsyncComponent } from 'vue';
import { RouteConfig, Route } from 'vue-router';
import _ from 'lodash';

export interface ModixRouteData
{
    path: string;
    name: string;
    title?: string;
    component?: ComponentOptions<Vue> | typeof Vue | AsyncComponent;
    requiredClaims?: string[];
    showInNavbar?: boolean;
    children?: ModixRouteData[];
    requiresAuth?: boolean;

    beforeEnter?: (to: Route, from: Route, next: any) => void;
}

export default class ModixRoute
{
    routeData: ModixRouteData;

    constructor(data: ModixRouteData)
    {
        this.routeData = data;
    }

    get requiresAuth()
    {
        return this.routeData.requiresAuth || (this.routeData.requiredClaims && this.routeData.requiredClaims.length > 0);
    }

    get title(): string
    {
        return this.routeData.title || this.routeData.name;
    }

    asVueRoute(): RouteConfig
    {
        let ret: RouteConfig = {
            path: this.routeData.path,
            name: this.routeData.name,
            component: this.routeData.component,
            beforeEnter: this.routeData.beforeEnter,
            children: _.map(this.routeData.children, route => new ModixRoute(route).asVueRoute()),
            meta: this
        }

        return ret;
    }
}