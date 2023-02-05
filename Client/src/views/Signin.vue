<template>
  <v-container>
    <div id="auth-container"></div>
    <v-alert v-if="codeError" type="error" prominent large
      >招待コードにより作成したユーザー以外サインイン出来ません</v-alert
    >
  </v-container>
</template>

<script lang="ts">
import Vue from "vue";
import { auth as authui } from "firebaseui";
import { useDeviceLanguage, EmailAuthProvider, GoogleAuthProvider, GithubAuthProvider, User } from "firebase/auth";
import Component from "vue-class-component";
import "firebaseui/dist/firebaseui.css";
// firebaseuiがまだv9に対応していないので、互換ライブラリを利用する
import firebase from "firebase/compat/app";
import "firebase/compat/auth";
import { firebaseOptions } from "@/firebase";

@Component
export default class Signin extends Vue {
  public codeError = false;

  private created() {
    firebase.initializeApp(firebaseOptions);
    useDeviceLanguage(firebase.auth());
  }

  private mounted() {
    let ui = authui.AuthUI.getInstance();
    if (!ui) {
      ui = new authui.AuthUI(firebase.auth());
    }
    ui.start("#auth-container", {
      signInOptions: [
        {
          provider: EmailAuthProvider.PROVIDER_ID,
          requireDisplayName: false,
        },
        GoogleAuthProvider.PROVIDER_ID,
        GithubAuthProvider.PROVIDER_ID,
      ],
      credentialHelper: authui.CredentialHelper.NONE,
      callbacks: {
        signInSuccessWithAuthResult: (res: any) => {
          this.signInSuccess(res.user as User);
          return false;
        },
      },
    });
  }

  private async signInSuccess(user: User) {
    const token = await user.getIdTokenResult();
    if (!token.claims.invitationCodeVerified) {
      this.codeError = true;
      return;
    }
    this.$router.push({
      name: "home",
    });
  }
}
</script>
