import { vuexStore } from '@/app/Store';
import Vue from 'vue';
import App from './App.vue';
import router from '@/router';
import VueClipboards from 'vue-clipboards';
import VTooltip from 'v-tooltip';
import { ensureConfig } from './models/PersistentConfig';

Vue.config.productionTip = false;
Vue.use(VueClipboards);
Vue.use(VTooltip);

ensureConfig();

new Vue
({
    router,
    store: vuexStore,
    render: h => h(App)
}).$mount('#app');