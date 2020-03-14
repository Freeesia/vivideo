<template>
  <div ref="videoContainer" data-shaka-player-container>
    <video id="video" ref="video" data-shaka-player :poster="thumbnailPath" autoplay></video>
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
import { Prop, Watch } from "vue-property-decorator";
import { polyfill, Player, ui } from "shaka-player/dist/shaka-player.ui";
import { assertIsDefined } from "../utilities/assert";
import "shaka-player/dist/controls.css";

@Component
export default class ShakaPlayer extends Vue {
  private player?: Player;

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
    this.player = new Player(this.$refs.video as HTMLMediaElement);
    const overlay = new ui.Overlay(
      this.player,
      this.$refs.videoContainer as HTMLElement,
      this.$refs.video as HTMLMediaElement
    );
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

  private beforeDestroy() {
    if (this.player) {
      this.player.destroy();
    }
  }
}
</script>
