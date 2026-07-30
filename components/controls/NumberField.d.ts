import * as React from "react";
/** Numeric stepper (NumericUpDown) — e.g. "Nand Reads". Field well plus two stacked spin buttons. */
export interface NumberFieldProps {
  value?: number;
  min?: number;
  max?: number;
  onChange?: (value: number) => void;
  width?: number;
  disabled?: boolean;
  style?: React.CSSProperties;
}
export declare function NumberField(props: NumberFieldProps): JSX.Element;
