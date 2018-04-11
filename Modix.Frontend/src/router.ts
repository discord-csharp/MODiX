import Vue from 'vue'
import Router from 'vue-router'
import Home from './views/Home.vue'
import Stats from './views/Stats.vue'
import Pastes from './views/Pastes.vue'
import SinglePaste from './views/SinglePaste.vue'
import Commands from './views/Commands.vue'
import Promotions from './views/Promotions.vue'
import CreatePromotion from './views/CreatePromotion.vue'

import { getCookie } from './app/Util';

import store from './app/Store';
import { url } from 'inspector';

Vue.use(Router)

const router = new Router
({
    mode: "history",
    routes:
    [
        {
            path: '/',
            name: 'home',
            component: Home,
            meta: { showNav: false }
        },
        {
            path: '/stats',
            name: 'stats',
            component: Stats,
            meta: { showNav: true, requiresAuth: true }
        },
        {
            path: '/pastes',
            name: 'pastes',
            component: Pastes,
            meta: { showNav: false }
        },
        {
            path: '/pastes/:routePasteId',
            name: 'singlePaste',
            component: SinglePaste,
            meta: { title: "Paste", showNav: false }
        },
        {
            path: '/commands',
            name: 'commands',
            component: Commands,
            meta: { showNav: true }
        },
        {
            path: '/promotions',
            name: 'promotions',
            component: Promotions,
            meta: { showNav: true, requiresAuth: true }
        },
        {
            path: '/promotions/create',
            name: 'createPromotion',
            component: CreatePromotion,
            meta: { title: "Start a Campaign", showNav: false, requiresAuth: true }
        },
        {
            path: '/error',
            name: 'error',
            meta: { showNav: false },
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
    ]
});

router.beforeEach(async (to, from, next) =>
{
    if (!store.hasTriedAuth())
    {
        await store.retrieveUserInfo()
    }

    if (to.meta.requiresAuth && !store.isLoggedIn())
    {
        store.pushErrorMessage(`You need to log in before accessing <code>${to.fullPath}</code>`);
        next('/');
    }
    else if (to.matched.length == 0)
    {
        store.pushErrorMessage(`Page not found: <code>${to.fullPath}</code>`);
        next('/');
    }
    else
    {
        next();
    }
});

router.onError((err: Error) =>
{
    store.pushErrorMessage(err.message);
});

export default router;
