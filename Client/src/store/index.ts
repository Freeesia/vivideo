import Vue from "vue";
import Vuex from "vuex";
import { getModule } from "vuex-module-decorators";
import VuexPersistence from "vuex-persist";
import Auth from "./modules/auth";
import Search from "./modules/search";
import General from "./modules/general";
import Global from "./modules/global";
import History from "./modules/history";

Vue.use(Vuex);

const persist = new VuexPersistence({
  modules: ["search", "global", "history"],
});

export const store = new Vuex.Store({
  modules: {
    auth: Auth,
    search: Search,
    general: General,
    global: Global,
    history: History,
  },
  plugins: [persist.plugin],
});

export const AuthModule = getModule(Auth, store);
export const SearchModule = getModule(Search, store);
export const GeneralModule = getModule(General, store);
export const GlobalModule = getModule(Global, store);
export const HistoryModule = getModule(History, store);
