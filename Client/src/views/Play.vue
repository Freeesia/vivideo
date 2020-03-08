<template>
  <v-container fluid>
    <v-row no-gutters dense>
      <v-col cols="12" md="8">
        <Player :stream-path="streamPath" :thumbnail-path="thumbnailPath" />
        <div>{{ path }}</div>
      </v-col>
      <v-col md="4">
        <v-list>
          <v-subheader>プレイリスト</v-subheader>
          <v-list-item v-for="item in contents" :key="item.name" @click="select(item)">
            <v-list-item-avatar tile>
              <v-overlay absolute :value="isPlaying(item)">
                <v-icon>play_arrow</v-icon>
              </v-overlay>
              <v-img :src="getThumbnailPath(item)">
                <template v-slot:placeholder>
                  <v-row class="fill-height" align="center" justify="center">
                    <v-icon>movie</v-icon>
                  </v-row>
                </template>
              </v-img>
            </v-list-item-avatar>
            <v-list-item-content>
              <v-list-item-title v-text="item.name"></v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list>
      </v-col>
    </v-row>
  </v-container>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import Player from "@/components/Player.vue";
import { AuthModule, GeneralModule, SearchModule } from "../store";
import { Prop } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import { AxiosInstance } from "axios";
import path from "path";
import assert from "assert";
import { assertIsDefined } from "../utilities/assert";
import { getThumbnailPath } from "../utilities/pathUtility";

@Component({ components: { Player } })
export default class Play extends Vue {
  private streamPath = "";
  private thumbnailPath = "";

  @Prop({ required: true, type: String, default: "" })
  path!: string;

  private contents: ContentNode[] = [];
  private axios?: AxiosInstance;

  private getThumbnailPath = getThumbnailPath;

  private async created() {
    assert(this.path);
    this.axios = await AuthModule.getAxios();
    this.loadVideo();
    this.loadList();
  }

  private async loadVideo() {
    assertIsDefined(this.axios);
    GeneralModule.setLoading(true);
    this.thumbnailPath = "/api/thumbnail/?path=" + encodeURIComponent(this.path);
    const res = await this.axios.post<string>(`/api/video/transcode/?path=${encodeURIComponent(this.path)}`);
    this.streamPath = res.data;
    GeneralModule.setLoading(false);
  }

  private async loadList() {
    assertIsDefined(this.axios);
    const dir = path.dirname(this.path);
    const res = await this.axios.get<ContentNode[]>("/api/video/" + dir);
    this.contents = res.data.filter(c => !c.isDirectory).sort(SearchModule.compareFunc);
  }

  private isPlaying(item: ContentNode) {
    return this.path === item.contentPath;
  }

  private select(item: ContentNode) {
    this.$router.push({
      name: "play",
      query: {
        path: item.contentPath
      }
    });
  }
}
</script>
