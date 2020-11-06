<template>
  <v-dialog v-model="value" @click:outside="close">
    <v-card :disabled="setting">
      <v-card-title>ロゴ設定</v-card-title>
      <v-card-subtitle>{{ name }}</v-card-subtitle>

      <v-form v-model="valid">
        <v-text-field v-model="url" class="ma-2" :rules="rules" label="ロゴ URL"></v-text-field>
      </v-form>

      <v-img contain class="ma-2" max-height="256" :src="thumbnail"></v-img>

      <v-card-actions>
        <v-spacer></v-spacer>

        <v-btn color="primary" text @click="close"> Cancel </v-btn>

        <v-btn color="primary" depressed :loading="setting" :disabled="!valid" @click="submit"> OK </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { Model, Prop, Emit } from "vue-property-decorator";
import ContentNode from "../models/ContnetNode";
import { getThumbnailPath } from "../utilities/pathUtility";
import { AuthModule } from "../store";

@Component
export default class Logo extends Vue {
  @Model("update", { type: Boolean, required: true })
  private value!: boolean;

  @Prop({ required: true })
  private target!: ContentNode | null;

  private url = "";
  private rules = [
    (v: string) => !!v || "Required.",
    (v: string) => {
      const pattern = /^https?:\/\/.+$/;
      return pattern.test(v) || "Invalid url";
    },
  ];
  private valid = false;
  private setting = false;

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

  @Emit("update")
  private close() {
    this.url = "";
    this.setting = false;
    return false;
  }

  private async submit() {
    if (!this.target) {
      return;
    }
    this.setting = true;
    const axios = await AuthModule.getAxios();
    await axios.post("/api/thumbnail", {
      url: this.url,
      output: this.target.contentPath,
    });
    this.close();
  }
}
</script>
