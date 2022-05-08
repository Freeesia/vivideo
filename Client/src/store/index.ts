import Vue from "vue";
import Vuex from "vuex";
import { getModule } from "vuex-module-decorators";
import VuexPersistence from "vuex-persist";
import Auth from "./modules/auth";
import Search from "./modules/search";
import General from "./modules/general";
import Global from "./modules/global";

Vue.use(Vuex);

const persist = new VuexPersistence({
  modules: ["search", "global"],
});

export const store = new Vuex.Store({
  modules: {
    auth: Auth,
    search: Search,
    general: General,
    global: Global,
  },
  plugins: [persist.plugin],
});

export const AuthModule = getModule(Auth, store);
export const SearchModule = getModule(Search, store);
export const GeneralModule = getModule(General, store);
export const GlobalModule = getModule(Global, store);
