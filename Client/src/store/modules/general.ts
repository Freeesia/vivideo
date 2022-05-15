import { createModule } from "vuex-class-component";

const VuexModule = createModule({
  namespaced: "general",
  strict: false,
});

export default class General extends VuexModule {
  loading = false;
}
