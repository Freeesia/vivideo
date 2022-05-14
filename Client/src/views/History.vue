<template>
  <v-container>
    <v-toolbar ref="title" class="mb-2">
      <v-toolbar-title>視聴履歴</v-toolbar-title>
    </v-toolbar>
    <v-lazy v-for="video in videos" :key="video.path" class="pa-1">
      <v-card :to="'/play/' + video.path">
        <v-row no-gutters wrap>
          <v-col cols="12" sm="4">
            <v-img :src="getThumbnailPath(video.path)" />
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
import { HistoryModule } from "@/store";
import { getThumbnailPath, getTitle } from "@/utilities/pathUtility";
import Vue from "vue";
import Component from "vue-class-component";
import { orderBy } from "lodash";

@Component
export default class History extends Vue {
  public readonly videos = orderBy(HistoryModule.videos, ["lastUpdate"], ["desc"]);
  public readonly getTitle = getTitle;
  public readonly getThumbnailPath = getThumbnailPath;
}
</script>
