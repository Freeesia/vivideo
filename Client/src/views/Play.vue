<template>
  <div>
    <div>{{ this.$route.params.request }}</div>
    <Player v-if="streamPath !== ''" :path="streamPath" />
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import Player from "@/components/Player.vue";
import Axios from "axios";

@Component({ components: { Player } })
export default class Play extends Vue {
  private streamPath = "";
  private readonly delay: (msec: number) => Promise<void> = msec => new Promise(resolve => setTimeout(resolve, msec));

  private async created() {
    const res = await Axios.post<string>("/api/video/transcode/", {
      path: this.$route.params.request
    });
    this.streamPath = res.data;
  }
}
</script>
