<template>
  <v-container>
    <v-toolbar class="mb-2">
      <v-toolbar-title>視聴履歴</v-toolbar-title>
    </v-toolbar>
    <v-lazy v-for="video in videos" :key="video.path" class="pa-1">
      <v-card @click="play(video.path)">
        <v-row no-gutters wrap>
          <v-col cols="12" sm="4">
            <video-thumbnail :content="contents[video.path]" :history="video" />
          </v-col>
          <v-col cols="12" sm="8">
            <v-card-title> {{ getTitle(video.path) }} </v-card-title>
          </v-col>
        </v-row>
      </v-card>
    </v-lazy>
  </v-container>
</template>
<script lang="ts">
import { AuthModule, HistoryModule } from "@/store";
import { getThumbnailPath, getTitle } from "@/utilities/pathUtility";
import Vue from "vue";
import Component from "vue-class-component";
import { orderBy } from "lodash";
import type { AxiosInstance } from "axios";
import { ContentNode } from "@/model";
import { toRecord } from "@/utilities/systemUtility";

@Component
export default class History extends Vue {
  public readonly videos = orderBy(HistoryModule.videos, ["lastUpdate"], ["desc"]);
  public readonly getTitle = getTitle;
  public readonly getThumbnailPath = getThumbnailPath;
  public contents: Record<string, ContentNode> = {};
  private axios?: AxiosInstance;

  private async created() {
    this.axios = await AuthModule.getAxios();
    const promises = this.videos
      // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
      .map(v => this.axios!.get<ContentNode>("/api/video/content/" + v.path, { validateStatus: () => true }));
    const results = await Promise.all(promises);
    this.contents = toRecord(
      results.map(r => r.data).filter(d => d),
      "contentPath"
    );
  }

  public async play(path: string) {
    await this.$router.push({
      name: "play",
      params: { path },
    });
  }
}
</script>
