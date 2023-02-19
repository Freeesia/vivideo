import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue2";
import { VitePWA } from "vite-plugin-pwa";
import Components from "unplugin-vue-components/vite";
import { VuetifyResolver } from "unplugin-vue-components/resolvers";
import NodeModulesPolyfills from "@esbuild-plugins/node-modules-polyfill";
import rollupNodePolyFill from "rollup-plugin-node-polyfills";
import { visualizer } from "rollup-plugin-visualizer";
import pluginRewriteAll from "vite-plugin-rewrite-all";
import { sentryVitePlugin } from "@sentry/vite-plugin";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  resolve: {
    alias: {
      "@": __dirname + "/src",
      assert: "rollup-plugin-node-polyfills/polyfills/assert",
      path: "rollup-plugin-node-polyfills/polyfills/path",
      util: "rollup-plugin-node-polyfills/polyfills/util",
    },
  },
  plugins: [
    vue(),
    Components({
      resolvers: [VuetifyResolver()],
      dts: "src/components.d.ts",
    }),
    VitePWA({
      registerType: "autoUpdate",
      includeAssets: ["favicon.ico", "robots.txt"],
      // devOptions: {
      //   enabled: true,
      // },
      manifest: {
        name: "Frix TV Prime",
        theme_color: "#FF9800",
        lang: "ja",
      },
      workbox: {
        maximumFileSizeToCacheInBytes: 3 * 1024 * 1024,
        navigateFallbackDenylist: [/\/hangfire/],
        runtimeCaching: [
          {
            urlPattern: /\/api\/thumbnail\//,
            handler: "CacheFirst",
            method: "GET",
            options: {
              cacheName: "thumbnail-cache",
              expiration: {
                maxEntries: 1000,
                maxAgeSeconds: 60 * 60 * 24 * 365,
              },
              cacheableResponse: {
                statuses: [0, 200],
              },
            },
          },
          {
            urlPattern: /\/api\/video\//,
            handler: "StaleWhileRevalidate",
            method: "GET",
            options: {
              cacheName: "list-cache",
              expiration: {
                maxEntries: 1000,
                maxAgeSeconds: 60 * 60 * 6,
              },
              cacheableResponse: {
                statuses: [0, 200],
              },
              broadcastUpdate: {
                channelName: "video-list",
                options: {},
              },
            },
          },
        ],
      },
    }),
    pluginRewriteAll(),
    process.env.SENTRY_AUTH_TOKEN &&
      sentryVitePlugin({
        org: process.env.SENTRY_ORG,
        project: process.env.SENTRY_PROJECT,
        authToken: process.env.SENTRY_AUTH_TOKEN,
        release: process.env.InformationalVersion,
        include: "./dist",
      }),
    mode === "analyze" &&
      visualizer({
        open: true,
        filename: "dist/stats.html",
        gzipSize: true,
        brotliSize: true,
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
    chunkSizeWarningLimit: 3000,
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
