<template>
  <div>
    <v-progress-linear :active="loading" indeterminate />
    <v-container class="fill-height" fluid>
      <v-row dense>
        <v-col v-for="item in filtered" :key="item.name" cols="12" sm="6" md="4" lg="3" xl="2">
          <v-card height="240" @click="selectContent(item)">
            <v-img contain height="160" :src="'/api/thumbnail/' + item.contentPath">
              <template v-slot:placeholder>
                <v-row class="fill-height" align="center" justify="center">
                  <v-icon size="100">{{ item.isDirectory ? "video_library" : "movie" }}</v-icon>
                </v-row>
              </template>
            </v-img>
            <v-card-title>
              {{ item.name }}
            </v-card-title>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<style lang="scss" scoped>
.v-card {
  overflow: hidden;
  .v-card__title {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
  }
}
</style>

<script lang="ts">
import Vue from "vue";
import Player from "@/components/Player.vue";
import { Component, Watch } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import { AxiosInstance } from "axios";
import { AuthModule, SearchModule } from "../store";

@Component({ components: { Player } })
export default class Home extends Vue {
  private selectedContent: ContentNode | null = null;
  private contents: ContentNode[] = [];
  private filtered: ContentNode[] = [];
  private loading = true;
  private axios?: AxiosInstance;

  @Watch("$route", { immediate: true, deep: true })
  private async onRequestChanged() {
    this.loading = true;
    this.contents = [];
    if (!this.axios) {
      this.axios = await AuthModule.getAxios();
    }
    const res = await this.axios.get<ContentNode[]>("/api/video/" + (this.$route.params.request ?? ""));
    this.filtered = this.contents = res.data;
    this.loading = false;
  }

  private get search() {
    return SearchModule.filter;
  }

  @Watch("search")
  private onSearchChanged() {
    if (this.search) {
      const search = this.search.toUpperCase();
      this.filtered = this.contents.filter(n => n.name.toUpperCase().includes(search));
    } else {
      this.filtered = this.contents;
    }
  }

  private selectContent(content: ContentNode) {
    this.selectedContent = content;
    if (this.selectedContent ? !this.selectedContent.isDirectory : false) {
      this.$router.push({
        name: "play",
        params: {
          request: content.contentPath
        }
      });
    } else {
      this.$router.push({
        name: "home",
        params: {
          request: content.contentPath
        }
      });
    }
  }
}
</script>
