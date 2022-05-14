<template>
  <v-img contain height="160" :src="thumbnailPath">
    <template #placeholder>
      <v-row class="fill-height" align="center" justify="center">
        <v-icon size="100">{{ content?.isDirectory ? "video_library" : "movie" }}</v-icon>
      </v-row>
    </template>
    <div class="thumbnail-overlay">
      <p v-if="total" class="duration">{{ total }}</p>
      <v-progress-linear v-if="current" :value="current" color="red" background-color="grey" />
      <slot></slot>
    </div>
  </v-img>
</template>
<script lang="ts">
import { ContentNode, HistoryVideo } from "@/model";
import { getThumbnailPath } from "@/utilities/pathUtility";
import Vue from "vue";
import Component from "vue-class-component";
import { Prop } from "vue-property-decorator";
import format from "format-duration";

@Component
export default class VideoThumbnail extends Vue {
  @Prop({ type: Object })
  public readonly content?: ContentNode;
  @Prop({ type: Object })
  public readonly history?: HistoryVideo;
  public readonly getThumbnailPath = getThumbnailPath;

  get thumbnailPath() {
    return getThumbnailPath(this.content?.contentPath ?? this.history?.path ?? "");
  }

  get total() {
    if (this.content && !this.content.isDirectory) {
      return format(this.content.duration * 1000);
    } else {
      return "";
    }
  }

  get current() {
    if (this.content && this.history) {
      if (this.history.current > 0) {
        // 極短い再生時間だと見にくいので、最低5%表示する
        return Math.max(this.history.current / this.content.duration, 0.05) * 100;
      } else {
        return 100;
      }
    } else {
      return 0;
    }
  }
}
</script>
<style>
.thumbnail-overlay {
  position: absolute;
  width: 100%;
  height: 100%;
}
.thumbnail-overlay .duration {
  position: absolute;
  right: 0px;
  bottom: 0px;
  padding: 4px;
  margin: 8px;
  background-color: #000a;
  color: white;
}
.thumbnail-overlay .v-progress-linear {
  position: absolute;
  bottom: 0px;
}
</style>
