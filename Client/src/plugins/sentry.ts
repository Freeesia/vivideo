import Vue from "vue";
import * as Sentry from "@sentry/vue";
import { BrowserTracing } from "@sentry/tracing";
import router from "@/router";

if (import.meta.env.PROD) {
  Sentry.init({
    Vue,
    dsn: "https://6bd5217ab2e24414973357727d9df261@o351180.ingest.sentry.io/2409801",
    integrations: [
      new BrowserTracing({
        routingInstrumentation: Sentry.vueRouterInstrumentation(router),
      }),
    ],
    // Set tracesSampleRate to 1.0 to capture 100%
    // of transactions for performance monitoring.
    // We recommend adjusting this value in production
    tracesSampleRate: 1.0,
    release: import.meta.env.VITE_APP_VERSION,
    tunnel: "/tunnel",
  });
}
