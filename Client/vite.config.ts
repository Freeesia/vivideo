import { defineConfig } from "vite";
import { createVuePlugin } from "vite-plugin-vue2";
import Components from "unplugin-vue-components/vite";
import { VuetifyResolver } from "unplugin-vue-components/resolvers";
import NodeModulesPolyfills from "@esbuild-plugins/node-modules-polyfill";
import rollupNodePolyFill from "rollup-plugin-node-polyfills";

// esbuildではなぜかpolyfil出来ないので直接書く
const serveAlias = {
  assert: "rollup-plugin-node-polyfills/polyfills/assert",
  path: "rollup-plugin-node-polyfills/polyfills/path",
};

// https://vitejs.dev/config/
export default defineConfig(env => ({
  resolve: {
    alias: {
      "@": __dirname + "/src",
      ...(env.command === "serve" ? serveAlias : {}),
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
      "/hangfire": "http://localhost:5000",
    },
  },
  optimizeDeps: {
    esbuildOptions: {
      plugins: [NodeModulesPolyfills()],
    },
  },
  build: {
    chunkSizeWarningLimit: 1500,
    rollupOptions: {
      plugins: [rollupNodePolyFill()],
    },
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
}));
