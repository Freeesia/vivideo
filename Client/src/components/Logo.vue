<template>
  <v-card :disabled="setting">
    <v-card-title>ロゴ設定</v-card-title>
    <v-card-subtitle>{{ name }}</v-card-subtitle>

    <v-form v-model="valid">
      <v-text-field v-model="url" class="ma-2" :rules="rules" label="ロゴ URL">
        <template #append-outer>
          <v-btn icon :href="searchLink" target="_blank">
            <v-icon>travel_explore</v-icon>
          </v-btn>
        </template>
      </v-text-field>
    </v-form>

    <v-img contain class="ma-2" max-height="256" :src="thumbnail"></v-img>

    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" text @click="cancel"> Cancel </v-btn>
      <v-btn color="primary" depressed :loading="setting" :disabled="!valid" @click="submit"> OK </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { Prop, Emit } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import { getThumbnailPath } from "../utilities/pathUtility";
import { AuthModule } from "../store";

@Component
export default class Logo extends Vue {
  @Prop({ required: true })
  public readonly target!: ContentNode;

  public url = "";
  public readonly rules = [
    (v: string) => !!v || "Required.",
    (v: string) => {
      const pattern = /^https?:\/\/.+$/;
      return pattern.test(v) || "Invalid url";
    },
  ];
  public valid = false;
  public setting = false;

  get name() {
    return this.target?.name;
  }

  get thumbnail() {
    if (this.url) {
      return this.url;
    } else if (this.target) {
      return getThumbnailPath(this.target);
    } else {
      return "";
    }
  }

  get searchLink() {
    return `https://www.google.com/search?q=${this.name} ロゴ&tbm=isch&tbs=ic:trans&hl=ja`;
  }

  @Emit("submit")
  public cancel() {
    this.url = "";
    this.setting = false;
  }

  @Emit()
  public async submit() {
    this.setting = true;
    const axios = await AuthModule.getAxios();
    await axios.post("/api/thumbnail", {
      url: this.url,
      output: this.target.contentPath,
    });
  }
}
</script>
