import Vue from 'vue'
import Vuex from 'vuex'

import App from './App.vue'
import RootState from '@/models/RootState';

import router from './router'
import {vuexStore} from '@/app/Store';

Vue.config.productionTip = false;

var vue = new Vue({
  router,
  store: vuexStore,
  render: h => h(App)
}).$mount('#app');