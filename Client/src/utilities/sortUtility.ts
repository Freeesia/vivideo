import ContentNode from "@/models/ContnetNode";
import { OrderType, SortType, SortOrder } from "@/store/modules/search";
import { compare } from "natural-orderby";

export function compareFunc(sortOrder?: SortOrder): (x: ContentNode, y: ContentNode) => number {
  const order = sortOrder?.order ?? OrderType.Asc;
  switch (sortOrder?.sort ?? SortType.Name) {
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
