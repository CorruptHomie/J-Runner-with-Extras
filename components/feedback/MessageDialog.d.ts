import * as React from "react";
/**
 * Small borderless modal - 12px radius, hairline outline, bold title over muted body.
 * @startingPoint section="Feedback" subtitle="Confirm / acknowledge modal" viewport="700x260"
 */
export interface MessageDialogProps {
  title: string;
  message: React.ReactNode;
  /** "yesno" = destructive confirm (Yes is the accent button); "ok" = acknowledgement */
  kind?: "ok" | "yesno";
  onYes?: () => void;
  onNo?: () => void;
  onOk?: () => void;
  width?: number;
  style?: React.CSSProperties;
}
export declare function MessageDialog(props: MessageDialogProps): JSX.Element;
