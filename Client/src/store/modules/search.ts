import { createModule, mutation } from "vuex-class-component";

const VuexModule = createModule({
  namespaced: "search",
  strict: false,
});

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

export default class Search extends VuexModule {
  filter = "";
  sorts: SortOrder[] = [];

  @mutation
  setSortOrder(value: SortOrder) {
    const index = this.sorts.findIndex(s => s.path === value.path);
    if (index >= 0) {
      this.sorts.splice(index, 1, value);
    } else {
      this.sorts.push(value);
    }
  }
}
