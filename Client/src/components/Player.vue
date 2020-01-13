<template>
  <video ref="videoPlayer" class="video-js"></video>
</template>

<script lang="ts">
import Vue from "vue";
import videojs from "video.js";
import Component from "vue-class-component";
import { Prop } from "vue-property-decorator";
import "video.js/dist/video-js.css";

@Component({})
export default class Player extends Vue {
  private player: any;

  @Prop({ type: Object })
  private options: {};

  private mounted() {
    this.player = videojs(this.$refs.videoPlayer, this.options);
  }
  private beforeDestroy() {
    if (this.player) {
      this.player.dispose();
    }
  }
}
</script>
