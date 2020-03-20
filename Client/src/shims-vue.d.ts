/* eslint-disable @typescript-eslint/class-name-casing */
declare module "*.vue" {
  import Vue from "vue";
  export default Vue;
}

declare module "shaka-player/dist/shaka-player.ui" {
  export class Player {
    constructor(mediaElement: HTMLMediaElement, dependencyInjector?: (player: Player) => void);
    static probeSupport(): Promise<extern.SupportType>;
    getLoadMode(): number;
    configure(config: extern.PlayerConfiguration): boolean;
    load(assetUri: string, mimeType?: string): Promise<void>;
    load(assetUri: string, startTimeopt: number, mimeType: string): Promise<void>;
    getMediaElement(): HTMLMediaElement;
    addEventListener(type: string, listener: EventListener | ((ev: Event) => void)): void;
    destroy(): void;
  }
  interface DrmSupportType {
    persistentState: boolean;
  }
  export namespace ui {
    export class Overlay {
      constructor(player: Player, videoContainer: HTMLElement, video: HTMLMediaElement);
      configure(config: string | extern.UIConfiguration, value?: any): void;
      getConfiguration(): extern.UIConfiguration;
    }
    enum TrackLabelFormat {
      LANGUAGE = 0,
      ROLE = 1,
      LANGUAGE_ROLE = 2
    }
  }
  namespace extern {
    interface SupportType {
      manifest: { [mime: string]: boolean };
      media: { [mime: string]: boolean };
      drm: { [name: string]: DrmSupportType };
    }
    interface PlayerConfiguration {
      streaming: StreamingConfiguration;
    }
    interface StreamingConfiguration {
      bufferBehind: number;
    }
    interface UIConfiguration {
      controlPanelElements?: string[];
      overflowMenuButtons?: string[];
      addSeekBar?: boolean;
      addBigPlayButton?: boolean;
      castReceiverAppId?: string;
      clearBufferOnQualityChange?: boolean;
      showUnbufferedStart?: boolean;
      seekBarColors?: UISeekBarColors;
      volumeBarColors?: UIVolumeBarColors;
      trackLabelFormat?: ui.TrackLabelFormat;
      fadeDelay?: number;
      doubleClickForFullscreen?: boolean;
    }
    interface UISeekBarColors {
      base: string;
      buffered: string;
      played: string;
      adBreaks: string;
    }
    interface UIVolumeBarColors {
      base: string;
      level: string;
    }
  }
  export class polyfill {
    static installAll(): void;
  }
}
