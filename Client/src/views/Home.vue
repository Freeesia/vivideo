<template>
  <v-container fluid>
    <v-toolbar v-if="segments.length > 0" flat class="mb-2">
      <v-toolbar-title v-for="(segment, index) in segments" :key="segment" class="ma-2 text-h5">
        / <router-link v-if="index + 1 !== segments.length" :to="getPath(segment)">{{ segment }}</router-link>
        <span v-else>{{ segment }}</span>
      </v-toolbar-title>
      <v-divider vertical class="pa-2" />
    </v-toolbar>
    <v-row dense>
      <v-col v-for="item in filtered" :key="item.name" cols="12" sm="6" md="4" lg="3" xl="2">
        <v-card height="260" @click="selectContent(item)">
          <video-thumbnail class="ma-1" :content="item" :history="histories[item.contentPath]">
            <v-row class="pa-4" align="start" justify="end">
              <v-avatar v-if="item.transcoded" color="white" size="32">
                <v-icon large color="teal">play_circle_filled</v-icon>
              </v-avatar>
              <v-menu offset-y>
                <template #activator="{ on }">
                  <v-btn small icon v-on="on">
                    <v-icon>more_vert</v-icon>
                  </v-btn>
                </template>
                <v-list v-if="item.isDirectory">
                  <v-list-item dense @click="openLogoDialog(item)">Logo</v-list-item>
                  <v-list-item dense @click="queuingAll(item)">全ての動画を再生可能にする</v-list-item>
                </v-list>
                <v-list v-else>
                  <v-list-item :disabled="!item.transcoded" dense @click="deleteCache(item)">
                    キャッシュの削除
                  </v-list-item>
                </v-list>
              </v-menu>
            </v-row>
          </video-thumbnail>
          <v-tooltip top>
            <template #activator="{ on }">
              <v-card-title v-on="on">{{ item.name }}</v-card-title>
            </template>
            <span>{{ item.name }}</span>
          </v-tooltip>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import Vue from "vue";
import Logo from "@/components/Logo.vue";
import { Component, Watch, Prop } from "vue-property-decorator";
import { ContentNode, HistoryVideo } from "@/model";
import { AxiosError, AxiosInstance } from "axios";
import { AuthModule, SearchModule, GeneralModule } from "../store";
import { getThumbnailPath } from "../utilities/pathUtility";
import { compareFunc } from "../utilities/sortUtility";
import { assertIsDefined } from "../utilities/assert";
import { toRecord } from "@/utilities/systemUtility";
import { getHistories } from "@/firebase/firestore";

@Component
export default class Home extends Vue {
  private selectedContent: ContentNode | null = null;
  private contents: ContentNode[] = [];
  private axios?: AxiosInstance;
  public readonly getThumbnailPath = getThumbnailPath;
  public histories: Record<string, HistoryVideo> = {};

  @Prop({ type: String, default: "" })
  public readonly path!: string;

  get segments() {
    return this.path.split("/").filter(v => v !== "");
  }

  get filtered() {
    const search = SearchModule.filter.toUpperCase();
    const comp = compareFunc(SearchModule.sorts.find(v => v.path === this.path));
    return this.contents.filter(n => (search ? n.name.toUpperCase().includes(search) : true)).sort(comp);
  }

  mounted() {
    this.$store.watch(
      state => state.search.path,
      () => {
        this.$forceUpdate();
      },
      { immediate: true, deep: true }
    );
  }

  @Watch("$route", { immediate: true, deep: true })
  private async onRequestChanged() {
    GeneralModule.loading = true;
    this.contents = [];
    if (!this.axios) {
      this.axios = await AuthModule.getAxios();
    }
    const last = this.segments.slice(-1)[0];
    document.title = last ? `Frix TV Prime: ${last}` : "Frix TV Prime";
    const res = await this.axios.get<ContentNode[]>("/api/video/" + this.path);
    this.contents = res.data;
    SearchModule.filter = "";
    this.histories = toRecord(await getHistories(this.path), "path");
    GeneralModule.loading = false;
  }

  public getPath(segment: string) {
    let path = "";
    for (const s of this.segments) {
      path += "/" + s;
      if (s === segment) {
        return path;
      }
    }
    return path;
  }

  public selectContent(content: ContentNode) {
    this.selectedContent = content;
    if (this.selectedContent ? !this.selectedContent.isDirectory : false) {
      this.$router.push(`/play/${encodeURIComponent(content.contentPath)}`);
    } else {
      this.$router.push("/" + content.contentPath);
    }
  }

  public openLogoDialog(target: ContentNode) {
    this.$dialog.show(Logo, {
      target,
    });
  }

  public async queuingAll(content: ContentNode) {
    assertIsDefined(this.axios);
    try {
      await this.axios.post<string>(`/api/video/transcode/all/${content.contentPath}`);
    } catch (error) {
      const e = error as AxiosError;
      this.$dialog.notify.error(e.message);
    }
  }

  public async deleteCache(content: ContentNode) {
    assertIsDefined(this.axios);
    await this.axios.delete("/api/video", {
      params: {
        path: content.contentPath,
      },
    });
    content.transcoded = false;
  }
}
</script>

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
