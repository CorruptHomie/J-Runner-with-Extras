import * as React from "react";
/**
 * Borderless-window chrome: 36px bar merging title, menu and window buttons.
 * @startingPoint section="Layout" subtitle="Merged title bar + menu strip, no maximize" viewport="700x120"
 */
export interface TitleBarProps {
  /** top-level menu labels, e.g. ["Tools","Nand","Advanced"] */
  menu?: string[];
  activeMenu?: string;
  onMenu?: (label: string) => void;
  /** right-aligned slot — the app puts the version string / About entry here */
  right?: React.ReactNode;
  onMinimize?: () => void;
  onClose?: () => void;
  /** src for the 16px JR mark that leads the strip */
  logo?: string;
  style?: React.CSSProperties;
}
export declare function TitleBar(props: TitleBarProps): JSX.Element;
