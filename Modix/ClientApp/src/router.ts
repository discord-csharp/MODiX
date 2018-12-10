import ModixRoute, { ModixRouteData } from '@/app/ModixRoute';
import store from '@/app/Store';
import { getCookie } from '@/app/Util';
import _ from 'lodash';
import Vue from 'vue';
import Router from 'vue-router';
import { toTitleCase } from './app/Util';

import Home from './views/Home.vue';
const Commands = () => import('./views/Commands.vue').then(m => m.default);
const ChannelDesignations = () => import('./views/Configuration/ChannelDesignations.vue').then(m => m.default)
const Claims = () => import('./views/Configuration/Claims.vue').then(m => m.default)
const Configuration = () => import('./views/Configuration/Configuration.vue').then(m => m.default)
const RoleDesignations = () => import('./views/Configuration/RoleDesignations.vue').then(m => m.default)
const CreatePromotion = () => import('./views/CreatePromotion.vue').then(m => m.default)
const Infractions = () => import('./views/Infractions.vue').then(m => m.default)
const Promotions = () => import('./views/Promotions.vue').then(m => m.default)
const Stats = () => import('./views/Stats.vue').then(m => m.default)
const Tags = () => import('./views/Tags/Tags.vue').then(m => m.default)
const Logs = () => import('./views/Logs.vue').then(m => m.default)
const DeletedMessages = () => import('./components/Logs/DeletedMessages.vue').then(m => m.default)

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
        path: '/tags',
        name: 'tags',
        component: Tags,
        showInNavbar: true,
        requiresAuth: true
    },
    {
        path: '/promotions',
        name: 'promotions',
        component: Promotions,
        showInNavbar: true,
        requiredClaims: ["PromotionsRead"]
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
        path: '/logs',
        name: 'logs',
        component: Logs,
        showInNavbar: true,
        requiresAuth: true,
        children:
        [
            {
                path: 'deletedMessages',
                name: 'deletedMessages',
                title: 'Deleted Messages',
                component: DeletedMessages,
                requiredClaims: ["LogViewDeletedMessages"]
            }
        ]
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
    if (from.name == null && to.name != "home" && !store.hasTriedAuth())
    {
        await store.retrieveUserInfo();
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
