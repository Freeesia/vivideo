<template>
  <v-container fluid>
    <v-row no-gutters dense>
      <v-col cols="12" md="8">
        <ShakaPlayer :stream-path="streamPath" :thumbnail-path="thumbnailPath" @ended="ended">
          <template v-if="autoNext" #overlay>
            <v-row justify="center">
              <v-progress-circular class="ma-2" rotate="-90" size="60" :value="percent"></v-progress-circular>
            </v-row>
            <v-btn @click="cancelAuto">キャンセル</v-btn>
          </template>
        </ShakaPlayer>
        <div>{{ path }}</div>
      </v-col>
      <v-col md="4">
        <v-list>
          <v-subheader>
            プレイリスト
            <v-spacer></v-spacer>
            <v-switch v-model="autoNext" label="自動再生"></v-switch>
          </v-subheader>
          <v-list-item v-for="item in contents" :key="item.name" @click="select(item)">
            <v-list-item-avatar tile>
              <v-overlay absolute :value="isPlaying(item)">
                <v-icon>play_arrow</v-icon>
              </v-overlay>
              <v-img :src="getThumbnailPath(item)">
                <template #placeholder>
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
import ShakaPlayer from "@/components/ShakaPlayer.vue";
import { AuthModule, GeneralModule, SearchModule } from "../store";
import { Prop } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import { AxiosInstance } from "axios";
import path from "path";
import assert from "assert";
import { assertIsDefined } from "../utilities/assert";
import { getThumbnailPath } from "../utilities/pathUtility";
import { delay } from "../utilities/systemUtility";
import { compareFunc } from "../utilities/sortUtility";

@Component({ components: { Player, ShakaPlayer } })
export default class Play extends Vue {
  private streamPath = "";
  private thumbnailPath = "";
  private autoNext = true;
  private percent = 0;

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
    const comp = compareFunc(SearchModule.sorts.find(v => v.path === this.path));
    this.contents = res.data.filter(c => !c.isDirectory).sort(comp);
  }

  private isPlaying(item: ContentNode) {
    return this.path === item.contentPath;
  }

  private async ended() {
    const index = this.contents.findIndex(c => c.contentPath === this.path);
    if (index < 0 || this.contents.length - 1 <= index) {
      this.autoNext = false;
    }
    if (!this.autoNext) {
      return;
    }
    const next = this.contents[index + 1];
    assertIsDefined(this.axios);
    // ローカルだと待っている間に初期セグメント超えて次のセグメントから再生してしまう
    this.axios.post<string>(`/api/video/transcode/?path=${encodeURIComponent(next.contentPath)}`);
    const now = Date.now();
    this.percent = 0;
    while (this.percent < 100) {
      this.percent = ((Date.now() - now) / 3000) * 100;
      await delay(300);
      if (!this.autoNext) {
        return;
      }
    }
    this.select(next);
  }

  private cancelAuto() {
    this.autoNext = false;
  }

  private async select(item: ContentNode) {
    await this.$router.push({
      name: "play",
      query: {
        path: item.contentPath,
      },
    });
    this.loadVideo();
  }
}
</script>
