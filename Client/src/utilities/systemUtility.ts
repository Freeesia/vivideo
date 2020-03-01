export const delay: (msec: number) => Promise<void> = msec => new Promise(resolve => setTimeout(resolve, msec));
