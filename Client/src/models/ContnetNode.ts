export default interface ContentNode {
  name: string;
  contentPath: string;
  isDirectory: boolean;
  createdAt: string;
  transcoded: boolean;
}
