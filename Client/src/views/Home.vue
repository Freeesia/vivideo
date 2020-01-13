<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png" />
    <Player path="/stream/-812461457" />
    <HelloWorld msg="Welcome to Your Vue.js App" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Player from "@/components/Player.vue";
import HelloWorld from "@/components/HelloWorld.vue";
import { Component } from "vue-property-decorator";

@Component({ components: { HelloWorld, Player } })
export default class Home extends Vue {}
</script>
