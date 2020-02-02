<template>
  <v-btn @click="onSignupClick">Signup</v-btn>
</template>

<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { app } from "firebase/app";
import "firebase/functions";

@Component({})
export default class Signup extends Vue {
  private functions = app().functions("asia-northeast1");
  private checkCode = this.functions.httpsCallable("checkInvitationCode");
  private signup = this.functions.httpsCallable("signup");

  async onSignupClick() {
    try {
      const res = await this.checkCode({ invitationCode: "wawawa" });
      alert(res);
    } catch (error) {
      alert(error);
    }
  }
}
</script>
