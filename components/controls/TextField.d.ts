import * as React from "react";
/** Single-line text box — FieldBg well with a square 1px border (FixedSingle). Never rounded. */
export interface TextFieldProps {
  /** right-aligned caption to the left of the field, as in the Nand Info panel */
  label?: string;
  labelWidth?: number;
  value?: string;
  placeholder?: string;
  readOnly?: boolean;
  disabled?: boolean;
  /** render the value in the VGA mono face — CPU keys, hashes, hex */
  mono?: boolean;
  onChange?: (value: string) => void;
  width?: number | string;
  style?: React.CSSProperties;
}
export declare function TextField(props: TextFieldProps): JSX.Element;
