import * as React from "react";
/**
 * Titled 6px-rounded frame — the app's only grouping container. Caption straddles the top border.
 * @startingPoint section="Layout" subtitle="Titled panel frame with an inset caption" viewport="700x160"
 */
export interface GroupBoxProps {
  /** omit for an untitled frame; the border then closes at the top */
  title?: string;
  children?: React.ReactNode;
  style?: React.CSSProperties;
  bodyStyle?: React.CSSProperties;
}
export declare function GroupBox(props: GroupBoxProps): JSX.Element;
