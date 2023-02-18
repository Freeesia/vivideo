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
import { invite } from "@/firebase/functions";
import Vue from "vue";
import Component from "vue-class-component";

@Component({})
export default class Invite extends Vue {
  public readonly host = window.location.origin + "/signup/";
  public generating = false;
  public code: string | null = null;

  async onInviteClick() {
    this.generating = true;
    try {
      const res = await invite();
      this.code = res.data;
    } catch (error) {
      alert(error);
    }
    this.generating = false;
  }
}
</script>
