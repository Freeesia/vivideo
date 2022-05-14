export function delay(msec: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, msec));
}

export function toRecord<T extends { [K in keyof T]: any }, K extends keyof T>(
  array: T[],
  selector: K
): Record<T[K], T> {
  return array.reduce((acc, item) => ((acc[item[selector]] = item), acc), {} as Record<T[K], T>);
}
