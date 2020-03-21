import { VuexModule, Module, Mutation } from "vuex-module-decorators";

export const enum SortType {
  Name,
  UpdatedAt
}

export const enum OrderType {
  Asc,
  Desc
}

export interface SortOrder {
  path: string;
  sort: SortType;
  order: OrderType;
}

@Module({ namespaced: true, name: "search" })
export default class Search extends VuexModule {
  filter = "";
  sort = SortType.Name;
  order = OrderType.Asc;
  sorts: SortOrder[] = [];

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

  @Mutation
  setSortOrder(value: SortOrder) {
    const index = this.sorts.findIndex(s => s.path === value.path);
    if (index >= 0) {
      this.sorts.splice(index, 1, value);
    } else {
      this.sorts.push(value);
    }
  }
}
