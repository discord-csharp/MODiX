import ModixRoute, { ModixRouteData, RedirectRouteData, RouteType } from '@/app/ModixRoute';
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
const Infractions = () => import('./components/Logs/Infractions.vue').then(m => m.default)
const Promotions = () => import('./views/Promotions.vue').then(m => m.default)
const Stats = () => import('./views/Stats.vue').then(m => m.default)
const Tags = () => import('./views/Tags/Tags.vue').then(m => m.default)
const Logs = () => import('./views/Logs.vue').then(m => m.default)
const DeletedMessages = () => import('./components/Logs/DeletedMessages.vue').then(m => m.default)
const UserLookup = () => import('./views/UserLookup.vue').then(m => m.default)

Vue.use(Router)

let routes: (ModixRouteData | RedirectRouteData)[] =
[
    {
        path: '/',
        name: 'home',
        component: Home,
        showInNavbar: false,
        type: RouteType.Normal
    },
    {
        path: '/stats',
        name: 'stats',
        component: Stats,
        showInNavbar: true,
        requiresAuth: true,
        type: RouteType.Normal
    },
    {
        path: '/commands',
        name: 'commands',
        component: Commands,
        showInNavbar: true,
        type: RouteType.Normal
    },
    {
        path: '/userlookup',
        name: 'userlookup',
        title: 'User Lookup',
        component: UserLookup,
        showInNavbar: true,
        requiresAuth: true,
        type: RouteType.Normal,
    },
    {
        path: '/tags',
        name: 'tags',
        component: Tags,
        showInNavbar: true,
        requiresAuth: true,
        type: RouteType.Normal
    },
    {
        path: '/promotions',
        name: 'promotions',
        component: Promotions,
        showInNavbar: true,
        requiredClaims: ["PromotionsRead"],
        type: RouteType.Normal
    },
    {
        path: '/promotions/create',
        name: 'createPromotion',
        title: "Start a Campaign",
        component: CreatePromotion,
        showInNavbar: false,
        requiredClaims: ["PromotionsCreateCampaign"],
        type: RouteType.Normal
    },
    {
        path: '/infractions',
        redirectTo: 'infractions',
        type: RouteType.Redirect
    },
    {
        path: '/logs',
        name: 'logs',
        component: Logs,
        showInNavbar: true,
        requiresAuth: true,
        type: RouteType.Normal,
        children:
        [
            {
                path: 'deletedMessages',
                name: 'deletedMessages',
                title: 'Deletions',
                component: DeletedMessages,
                type: RouteType.Normal,
                requiredClaims: ["LogViewDeletedMessages"]
            },
            {
                path: 'infractions',
                name: 'infractions',
                component: Infractions,
                showInNavbar: true,
                type: RouteType.Normal,
                requiredClaims: ["ModerationRead"]
            }
        ]
    },
    {
        path: '/config',
        name: 'config',
        title: '🛠',
        component: Configuration,
        isButton: true,
        showInNavbar: true,
        requiresAuth: true,
        type: RouteType.Normal,
        children:
        [
            {
                path: 'roles',
                name: 'roles',
                component: RoleDesignations,
                type: RouteType.Normal,
                requiredClaims: ["DesignatedRoleMappingRead"]
            },
            {
                path: 'channels',
                name: 'channels',
                component: ChannelDesignations,
                type: RouteType.Normal,
                requiredClaims: ["DesignatedChannelMappingRead"]
            },
            {
                path: 'claims',
                name: 'claims',
                component: Claims,
                type: RouteType.Normal,
                requiredClaims: ["AuthorizationConfigure"]
            }
        ]
    },
    {
        path: '/error',
        name: 'error',
        showInNavbar: false,
        type: RouteType.Normal,
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
    if (from.name == null && !store.hasTriedAuth())
    {
        await store.retrieveUserInfo();
        if (store.isLoggedIn())
        {
            await store.retrieveGuilds();
        }
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
    else if (toRoute.requiredClaims && !store.userHasClaims(toRoute.requiredClaims))
    {
        store.pushErrorMessage(`You are not authorized to view <code>${to.fullPath}</code>. Required claims: ` +
            toRoute.requiredClaims.join(', '));

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
