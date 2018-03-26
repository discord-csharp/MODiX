import Vue from 'vue'
import Vuex from 'vuex'

import App from './App.vue'
import router from './router'
import RootState from '@/models/RootState';

import {modix} from '@/app/Store'

Vue.use(Vuex);

const store = new Vuex.Store<RootState>
({
    modules:
    {
        modix,
    },
});

Vue.config.productionTip = false;

var vue = new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app');