import * as React from "react";
/** Owner-drawn dropdown — closed display is a field well; the highlighted row fills with AccentDim. */
export interface SelectProps {
  label?: string;
  labelWidth?: number;
  value?: string;
  options?: string[];
  /** shown when nothing is chosen; the app's own wording is "None Selected" */
  placeholder?: string;
  onChange?: (value: string) => void;
  disabled?: boolean;
  width?: number | string;
  style?: React.CSSProperties;
}
export declare function Select(props: SelectProps): JSX.Element;
