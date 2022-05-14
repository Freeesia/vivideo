<template>
  <div ref="videoContainer" data-shaka-player-container>
    <video id="video" ref="video" data-shaka-player :poster="thumbnailPath" autoplay></video>
    <v-overlay absolute :value="isEnded">
      <slot name="overlay">
        <v-tooltip top>
          <template #activator="{ on }">
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
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { Prop, Watch, Emit, PropSync } from "vue-property-decorator";
import { polyfill, Player, ui } from "shaka-player/dist/shaka-player.ui";
import { assertIsDefined } from "../utilities/assert";
import "shaka-player/dist/controls.css";

@Component
export default class ShakaPlayer extends Vue {
  private updateCurrentTimer!: number;
  private player?: Player;
  public isEnded = false;

  @Prop({ type: String, required: true, default: "" })
  public readonly streamPath!: string;

  @Prop({ type: String, required: true, default: "" })
  public readonly thumbnailPath!: string;

  @PropSync("current", { type: Number, default: 0 })
  public _current!: number;

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
    this.updateCurrentTimer = window.setInterval(this.updateCurrent, 1_000);
    const video = this.$refs.video as HTMLMediaElement;
    this.player = new Player(video);
    this.player.configure({
      streaming: {
        bufferBehind: Number.POSITIVE_INFINITY,
      },
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
        "overflow_menu",
      ],
      overflowMenuButtons: ["picture_in_picture"],
      doubleClickForFullscreen: true,
    });
    video.addEventListener("playing", () => this.play());
    video.addEventListener("ended", () => this.ended());
  }

  private async load() {
    assertIsDefined(this.streamPath);
    assertIsDefined(this.player);
    const supportType = await Player.probeSupport();
    if (supportType.manifest["application/dash+xml"]) {
      await this.player.load(this.streamPath + "/master.mpd", this._current, "application/dash+xml");
    } else {
      await this.player.load(this.streamPath + "/master.m3u8", this._current, "application/x-mpegURL");
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

  private updateCurrent() {
    assertIsDefined(this.player);
    const video = this.player?.getMediaElement();
    this._current = video.currentTime;
  }

  public replay() {
    assertIsDefined(this.player);
    const video = this.player.getMediaElement();
    video.play();
  }

  private beforeDestroy() {
    if (this.player) {
      this.player.destroy();
    }
    clearInterval(this.updateCurrentTimer);
  }
}
</script>
<style lang="scss" scoped>
video {
  width: 100%;
  height: 100%;
}
</style>
