import * as React from "react";
/**
 * Hardware presence panel - the flasher photo when one is attached, a muted empty line when not.
 * @startingPoint section="Feedback" subtitle="Detected-flasher panel with empty state" viewport="700x200"
 */
export interface DeviceCardProps {
  /** path into assets/ - device-picoflasher.png, device-matrix.png, device-demon.png, ... */
  image?: string;
  name?: string;
  detail?: string;
  /** wording when nothing is attached; the app says "No flasher detected" */
  empty?: string;
  height?: number;
  style?: React.CSSProperties;
}
export declare function DeviceCard(props: DeviceCardProps): JSX.Element;
