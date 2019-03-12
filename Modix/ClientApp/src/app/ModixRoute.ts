import Vue, { ComponentOptions, AsyncComponent } from 'vue';
import { RouteConfig, Route } from 'vue-router';
import _ from 'lodash';

export enum RouteType
{
    Redirect = "redirect",
    Normal = "normal"
}

type MixedRoute = ModixRouteData & RedirectRouteData;

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
    isButton?: boolean;
    type: RouteType.Normal;

    beforeEnter?: (to: Route, from: Route, next: any) => void;
}

export interface RedirectRouteData
{
    path: string;
    redirectTo: string;
    type: RouteType.Redirect;
}

export default class ModixRoute
{
    routeData: (ModixRouteData | RedirectRouteData);
    optionalClaims: string[] = [];

    constructor(data: (ModixRouteData | RedirectRouteData))
    {
        this.routeData = data;

        if (this.routeData.type == RouteType.Normal)
        {
            for (let claim of _.flatMap(this.routeData.children, child => child.requiredClaims))
            {
                this.optionalClaims.push(claim as any);
            }
        }
    }

    get requiresAuth()
    {
        if (this.routeData.type == RouteType.Redirect)
        {
            return false;
        }

        return this.routeData.requiresAuth || (this.routeData.requiredClaims && this.routeData.requiredClaims.length > 0);
    }

    get requiredClaims(): string[]
    {
        if (this.routeData.type == RouteType.Redirect)
        {
            return [];
        }

        return this.routeData.requiredClaims || [];
    }

    get isButton(): boolean
    {
        if (this.routeData.type == RouteType.Redirect)
        {
            return false;
        }

        return (this.routeData.isButton == undefined ? false : this.routeData.isButton);
    }

    get title(): string
    {
        if (this.routeData.type == RouteType.Redirect)
        {
            return "Redirecting...";
        }

        return this.routeData.title || this.routeData.name;
    }

    asVueRoute(): RouteConfig
    {
        let ret: RouteConfig = { path: "" };

        switch (this.routeData.type)
        {
            case RouteType.Redirect:
                ret = {
                    path: this.routeData.path,
                    redirect: { name: this.routeData.redirectTo }
                };
                break;
            default:
                ret = {
                    path: this.routeData.path,
                    name: this.routeData.name,
                    component: this.routeData.component,
                    beforeEnter: this.routeData.beforeEnter,
                    children: _.map(this.routeData.children, route => new ModixRoute(route as any).asVueRoute()),
                    meta: this
                };
                break;
        }

        return ret;
    }
}