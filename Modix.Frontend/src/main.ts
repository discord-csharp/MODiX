import { vuexStore } from '@/app/Store';
import Vue from 'vue';
import App from './App.vue';
import router from './router';

Vue.config.productionTip = false;

var vue = new Vue({
  router,
  store: vuexStore,
  render: h => h(App)
}).$mount('#app');