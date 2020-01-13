<template>
  <div>
    <div>{{ path }}</div>
    <v-list>
      <v-list-item-group color="primary">
        <v-list-item v-for="item in contents" :key="item.name" @click="selectContent(item)">
          <v-list-item-content>
            <v-list-item-title>{{ item.name }}</v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </v-list-item-group>
    </v-list>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Player from "@/components/Player.vue";
import { Component } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import Axios from "axios";

@Component({ components: { Player } })
export default class Home extends Vue {
  private selectedContent: ContentNode | null = null;
  private contents: ContentNode[] = [];

  private get path(): string {
    return this.selectedContent ? this.selectedContent.contentPath : "";
  }

  private mounted() {
    this.updateContents();
  }

  private async updateContents() {
    const res = await Axios.get<ContentNode[]>("/api/video/" + this.path);
    this.contents = res.data;
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
      this.updateContents();
    }
  }
}
</script>
