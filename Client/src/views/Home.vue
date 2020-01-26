<template>
  <div>
    <v-progress-linear :active="loading" indeterminate />
    <v-container class="fill-height" fluid>
      <v-row>
        <v-col> {{ $route.params.request }} </v-col>
        <v-col sm="4">
          <v-text-field
            v-model="search"
            label="検索"
            prepend-inner-icon="search"
            hide-details
            clearable
            dense
            outlined
          />
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-list>
            <v-list-item-group color="primary">
              <v-list-item v-for="item in filtered" :key="item.name" @click="selectContent(item)">
                <v-list-item-content>
                  <v-list-item-title>{{ item.name }}</v-list-item-title>
                </v-list-item-content>
              </v-list-item>
            </v-list-item-group>
          </v-list>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Player from "@/components/Player.vue";
import { Component, Watch } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import Axios from "axios";

@Component({ components: { Player } })
export default class Home extends Vue {
  private selectedContent: ContentNode | null = null;
  private contents: ContentNode[] = [];
  private filtered: ContentNode[] = [];
  private loading = true;
  private search: string | null = null;

  @Watch("$route", { immediate: true, deep: true })
  private async onRequestChanged() {
    this.loading = true;
    this.contents = [];
    this.search = "";
    const res = await Axios.get<ContentNode[]>("/api/video/" + (this.$route.params.request ?? ""));
    this.filtered = this.contents = res.data;
    this.loading = false;
  }

  @Watch("search")
  private onSearchChanged() {
    if (this.search) {
      const search = this.search;
      this.filtered = this.contents.filter(n => n.name.toUpperCase().includes(search.toUpperCase()));
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
