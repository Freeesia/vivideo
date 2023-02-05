import Vue from "vue";
import Vuex from "vuex";
import VuexPersistence from "vuex-persist";
import Auth from "./modules/auth";
import Search from "./modules/search";
import General from "./modules/general";
import Global from "./modules/global";
import { createProxy, extractVuexModule } from "vuex-class-component";

Vue.use(Vuex);

const persist = new VuexPersistence({
  modules: ["search", "global", "history"],
});

export const store = new Vuex.Store({
  modules: {
    ...extractVuexModule(Auth),
    ...extractVuexModule(Search),
    ...extractVuexModule(General),
    ...extractVuexModule(Global),
  },
  plugins: [persist.plugin],
});

export const AuthModule = createProxy(store, Auth);
export const SearchModule = createProxy(store, Search);
export const GeneralModule = createProxy(store, General);
export const GlobalModule = createProxy(store, Global);
