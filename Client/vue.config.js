/* eslint-disable @typescript-eslint/no-var-requires */
const HardSourceWebpackPlguin = require("hard-source-webpack-plugin");

module.exports = {
  transpileDependencies: ["vuetify"],
  configureWebpack: {
    devtool: "source-map",
    plugins: [new HardSourceWebpackPlguin()]
  },
  pwa: {
    name: "Vivideo",
    themeColor: "#FF9800",
    msTileColor: "#FF9800",
    workboxOptions: {
      clientsClaim: true,
      skipWaiting: true
    },
    // iconPaths: {
    //   appleTouchIcon: "img/icons/icon-152x152_light.png",
    //   maskIcon: "img/Logo_light.svg",
    //   msTileImage: "img/icons/icon-144x144_dark.png"
    // }
  }
}