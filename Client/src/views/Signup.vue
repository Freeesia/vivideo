<template>
  <v-container>
    <v-alert v-if="!code" type="error" prominent>招待コードが指定されていません</v-alert>
    <v-row v-if="isCodeValid === null" justify="center">
      <v-progress-circular indeterminate width="5" size="96" color="primary"></v-progress-circular>
    </v-row>
    <v-card v-if="isCodeValid">
      <v-card-text>
        <v-form ref="form" v-model="valid">
          <v-text-field
            v-model="email"
            type="text"
            autocomplete="email"
            :rules="emailRules"
            label="E-mail"
            required
          ></v-text-field>
          <v-text-field
            v-model="password"
            type="password"
            autocomplete="new-password"
            counter
            :rules="passwordRules"
            label="Password"
            required
          ></v-text-field>
        </v-form>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn :disabled="!valid" :loading="isSubmiting" large depressed block color="success" @click="submit"
          >登録</v-btn
        >
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import firebase from "firebase/app";
import "firebase/functions";
import app = firebase.app;

@Component({})
export default class Signup extends Vue {
  private readonly functions = app().functions("asia-northeast1");
  private readonly checkCode = this.functions.httpsCallable("checkInvitationCode");
  private readonly signup = this.functions.httpsCallable("signup");
  private readonly emailRules = [
    (v: string) => !!v || "E-mail is required",
    (v: string) => /.+@.+\..+/.test(v) || "E-mail must be valid",
  ];
  private readonly passwordRules = [
    (v: string) => !!v || "Password is required",
    (v: string) => v?.length > 8 || v?.length < 16 || "パスワードは8文字以上16文字未満",
  ];
  private isCodeValid: boolean | null = null;
  private code = "";
  private valid = false;
  private email = "";
  private password = "";
  private isSubmiting = false;

  private async mounted() {
    this.code = this.$route.params.code;
    if (!this.code) {
      this.isCodeValid = false;
      return;
    }
    try {
      await this.checkCode({ invitationCode: this.code });
    } catch (error) {
      this.isCodeValid = false;
      return;
    }
    this.isCodeValid = true;
  }

  private async submit() {
    try {
      this.isSubmiting = true;
      await this.signup({
        email: this.email,
        password: this.password,
        invitationCode: this.code,
      });
      this.$router.push({ name: "home" });
    } catch (error) {
      alert(error);
    }
  }
}
</script>
