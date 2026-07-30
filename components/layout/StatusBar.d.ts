import * as React from "react";
/** Bottom status strip — label/value pairs for bundled tool versions. */
export interface StatusBarItem { label: string; value: string }
export interface StatusBarProps { items?: StatusBarItem[]; style?: React.CSSProperties }
export declare function StatusBar(props: StatusBarProps): JSX.Element;
