<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png" />
    <Player :options="videoOptions" />
    <HelloWorld msg="Welcome to Your Vue.js App" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Player from "@/components/Player.vue";
import HelloWorld from "@/components/HelloWorld.vue";
import { Component } from "vue-property-decorator";

@Component({ components: { HelloWorld, Player } })
export default class Home extends Vue {
  private videoOptions = {
    autoplay: true,
    controls: true,
    sources: [
      {
        src: "/stream/-812461457/master.mpd",
        type: "application/dash+xml"
      },
      {
        src: "/stream/-812461457/master.m3u8",
        type: "application/x-mpegURL"
      }
    ]
  };
}
</script>
