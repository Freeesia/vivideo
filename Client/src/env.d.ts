/* eslint-disable */
/// <reference types="vite/client" />

// Vuetify
declare module "vuetify/lib/framework" {
  import "vuetify/types";
  import Vuetify from "vuetify";
  export default Vuetify;
}

interface ImportMetaEnv {
  // see https://vitejs.dev/guide/env-and-mode.html#env-files
  // add .env variables.
  readonly VITE_APP_VERSION: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
