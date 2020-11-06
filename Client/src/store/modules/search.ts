import { VuexModule, Module, Mutation } from "vuex-module-decorators";

export const enum SortType {
  Name,
  UpdatedAt,
}

export const enum OrderType {
  Asc,
  Desc,
}

export interface SortOrder {
  path: string;
  sort: SortType;
  order: OrderType;
}

@Module({ namespaced: true, name: "search" })
export default class Search extends VuexModule {
  filter = "";
  sorts: SortOrder[] = [];

  @Mutation
  setFilter(value: string) {
    this.filter = value;
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
