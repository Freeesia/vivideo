<template>
  <v-card flat>
    <video ref="videoPlayer" autoplay class="video-js"></video>
    <v-overlay absolute :value="isEnded">
      <slot name="overlay">
        <v-tooltip top>
          <template #activator="{ on }">
            <v-btn icon x-large @click="replay" v-on="on">
              <v-icon x-large>replay</v-icon>
            </v-btn>
          </template>
          <span>リプレイ</span>
        </v-tooltip>
      </slot>
    </v-overlay>
  </v-card>
</template>

<script lang="ts">
import Vue from "vue";
import videojs, { VideoJsPlayer } from "video.js";
import Component from "vue-class-component";
import { Prop, Watch, Emit } from "vue-property-decorator";
import "video.js/dist/video-js.css";

@Component({})
export default class Player extends Vue {
  private player!: VideoJsPlayer;
  public isEnded = false;

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
        type: "application/dash+xml",
      },
      {
        src: this.streamPath + "/master.m3u8",
        type: "application/x-mpegURL",
      },
    ]);
  }

  private mounted() {
    this.player = videojs(this.$refs.videoPlayer as Element, {
      liveui: true,
      controls: true,
      fluid: true,
      autoplay: true,
      poster: this.thumbnailPath,
    });
    this.player.on("ended", () => this.ended());
    this.player.on("play", () => this.play());
  }

  public replay() {
    this.player.currentTime(0);
    this.player.play();
  }

  @Emit()
  private ended() {
    this.isEnded = true;
  }

  @Emit()
  private play() {
    this.isEnded = false;
  }

  private beforeDestroy() {
    if (this.player) {
      this.player.dispose();
    }
  }
}
</script>
