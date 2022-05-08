import Vue from "vue";
import Vuetify from "vuetify";
import "vuetify/dist/vuetify.min.css";
import "material-design-icons-iconfont/dist/material-design-icons.css";
import ja from "vuetify/src/locale/ja";
import { GlobalModule } from "@/store";

Vue.use(Vuetify);

export default new Vuetify({
  lang: {
    locales: { ja },
    current: "ja",
  },
  icons: {
    iconfont: "md",
  },
  theme: {
    dark: GlobalModule.dark,
  },
});
