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
          <v-list-item dense href="/hangfire">Hangfire</v-list-item>
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

@Component({})
export default class App extends Vue {}
</script>
