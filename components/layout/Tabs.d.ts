import * as React from "react";
/**
 * Owner-drawn tab strip — selected tab lifts to RaisedBg and gets a 2px accent underline.
 * @startingPoint section="Layout" subtitle="Dark tab strip with accent underline" viewport="700x200"
 */
export interface TabsProps {
  tabs?: string[];
  value?: string;
  onChange?: (tab: string) => void;
  children?: React.ReactNode;
  style?: React.CSSProperties;
  bodyStyle?: React.CSSProperties;
}
export declare function Tabs(props: TabsProps): JSX.Element;
