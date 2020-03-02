import ContentNode from "@/models/ContnetNode";

export const getThumbnailPath = (content: ContentNode) =>
  "/api/thumbnail/?path=" + encodeURIComponent(content.contentPath);
