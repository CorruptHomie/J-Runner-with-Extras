import * as React from "react";
/**
 * Scrolling session log - the app's primary feedback channel. Borderless well, colour-coded lines.
 * @startingPoint section="Feedback" subtitle="Session log with severity colouring" viewport="700x240"
 */
export interface LogLine { text: string; severity?: "ok" | "info" | "warn" | "error" | "plain" | "muted" }
export interface LogConsoleProps {
  lines?: Array<string | LogLine>;
  height?: number;
  style?: React.CSSProperties;
}
export declare function LogConsole(props: LogConsoleProps): JSX.Element;
