import Vue from "vue";
import Vuex from "vuex";
import Auth from "./modules/auth";
import Search from "./modules/search";
import { getModule } from "vuex-module-decorators";

Vue.use(Vuex);

export const store = new Vuex.Store({
  modules: {
    auth: Auth,
    search: Search
  }
});

export const AuthModule = getModule(Auth, store);
export const SearchModule = getModule(Search, store);
