import * as React from "react";
/**
 * Thin themed progress bar shared by reads, ECC builds and every non-flash operation.
 * @startingPoint section="Feedback" subtitle="Accent-filled progress track with % label" viewport="700x120"
 */
export interface ProgressBarProps {
  value?: number;
  max?: number;
  /** indeterminate - fills the track and reads "Working..." */
  marquee?: boolean;
  height?: number;
  showText?: boolean;
  style?: React.CSSProperties;
}
export declare function ProgressBar(props: ProgressBarProps): JSX.Element;
