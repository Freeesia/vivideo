export interface ContentNode {
  name: string;
  contentPath: string;
  isDirectory: boolean;
  createdAt: string;
  transcoded: boolean;
}

export interface HistoryVideo {
  path: string;
  current: number;
  lastUpdate: number;
}
