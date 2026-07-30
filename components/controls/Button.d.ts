import * as React from "react";

/**
 * Flat dark button, owner-drawn in Theme.Button_Paint - hairline border, radius scales with height.
 * @startingPoint section="Controls" subtitle="Flat dark buttons, accent + danger variants" viewport="700x180"
 */
export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  /** default = RaisedBg surface; primary = accent green (dialogs, Program); danger = destructive */
  variant?: "default" | "primary" | "danger";
  /** sm 22px | md 26px (designer default) | lg 32px (MessageDialog) | xl 34px (Program) */
  size?: "sm" | "md" | "lg" | "xl";
  disabled?: boolean;
  /** fill the parent column, as in MainForm's stacked action columns */
  block?: boolean;
  icon?: React.ReactNode;
  children?: React.ReactNode;
}
export declare function Button(props: ButtonProps): JSX.Element;
