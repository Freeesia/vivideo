<template>
  <v-app>
    <v-app-bar app color="primary" dark dense>
      <v-toolbar-title>Vivideo</v-toolbar-title>
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
      <HelloWorld />
    </v-content>
  </v-app>
</template>

<script lang="ts">
import Vue from "vue";
import HelloWorld from "./components/HelloWorld.vue";
import { Component } from "vue-property-decorator";

@Component({ components: { HelloWorld } })
export default class App extends Vue {}
</script>
