<template>
  <div ref="videoContainer" data-shaka-player-container>
    <video id="video" ref="video" data-shaka-player :poster="thumbnailPath" autoplay></video>
    <v-overlay absolute :value="isEnded">
      <slot name="overlay">
        <v-tooltip top>
          <template v-slot:activator="{ on }">
            <v-btn width="128" height="128" icon @click="replay" v-on="on">
              <v-icon x-large>replay</v-icon>
            </v-btn>
          </template>
          <span>リプレイ</span>
        </v-tooltip>
      </slot>
    </v-overlay>
  </div>
</template>
<style lang="scss" scoped>
video {
  width: 100%;
  height: 100%;
}
</style>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { Prop, Watch, Emit } from "vue-property-decorator";
import { polyfill, Player, ui } from "shaka-player/dist/shaka-player.ui";
import { assertIsDefined } from "../utilities/assert";
import "shaka-player/dist/controls.css";

@Component
export default class ShakaPlayer extends Vue {
  private player?: Player;
  private isEnded = false;

  @Prop({ type: String, required: true, default: "" })
  private streamPath!: string;

  @Prop({ type: String, required: true, default: "" })
  private thumbnailPath!: string;

  @Watch("streamPath", { immediate: true })
  private onPathChanged(newPath: string) {
    if (!newPath || !this.player) {
      return;
    }
    this.load();
  }

  created() {
    polyfill.installAll();
  }

  mounted() {
    const video = this.$refs.video as HTMLMediaElement;
    this.player = new Player(video);
    this.player.configure({
      streaming: {
        bufferBehind: Number.POSITIVE_INFINITY
      }
    });
    const overlay = new ui.Overlay(this.player, this.$refs.videoContainer as HTMLElement, video);
    overlay.configure({
      controlPanelElements: [
        "rewind",
        "fast_forward",
        "mute",
        "volume",
        "time_and_duration",
        "spacer",
        "fullscreen",
        "overflow_menu"
      ],
      overflowMenuButtons: ["picture_in_picture"]
    });
    video.addEventListener("playing", () => this.play());
    video.addEventListener("ended", () => this.ended());
  }

  private async load() {
    assertIsDefined(this.streamPath);
    assertIsDefined(this.player);
    const supportType = await Player.probeSupport();
    if (supportType.manifest["application/dash+xml"]) {
      await this.player.load(this.streamPath + "/master.mpd", 0, "application/dash+xml");
    } else {
      await this.player.load(this.streamPath + "/master.m3u8", 0, "application/x-mpegURL");
    }
  }

  @Emit()
  private ended() {
    this.isEnded = true;
  }

  @Emit()
  private play() {
    this.isEnded = false;
  }

  private replay() {
    assertIsDefined(this.player);
    const video = this.player.getMediaElement();
    video.play();
  }

  private beforeDestroy() {
    if (this.player) {
      this.player.destroy();
    }
  }
}
</script>
