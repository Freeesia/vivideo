<template>
  <section class="ma-2">
    <header class="headline">ユーザーの招待</header>
    <p>ユーザーを招待するための招待URLを生成します</p>
    <v-btn :loading="generating" @click="onInviteClick">invite</v-btn>
    <div v-if="code">
      <p>以下のURLから新規ユーザーを1名登録可能です</p>
      <code>{{ host + code }}</code>
    </div>
  </section>
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
  private generating = false;
  private host = window.location.origin + "/signup/";
  private code: string | null = null;

  async onInviteClick() {
    this.generating = true;
    try {
      const res = await this.invite();
      this.code = res.data;
    } catch (error) {
      alert(error);
    }
    this.generating = false;
  }
}
</script>
