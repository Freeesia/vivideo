<template>
  <v-container fill-height>
    <v-row no-gutters class="fill-height">
      <v-col :md="4" cols="12" class="ma-2">
        <section class="d-flex align-center flex-column">
          <v-avatar :size="200" color="primary">
            <v-img v-if="user.photoURL" :src="user.photoURL" alt="avatar">
              <template v-slot:placeholder>
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
      <v-col cols="12">
        <v-tabs v-model="activeTab">
          <v-tab>アカウント</v-tab>
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
              <header class="headline">退会</header>
              <p>PinPinからアカウントを削除します。すべてのブックマークは削除されます。</p>
              <v-btn color="error" @click="deleteMe">退会</v-btn>
            </section>
          </v-tab-item>
        </v-tabs-items>
      </v-col>
    </v-row>
  </v-container>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { User } from "firebase/app";
import { AuthModule, GeneralModule } from "../store";
import { assertIsDefined } from "../utilities/assert";

@Component
export default class Account extends Vue {
  private user!: User;
  private activeTab: any = null;
  private created() {
    const user = AuthModule.user;
    assertIsDefined(user);
    this.user = user;
  }

  private async signOut() {
    GeneralModule.setLoading(true);
    await AuthModule.signOut();
    GeneralModule.setLoading(false);
    this.$router.push("/");
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
}
</script>
