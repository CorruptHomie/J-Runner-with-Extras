import * as React from "react";
/** Button with an attached dropdown arrow (UI.SplitButton) — used where one action has alternates. */
export interface SplitButtonProps {
  children?: React.ReactNode;
  onClick?: () => void;
  /** fired by the caret half */
  onOpen?: () => void;
  /** hold the caret half in its hover fill while a menu is open */
  open?: boolean;
  disabled?: boolean;
  style?: React.CSSProperties;
}
export declare function SplitButton(props: SplitButtonProps): JSX.Element;
