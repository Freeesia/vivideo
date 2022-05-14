export function getThumbnailPath(path: string) {
  return "/api/thumbnail/?path=" + encodeURIComponent(path);
}
