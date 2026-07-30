import * as React from "react";
/** Custom-painted checkbox — 13px box, 3px radius, accent fill with a dark tick. */
export interface CheckboxProps {
  checked?: boolean;
  onChange?: (checked: boolean) => void;
  disabled?: boolean;
  label?: string;
  style?: React.CSSProperties;
}
export declare function Checkbox(props: CheckboxProps): JSX.Element;
