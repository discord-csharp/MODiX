import _ from 'lodash';
import Vue from 'vue';
import Router from 'vue-router';
import store from '@/app/Store';
import { getCookie } from '@/app/Util';
import Commands from './views/Commands.vue';
import CreatePromotion from './views/CreatePromotion.vue';
import Home from './views/Home.vue';
import Infractions from './views/Infractions.vue';
import Promotions from './views/Promotions.vue';
import Configuration from './views/Configuration/Configuration.vue';
import ChannelDesignations from './views/Configuration/ChannelDesignations.vue';
import RoleDesignations from './views/Configuration/RoleDesignations.vue';
import Claims from './views/Configuration/Claims.vue';
import Stats from './views/Stats.vue';
import ModixRoute, { ModixRouteData } from '@/app/ModixRoute';
import {toTitleCase} from './app/Util';

Vue.use(Router)

let routes: ModixRouteData[] =
[
    {
        path: '/',
        name: 'home',
        component: Home,
        showInNavbar: false
    },
    {
        path: '/stats',
        name: 'stats',
        component: Stats,
        showInNavbar: true,
        requiresAuth: true
    },
    {
        path: '/commands',
        name: 'commands',
        component: Commands,
        showInNavbar: true
    },
    {
        path: '/promotions',
        name: 'promotions',
        component: Promotions,
        showInNavbar: true,
        requiresAuth: true
    },
    {
        path: '/promotions/create',
        name: 'createPromotion',
        title: "Start a Campaign",
        component: CreatePromotion,
        showInNavbar: false,
        requiredClaims: ["PromotionsCreateCampaign"]
    },
    {
        path: '/infractions',
        name: 'infractions',
        component: Infractions,
        showInNavbar: true,
        requiredClaims: ["ModerationRead"]
    },
    {
        path: '/config',
        name: 'config',
        title: 'Configuration',
        component: Configuration,
        showInNavbar: false,
        requiresAuth: true,
        children: 
        [
            {
                path: 'roles',
                name: 'roles',
                component: RoleDesignations,
                requiredClaims: ["DesignatedRoleMappingRead"]
            },
            {
                path: 'channels',
                name: 'channels',
                component: ChannelDesignations,
                requiredClaims: ["DesignatedChannelMappingRead"]
            },
            {
                path: 'claims',
                name: 'claims',
                component: Claims,
                requiredClaims: ["AuthorizationConfigure"]
            }
        ]
    },
    {
        path: '/error',
        name: 'error',
        showInNavbar: false,
        beforeEnter: (to, from, next) =>
        {
            let errorCookie = getCookie("Error");

            if (errorCookie)
            {
                store.pushErrorMessage(errorCookie);
            }

            next('/');
        }
    }
];

const router = new Router
({
    mode: "history",
    routes: _.map(routes, routeData => new ModixRoute(routeData).asVueRoute())
});

router.beforeEach(async (to, from, next) =>
{
    if (!store.hasTriedAuth())
    {
        await store.retrieveUserInfo()
    }

    let toRoute: ModixRoute = to.meta;

    if (toRoute.requiresAuth && !store.isLoggedIn())
    {
        store.pushErrorMessage(`You need to log in before accessing <code>${to.fullPath}</code>`);
        next('/');
    }
    else if (to.matched.length == 0)
    {
        store.pushErrorMessage(`Page not found: <code>${to.fullPath}</code>`);
        next('/');
    }
    else if (toRoute.routeData.requiredClaims && !store.userHasClaims(toRoute.routeData.requiredClaims))
    {
        store.pushErrorMessage(`You are not authorized to view <code>${to.fullPath}</code>. Required claims: ` +
            toRoute.routeData.requiredClaims.join(', '));
            
        next('/');
    }
    else
    {
        next();
    }
});

router.afterEach((to, from) =>
{
    let toRoute: ModixRoute = to.meta;
    document.title = "Modix - " + toTitleCase(toRoute.title);
});

router.onError((err: Error) =>
{
    store.pushErrorMessage(err.message);
});

export default router;
