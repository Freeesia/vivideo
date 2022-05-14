import { VuexModule, Mutation, Module } from "vuex-module-decorators";
import { HistoryVideo } from "@/model";

@Module({ namespaced: true, name: "history" })
export default class History extends VuexModule {
  public videos: HistoryVideo[] = [];

  @Mutation
  public watch(payload: { path: string; current: number }) {
    const { path, current } = payload;
    const i = this.videos.findIndex(v => v.path === path);
    if (i === -1) {
      this.videos.push({ path, current, lastUpdate: Date.now() });
    } else {
      this.videos.splice(i, 1, { path, current, lastUpdate: Date.now() });
    }
  }

  @Mutation
  public end(path: string) {
    const i = this.videos.findIndex(v => v.path === path);
    if (i === -1) {
      this.videos.push({ path, current: -1, lastUpdate: Date.now() });
    } else {
      this.videos.splice(i, 1, { path, current: -1, lastUpdate: Date.now() });
    }
  }
}
