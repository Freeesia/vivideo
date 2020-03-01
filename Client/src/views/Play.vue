<template>
  <div>
    <Player :stream-path="streamPath" :thumbnail-path="thumbnailPath" />
    <div>{{ path }}</div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import Player from "@/components/Player.vue";
import { AuthModule, GeneralModule } from "../store";
import { Prop } from "vue-property-decorator";

@Component({ components: { Player } })
export default class Play extends Vue {
  private streamPath = "";
  private thumbnailPath = "";

  @Prop({ required: true, type: String, default: "" })
  path!: string;

  private async created() {
    if (!this.path) {
      throw new Error("path not seted.");
    }
    GeneralModule.setLoading(true);
    this.thumbnailPath = "/api/thumbnail/?path=" + encodeURIComponent(this.path);
    const axios = await AuthModule.getAxios();
    const res = await axios.post<string>(`/api/video/transcode/?path=${encodeURIComponent(this.path)}`);
    this.streamPath = res.data;
    GeneralModule.setLoading(false);
  }
}
</script>
