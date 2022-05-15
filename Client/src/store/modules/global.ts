import { createModule } from "vuex-class-component";

const VuexModule = createModule({
  namespaced: "global",
  strict: false,
});

export default class Global extends VuexModule {
  dark = true;
  auto = true;
  volume = 1;
}
