import path from "path";

export function getThumbnailPath(contentPath: string) {
  return "/api/thumbnail/?path=" + encodeURIComponent(contentPath);
}

export function getTitle(contentPath: string) {
  return path.basename(contentPath, path.extname(contentPath));
}
