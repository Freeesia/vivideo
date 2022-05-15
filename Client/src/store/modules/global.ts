import { VuexModule, Mutation, Module } from "vuex-module-decorators";

@Module({ namespaced: true, name: "global" })
export default class Global extends VuexModule {
  dark = true;
  auto = true;
  volume = 1;

  @Mutation
  setDark(value: boolean) {
    this.dark = value;
  }

  @Mutation
  setAuto(value: boolean) {
    this.auto = value;
  }

  @Mutation
  setVolume(value: number) {
    this.volume = value;
  }
}
