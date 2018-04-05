import Vue from 'vue'
import Router from 'vue-router'
import Home from './views/Home.vue'
import Stats from './views/Stats.vue'
import Pastes from './views/Pastes.vue'
import Commands from './views/Commands.vue'

Vue.use(Router)

export default new Router({
  mode: "history",
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/stats',
      name: 'stats',
      component: Stats
    },
    {
      path: '/pastes/:routePasteId?',
      name: 'pastes',
      component: Pastes
    },
    {
      path: '/commands',
      name: 'commands',
      component: Commands
    }
  ]
})
