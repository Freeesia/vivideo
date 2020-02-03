import Vue from "vue";
import Vuex from "vuex";
import Auth from "./modules/auth";
import { getModule } from "vuex-module-decorators";

Vue.use(Vuex);

export const store = new Vuex.Store({
  modules: {
    auth: Auth
  }
});

export const AuthModule = getModule(Auth, store);
