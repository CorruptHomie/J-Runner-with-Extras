import * as React from "react";
/** Custom-painted radio — 13px circle, accent ring + 5px accent dot when selected. */
export interface RadioProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  label?: string;
  name?: string;
  style?: React.CSSProperties;
}
export declare function Radio(props: RadioProps): JSX.Element;
