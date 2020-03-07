import ContentNode from "@/models/ContnetNode";

export function getThumbnailPath(content: ContentNode) {
  return "/api/thumbnail/?path=" + encodeURIComponent(content.contentPath);
}
