import { VuexModule, Mutation, Module } from "vuex-module-decorators";

@Module({ namespaced: true, name: "global" })
export default class Global extends VuexModule {
  dark = true;
  auto = true;

  @Mutation
  setDark(value: boolean) {
    this.dark = value;
  }

  @Mutation
  setAuto(value: boolean) {
    this.auto = value;
  }
}
