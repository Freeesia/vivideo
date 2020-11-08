/* eslint-disable @typescript-eslint/no-var-requires */
const HardSourceWebpackPlguin = require("hard-source-webpack-plugin");
const SentryWebpackPlugin = require("@sentry/webpack-plugin");
const os = require("os");

const targetUrl = process.env.TARGET_URL || "http://localhost:5000";

module.exports = {
  transpileDependencies: ["vuetify"],
  devServer: {
    proxy: {
      "/hangfire": {
        target: targetUrl,
      },
      "/api": {
        target: targetUrl,
      },
      "/stream": {
        target: targetUrl,
      },
    },
  },
  configureWebpack: {
    devtool: "source-map",
    plugins: [
      new HardSourceWebpackPlguin(),
      new SentryWebpackPlugin({
        include: ["dist", "src"],
        dryRun: process.env.NODE_ENV !== "production" || !process.env.SENTRY_AUTH_TOKEN,
        release: process.env.VUE_APP_VERSION,
        validate: true,
        ext: ["js", "map", "ts"],
        finalize: false,
      }),
    ],
  },
  chainWebpack: config => {
    config.plugin("fork-ts-checker").tap(args => {
      args[0].workers = Math.max(os.cpus().length - 1, 1);
      args[0].memoryLimit = os.freemem() > 8096 ? 8096 : 2048;
      return args;
    });
  },
  pwa: {
    name: "Frix TV Prime",
    themeColor: "#FF9800",
    msTileColor: "#FF9800",
    workboxOptions: {
      clientsClaim: true,
      skipWaiting: true,
      runtimeCaching: [
        {
          urlPattern: /\/api\/thumbnail\//,
          handler: "StaleWhileRevalidate",
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
          },
        },
      ],
    },
    // iconPaths: {
    //   appleTouchIcon: "img/icons/icon-152x152_light.png",
    //   maskIcon: "img/Logo_light.svg",
    //   msTileImage: "img/icons/icon-144x144_dark.png"
    // }
  },
};
