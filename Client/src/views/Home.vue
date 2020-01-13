<template>
  <div>
    <Player path="/stream/-812461457" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Player from "@/components/Player.vue";
import { Component } from "vue-property-decorator";

@Component({ components: { Player } })
export default class Home extends Vue {}
</script>
