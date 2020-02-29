<template>
  <v-container>
    <v-card class="mx-auto" max-width="480">
      <v-card-title>About</v-card-title>
      <v-card-text>
        <v-row>
          <v-col>Client Version : </v-col>
          <v-col>{{ clientVersion }}</v-col>
        </v-row>
        <v-row>
          <v-col>Server Version : </v-col>
          <v-col>{{ serverVersion }}</v-col>
        </v-row>
        <v-row>
          <v-col>Worker Service : </v-col>
          <v-col><a href="/hangfire">Hangfire</a></v-col>
        </v-row>
      </v-card-text>
    </v-card>
  </v-container>
</template>
<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import Axios from "axios";

@Component
export default class About extends Vue {
  get clientVersion() {
    return process.env.VUE_APP_VERSION;
  }
  serverVersion = "Checking...";

  private async created() {
    try {
      const res = await Axios.get<string>("/api/info/version");
      this.serverVersion = res.data;
    } catch (error) {
      this.serverVersion = "Check failed";
    }
  }
}
</script>
