import { VuexModule, Module, Mutation } from "vuex-module-decorators";

export const enum SortType {
  Name,
  UpdatedAt
}

export const enum OrderType {
  Asc,
  Desc
}

@Module({ namespaced: true, name: "search" })
export default class Search extends VuexModule {
  filter = "";
  sort = SortType.Name;
  order = OrderType.Asc;

  @Mutation
  setFilter(value: string) {
    this.filter = value;
  }

  @Mutation
  setSort(value: SortType) {
    this.sort = value;
  }

  @Mutation
  setOrder(value: OrderType) {
    this.order = value;
  }

  @Mutation
  setSortAndOrder(value: { sort: SortType; order: OrderType }) {
    this.sort = value.sort;
    this.order = value.order;
  }
}
