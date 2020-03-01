<template>
  <v-app>
    <v-app-bar app color="primary" dark dense>
      <router-link to="/" aria-label="Frix TV Prime" title="Frix TV Prime">
        <v-toolbar-title>Frix TV Prime</v-toolbar-title>
      </router-link>
      <v-spacer></v-spacer>
      <v-text-field
        v-if="isSignedIn"
        v-model="search"
        label="検索"
        prepend-inner-icon="search"
        hide-details
        clearable
        dense
        outlined
        flat
        solo-inverted
      />

      <v-menu offset-y>
        <template v-slot:activator="{ on }">
          <v-btn icon v-on="on">
            <v-icon>more_vert</v-icon>
          </v-btn>
        </template>

        <v-list>
          <v-list-item dense :disabled="!isSignedIn" @click="signout">Sign out</v-list-item>
          <v-list-item dense :disabled="!isSignedIn" to="/about">About</v-list-item>
        </v-list>
      </v-menu>
      <v-progress-linear
        striped
        :active="loading"
        background-color="accent"
        color="secondary"
        indeterminate
        absolute
        top
      />
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
import { Component, Watch } from "vue-property-decorator";
import { AuthModule, SearchModule, GeneralModule } from "./store";

@Component({})
export default class App extends Vue {
  private search = "";

  get loading() {
    return GeneralModule.loading;
  }

  private mounted() {
    this.$store.watch(
      state => state.search.filter,
      newValue => {
        this.search = newValue;
      },
      { immediate: true }
    );
  }

  @Watch("search")
  private onSearchChanged() {
    SearchModule.setFilter(this.search ?? "");
  }

  private get isSignedIn() {
    return AuthModule.user ? true : false;
  }
  private async signout() {
    await AuthModule.signOut();
    this.$router.push({ name: "signin" });
  }
}
</script>
