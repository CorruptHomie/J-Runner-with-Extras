import * as React from "react";
/**
 * Full-window blocking sheet shown only while a NAND write runs - greyscale mark filling with colour.
 * @startingPoint section="Feedback" subtitle="Blocking flash-progress sheet" viewport="700x400"
 */
export interface FlashOverlayProps {
  value?: number;
  max?: number;
  caption?: string;
  hint?: string;
  /** path to the mark that fills; the app uses the console manufacturer's sphere from its own resources */
  logo?: string;
  size?: number;
  style?: React.CSSProperties;
}
export declare function FlashOverlay(props: FlashOverlayProps): JSX.Element;
