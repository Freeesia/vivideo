import Vue from "vue";
import App from "./App.vue";
import "./registerServiceWorker";
import "./firebase";
import router from "./router";
import vuetify from "./plugins/vuetify";
import { store } from "./store";
import { init as SentryInit } from "@sentry/browser";
import { Vue as SentryVue } from "@sentry/integrations";

Vue.config.productionTip = false;

if (process.env.NODE_ENV === "production") {
  SentryInit({
    dsn: "https://6bd5217ab2e24414973357727d9df261@sentry.io/2409801",
    release: process.env.VUE_APP_VERSION,
    environment: "Production",
    integrations: [new SentryVue({ Vue, attachProps: true })]
  });
}

new Vue({
  router,
  vuetify,
  store,
  render: h => h(App)
}).$mount("#app");
