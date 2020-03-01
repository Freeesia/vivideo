<template>
  <video ref="videoPlayer" autoplay class="video-js"></video>
</template>

<script lang="ts">
import Vue from "vue";
import videojs, { VideoJsPlayer } from "video.js";
import Component from "vue-class-component";
import { Prop, Watch } from "vue-property-decorator";
import "video.js/dist/video-js.css";

@Component({})
export default class Player extends Vue {
  private player!: VideoJsPlayer;

  @Prop({ type: String, required: true, default: "" })
  private streamPath!: string;

  @Prop({ type: String, required: true, default: "" })
  private thumbnailPath!: string;

  @Watch("streamPath", { immediate: true })
  private onPathChanged(newPath: string) {
    if (!newPath) {
      return;
    }
    this.player.src([
      {
        src: this.streamPath + "/master.mpd",
        type: "application/dash+xml"
      },
      {
        src: this.streamPath + "/master.m3u8",
        type: "application/x-mpegURL"
      }
    ]);
  }

  private mounted() {
    this.player = videojs(this.$refs.videoPlayer, {
      liveui: true,
      controls: true,
      fluid: true,
      autoplay: true,
      poster: this.thumbnailPath
    });
  }
  private beforeDestroy() {
    if (this.player) {
      this.player.dispose();
    }
  }
}
</script>
