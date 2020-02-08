<template>
  <v-app>
    <v-app-bar app color="primary" dark dense>
      <router-link to="/" aria-label="Frix TV Prime" title="Frix TV Prime">
        <v-toolbar-title>Vivideo</v-toolbar-title>
      </router-link>
      <v-spacer></v-spacer>

      <v-menu offset-y>
        <template v-slot:activator="{ on }">
          <v-btn icon v-on="on">
            <v-icon>more_vert</v-icon>
          </v-btn>
        </template>

        <v-list>
          <v-list-item dense :disabled="!isSignedIn" href="/hangfire">Hangfire</v-list-item>
          <v-list-item dense :disabled="!isSignedIn" @click="signout">Sign out</v-list-item>
        </v-list>
      </v-menu>
    </v-app-bar>

    <v-content>
      <router-view></router-view>
    </v-content>
  </v-app>
</template>
<style lang="scss" scoped>
.v-toolbar__title {
  color: white;
}
</style>
<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { AuthModule } from "./store";

@Component({})
export default class App extends Vue {
  private get isSignedIn() {
    return AuthModule.user ? true : false;
  }
  private async signout() {
    await AuthModule.signOut();
    this.$router.push({ name: "signin" });
  }
}
</script>
