<template>
  <v-app>
    <v-app-bar app color="primary" dark dense>
      <router-link to="/" aria-label="Frix TV Prime" title="Frix TV Prime">
        <v-toolbar-title>Frix TV Prime</v-toolbar-title>
      </router-link>
      <v-spacer></v-spacer>
      <v-text-field
        v-if="isSignedIn && isHome"
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

      <v-menu v-if="isSignedIn && isHome" offset-y open-on-hover :close-on-content-click="false">
        <template #activator="{ on }">
          <v-btn icon v-on="on">
            <v-icon>sort</v-icon>
          </v-btn>
        </template>

        <v-list>
          <v-list-item dense @click="selectSort(0)">
            <v-icon>sort_by_alpha</v-icon> <v-icon dense>{{ isOrderSelected(0) }}</v-icon>
          </v-list-item>
          <v-list-item dense @click="selectSort(1)">
            <v-icon>access_time</v-icon> <v-icon dense>{{ isOrderSelected(1) }}</v-icon>
          </v-list-item>
        </v-list>
      </v-menu>
      <v-menu offset-y>
        <template #activator="{ on }">
          <v-btn icon v-on="on">
            <v-icon>more_vert</v-icon>
          </v-btn>
        </template>

        <v-list>
          <v-list-item dense :disabled="!isSignedIn" to="/account">Account</v-list-item>
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
<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { AuthModule, SearchModule, GeneralModule } from "./store";
import { SortType, OrderType } from "./store/modules/search";

@Component({})
export default class App extends Vue {
  private search = "";
  private sort = SortType.Name;
  private order = OrderType.Asc;

  get loading() {
    return GeneralModule.loading;
  }

  get isHome() {
    return this.$route.name === "home";
  }

  get currentPath(): string {
    return this.$route.params.request ?? "";
  }

  get isSignedIn() {
    return AuthModule.user ? true : false;
  }

  private mounted() {
    this.$store.watch(
      (state) => state.search.filter,
      (newValue) => {
        this.search = newValue;
      },
      { immediate: true }
    );
  }

  @Watch("search")
  private onSearchChanged() {
    SearchModule.setFilter(this.search ?? "");
  }

  @Watch("currentPath", { immediate: true })
  private onCurrentPathChanged() {
    const sortOrder = SearchModule.sorts.find((v) => v.path === this.currentPath);
    this.order = sortOrder?.order ?? OrderType.Asc;
    this.sort = sortOrder?.sort ?? SortType.Name;
  }

  private selectSort(sort: SortType) {
    if (this.sort === sort) {
      switch (this.order) {
        case OrderType.Asc:
          this.order = OrderType.Desc;
          break;
        case OrderType.Desc:
          this.order = OrderType.Asc;
          break;
        default:
          throw new RangeError("OrderType out of range");
      }
    } else {
      this.sort = sort;
    }
    SearchModule.setSortOrder({ path: this.currentPath, sort: this.sort, order: this.order });
  }

  private isOrderSelected(sort: SortType): string {
    if (this.sort === sort) {
      switch (this.order) {
        case OrderType.Asc:
          return "arrow_upward";
        case OrderType.Desc:
          return "arrow_downward";
        default:
          throw new RangeError(`OrderType out of range: ${this.order}`);
      }
    } else {
      return "";
    }
  }
}
</script>
<style lang="scss" scoped>
.v-toolbar__title {
  color: white;
}
</style>
