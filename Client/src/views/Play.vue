<template>
  <div>
    <Player v-if="streamPath !== ''" :path="streamPath" />
    <div>{{ decodedPath }}</div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import Player from "@/components/Player.vue";
import { AuthModule } from "../store";
import { Prop } from "vue-property-decorator";

@Component({ components: { Player } })
export default class Play extends Vue {
  private streamPath = "";
  private readonly delay: (msec: number) => Promise<void> = msec => new Promise(resolve => setTimeout(resolve, msec));

  @Prop({ required: true, type: String, default: "" })
  path!: string;

  get decodedPath() {
    return decodeURIComponent(this.path);
  }

  private async created() {
    if (!this.path) {
      return;
    }
    const axios = await AuthModule.getAxios();
    const res = await axios.post<string>(`/api/video/transcode/?path=${this.path}`);
    this.streamPath = res.data;
  }
}
</script>
