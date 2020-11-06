/* eslint-disable @typescript-eslint/no-var-requires */
const HardSourceWebpackPlguin = require("hard-source-webpack-plugin");
const SentryWebpackPlugin = require("@sentry/webpack-plugin");
const os = require("os");

module.exports = {
  transpileDependencies: ["vuetify"],
  devServer: {
    proxy: {
      "/hangfire": {
        target: "http://localhost:5000",
      },
      "/api": {
        target: "http://localhost:5000",
      },
      "/stream": {
        target: "http://localhost:5000",
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
  chainWebpack: (config) => {
    config.plugin("fork-ts-checker").tap((args) => {
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
    },
    // iconPaths: {
    //   appleTouchIcon: "img/icons/icon-152x152_light.png",
    //   maskIcon: "img/Logo_light.svg",
    //   msTileImage: "img/icons/icon-144x144_dark.png"
    // }
  },
};
