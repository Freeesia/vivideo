<template>
  <video autoplay ref="videoPlayer" class="video-js"></video>
</template>

<script lang="ts">
import Vue from "vue";
import videojs, { VideoJsPlayer } from "video.js";
import Component from "vue-class-component";
import { Prop } from "vue-property-decorator";
import "video.js/dist/video-js.css";

@Component({})
export default class Player extends Vue {
  private player: VideoJsPlayer;

  @Prop({ type: String, required: true })
  private path!: string;

  private mounted() {
    this.player = videojs(this.$refs.videoPlayer, {
      liveui: true,
      controls: true,
      fluid: true,
      sources: [
        {
          src: this.path + "/master.mpd",
          type: "application/dash+xml"
        },
        {
          src: this.path + "/master.m3u8",
          type: "application/x-mpegURL"
        }
      ]
    });
  }
  private beforeDestroy() {
    if (this.player) {
      this.player.dispose();
    }
  }
}
</script>
