export interface ContentNode {
  name: string;
  contentPath: string;
  isDirectory: boolean;
  createdAt: string;
  transcoded: boolean;
  duration: number;
}

export interface HistoryVideo {
  path: string;
  current: number;
  lastUpdate: number;
}
