<template>
  <v-container fill-height>
    <v-row no-gutters class="fill-height mt-4">
      <v-col md="4" cols="12" class="pa-2">
        <section class="d-flex align-center flex-column">
          <v-avatar :size="200" color="primary">
            <v-img v-if="user.photoURL" :src="user.photoURL" alt="avatar">
              <template #placeholder>
                <v-row class="fill-height" align="center" justify="center">
                  <v-progress-circular indeterminate color="primary"></v-progress-circular>
                </v-row>
              </template>
            </v-img>
            <span v-else class="white--text display-4">{{ user.email[0] }}</span>
          </v-avatar>
          <p class="headline font-weight-bold ma-2">{{ user.displayName }}</p>
          <address class="caption">{{ user.email }}</address>
        </section>
      </v-col>
      <v-col md="8" cols="12">
        <v-tabs v-model="activeTab">
          <v-tab>アカウント</v-tab>
          <v-tab>招待</v-tab>
        </v-tabs>
        <v-tabs-items v-model="activeTab" class="fill-height">
          <v-tab-item class="pa-2">
            <section class="ma-2">
              <header class="headline">サインアウト</header>
              <p>サインアウトし、サインイン画面に遷移します</p>
              <v-btn color="info" @click="signOut">サインアウト</v-btn>
            </section>
            <v-divider class="my-4" />
            <section class="ma-2">
              <header class="headline">他のアカウントと連携</header>
              <p>連携したアカウントでログインできます</p>
              <div class="d-flex flex-column providers">
                <v-btn v-for="id in targetProviders" :key="id" class="ma-1" :loading="isLinking(id)" @click="link(id)">
                  {{ linkText(id) }}
                </v-btn>
              </div>
            </section>
            <v-divider class="my-4" />
            <section class="ma-2">
              <header class="headline">退会</header>
              <p>アカウントを削除します</p>
              <v-btn color="error" @click="deleteMe">退会</v-btn>
            </section>
          </v-tab-item>
          <v-tab-item class="pa-2">
            <Invite />
          </v-tab-item>
        </v-tabs-items>
      </v-col>
    </v-row>
  </v-container>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import Invite from "@/components/Invite.vue";
import { User, auth } from "firebase/app";
import "firebase/auth";
import { AuthModule, GeneralModule } from "../store";
import { assertIsDefined } from "../utilities/assert";

@Component({ components: { Invite } })
export default class Account extends Vue {
  private user!: User;
  private activeTab: any = null;
  private targetProviders = [
    auth.GoogleAuthProvider.PROVIDER_ID,
    // auth.FacebookAuthProvider.PROVIDER_ID,
    // auth.TwitterAuthProvider.PROVIDER_ID,
    auth.GithubAuthProvider.PROVIDER_ID,
  ];
  private linkedProviders: string[] = [];
  private linking: string[] = [];

  get linkText() {
    return (id: string) => {
      switch (id) {
        case auth.GoogleAuthProvider.PROVIDER_ID:
          return "Google" + (this.isLinked(id) ? " リンク解除" : "");
        case auth.FacebookAuthProvider.PROVIDER_ID:
          return "Facebook" + (this.isLinked(id) ? " リンク解除" : "");
        case auth.TwitterAuthProvider.PROVIDER_ID:
          return "Twitter" + (this.isLinked(id) ? " リンク解除" : "");
        case auth.GithubAuthProvider.PROVIDER_ID:
          return "GitHub" + (this.isLinked(id) ? " リンク解除" : "");
        default:
          throw new Error("未対応プロバイダ");
      }
    };
  }

  get isLinking() {
    return (id: string) => this.linking.includes(id);
  }

  private created() {
    const user = AuthModule.user;
    assertIsDefined(user);
    this.user = user;
    this.linkedProviders = this.user.providerData.map(p => p?.providerId ?? "");
    this.linkedRedirect();
  }

  private async signOut() {
    GeneralModule.setLoading(true);
    await AuthModule.signOut();
    GeneralModule.setLoading(false);
    this.$router.push("/");
  }

  private async linkedRedirect() {
    const result = await auth().getRedirectResult();
    if (result.operationType !== "link") {
      return;
    }
    const info = result.additionalUserInfo;
    assertIsDefined(info?.profile);
    switch (info.providerId) {
      case auth.GoogleAuthProvider.PROVIDER_ID:
        this.linkedGoogle(info.profile);
        break;
      case auth.GithubAuthProvider.PROVIDER_ID:
        this.linkedGitHub(info.profile);
        break;
      default:
        throw new Error("未対応プロバイダ");
    }
  }

  private async linkedGoogle(profile: Record<string, any>) {
    const newProf: Record<string, any> = {};
    if (!this.user.displayName && profile["name"]) {
      newProf.displayName = profile["name"];
    }
    if (!this.user.photoURL && profile["picture"]) {
      newProf.photoURL = profile["picture"];
    }
    await this.user.updateProfile(newProf);
    const user = auth().currentUser;
    assertIsDefined(user);
    this.user = user;
  }

  private async linkedGitHub(profile: Record<string, any>) {
    const newProf: Record<string, any> = {};
    if (!this.user.displayName && profile["name"]) {
      newProf.displayName = profile["name"];
    }
    if (!this.user.photoURL && profile["avatar_url"]) {
      newProf.photoURL = profile["avatar_url"];
    }
    await this.user.updateProfile(newProf);
    const user = auth().currentUser;
    assertIsDefined(user);
    this.user = user;
  }

  private async deleteMe() {
    GeneralModule.setLoading(true);
    try {
      if (!this.user) {
        throw new Error("ログインしていません");
      }
      await this.user.delete();
    } catch (error) {
      alert("退会処理が正常に完了しませんでした。再度ログインして退会してください");
      GeneralModule.setLoading(false);
      this.$router.push("/signin");
    }
    GeneralModule.setLoading(false);
    this.$router.push("/");
  }

  private async link(id: string) {
    this.linking.push(id);
    if (this.isLinked(id)) {
      await this.user.unlink(id);
      this.linkedProviders = this.user.providerData.map(p => p?.providerId ?? "");
    } else {
      await this.user.linkWithRedirect(this.getProvider(id));
    }
    this.linking = this.linking.filter(i => i !== id);
  }

  private getProvider(id: string): auth.AuthProvider {
    switch (id) {
      case auth.GoogleAuthProvider.PROVIDER_ID:
        return new auth.GoogleAuthProvider();
      case auth.FacebookAuthProvider.PROVIDER_ID:
        return new auth.FacebookAuthProvider();
      case auth.TwitterAuthProvider.PROVIDER_ID:
        return new auth.TwitterAuthProvider();
      case auth.GithubAuthProvider.PROVIDER_ID:
        return new auth.GithubAuthProvider();
      default:
        throw new Error("未対応プロバイダ");
    }
  }

  private isLinked(id: string) {
    return this.linkedProviders.includes(id);
  }
}
</script>
<style lang="scss" scoped>
.providers {
  max-width: 400px;
}
</style>
