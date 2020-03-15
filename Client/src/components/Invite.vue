<template>
  <div>
    <v-btn @click="onInviteClick">invite</v-btn>
    <p>code : {{ code }}</p>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { app } from "firebase/app";
import "firebase/functions";

@Component({})
export default class Invite extends Vue {
  private functions = app().functions("asia-northeast1");
  private invite = this.functions.httpsCallable("invite");
  private code: string | null = null;

  async onInviteClick() {
    try {
      const res = await this.invite();
      this.code = res.data;
    } catch (error) {
      alert(error);
    }
  }
}
</script>
