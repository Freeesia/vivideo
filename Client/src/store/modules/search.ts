import { VuexModule, Module, Mutation } from "vuex-module-decorators";
import { compare } from "natural-orderby";
import ContentNode from "@/models/ContnetNode";

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

  get compareFunc() {
    const order = this.order;
    switch (this.sort) {
      case SortType.Name: {
        const c = compare({ order: order === OrderType.Asc ? "asc" : "desc" });
        return (x: ContentNode, y: ContentNode) => c(x.name, y.name);
      }
      case SortType.UpdatedAt: {
        if (order === OrderType.Asc) {
          return (x: ContentNode, y: ContentNode) => new Date(x.createdAt).valueOf() - new Date(y.createdAt).valueOf();
        } else {
          return (x: ContentNode, y: ContentNode) => new Date(y.createdAt).valueOf() - new Date(x.createdAt).valueOf();
        }
      }
      default:
        throw new RangeError("out of sorttype range");
    }
  }
}
