import { defineConfig } from "vite";
import { createVuePlugin } from "vite-plugin-vue2";
import Components from "unplugin-vue-components/vite";
import { VuetifyResolver } from "unplugin-vue-components/resolvers";

// https://vitejs.dev/config/
export default defineConfig({
  resolve: {
    alias: {
      "@": __dirname + "/src",
    },
  },
  plugins: [
    createVuePlugin(),
    Components({
      resolvers: [VuetifyResolver()],
      dts: "src/components.d.ts",
    }),
  ],
  server: {
    proxy: {
      "/api": "http://localhost:5000",
    },
  },
  build: {
    chunkSizeWarningLimit: 1500,
  },
  css: {
    // sassのバージョン上げれば動くけど、Vuetifyで警告が出る…
    // preprocessorOptions: {
    //   sass: { charset: false },
    //   scss: { charset: false },
    // },
    postcss: {
      plugins: [
        {
          postcssPlugin: "internal:charset-removal",
          AtRule: {
            charset: atRule => {
              if (atRule.name === "charset") {
                atRule.remove();
              }
            },
          },
        },
      ],
    },
  },
});
