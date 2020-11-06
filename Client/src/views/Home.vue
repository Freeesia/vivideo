<template>
  <v-container fluid>
    <v-row dense>
      <v-col v-for="item in filtered" :key="item.name" cols="12" sm="6" md="4" lg="3" xl="2">
        <v-card height="240" @click="selectContent(item)">
          <v-img contain height="160" :src="getThumbnailPath(item)">
            <template #placeholder>
              <v-row class="fill-height" align="center" justify="center">
                <v-icon size="100">{{ item.isDirectory ? "video_library" : "movie" }}</v-icon>
              </v-row>
            </template>
            <template #default>
              <v-row class="pa-4" align="start" justify="end">
                <v-menu v-if="item.isDirectory" offset-y>
                  <template #activator="{ on }">
                    <v-btn small icon v-on="on">
                      <v-icon>more_vert</v-icon>
                    </v-btn>
                  </template>

                  <v-list>
                    <v-list-item dense @click.stop="openLogoDialog(item)">Logo</v-list-item>
                  </v-list>
                </v-menu>
                <v-avatar v-else-if="item.transcoded" color="white" size="32">
                  <v-icon large color="teal">play_circle_filled</v-icon>
                </v-avatar>
              </v-row>
            </template>
          </v-img>
          <v-tooltip top>
            <template #activator="{ on }">
              <v-card-title v-on="on">{{ item.name }}</v-card-title>
            </template>
            <span>{{ item.name }}</span>
          </v-tooltip>
        </v-card>
      </v-col>
    </v-row>
    <logo v-model="logoDialog" :target="logoTarget"></logo>
  </v-container>
</template>

<script lang="ts">
import Vue from "vue";
import Logo from "@/components/Logo.vue";
import { Component, Watch, Prop } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import { AxiosInstance } from "axios";
import { AuthModule, SearchModule, GeneralModule } from "../store";
import { getThumbnailPath } from "../utilities/pathUtility";
import { compareFunc } from "../utilities/sortUtility";

@Component({ components: { Logo } })
export default class Home extends Vue {
  private selectedContent: ContentNode | null = null;
  private contents: ContentNode[] = [];
  private axios?: AxiosInstance;
  private logoDialog = false;
  private logoTarget: ContentNode | null = null;
  private getThumbnailPath = getThumbnailPath;

  @Prop({ required: true, type: String, default: "" })
  path!: string;

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
    GeneralModule.setLoading(true);
    this.contents = [];
    if (!this.axios) {
      this.axios = await AuthModule.getAxios();
    }
    const res = await this.axios.get<ContentNode[]>("/api/video/" + this.path);
    this.contents = res.data;
    SearchModule.setFilter("");
    GeneralModule.setLoading(false);
  }

  get filtered() {
    const search = SearchModule.filter.toUpperCase();
    const comp = compareFunc(SearchModule.sorts.find(v => v.path === this.path));
    return this.contents.filter(n => (search ? n.name.toUpperCase().includes(search) : true)).sort(comp);
  }

  private selectContent(content: ContentNode) {
    this.selectedContent = content;
    if (this.selectedContent ? !this.selectedContent.isDirectory : false) {
      this.$router.push({
        name: "play",
        query: {
          path: content.contentPath,
        },
      });
    } else {
      this.$router.push({
        name: "home",
        params: {
          request: content.contentPath,
        },
      });
    }
  }

  private openLogoDialog(content: ContentNode) {
    this.logoTarget = content;
    this.logoDialog = true;
  }

  @Watch("logoDialog")
  private async logoDialogChanged(newValue: boolean) {
    if (!newValue) {
      this.logoTarget = null;
    }
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
