import React from "react";

/* Theme.GroupBox_Paint: PanelBg fill, 6px rounded BorderSubtle frame whose top edge starts
   at half the caption height, with a gap punched out for the caption. Caption is
   TextSecondary, sits at x=11, and straddles the border line. */
export function GroupBox({ title, children, style, bodyStyle }) {
  return (
    <fieldset style={{ position: "relative", margin: 0, padding: 0, border: "none",
      background: "var(--jr-panel-bg)", ...style }}>
      <div style={{ position: "absolute", inset: `${title ? 7 : 0}px 0 0 0`,
        border: "1px solid var(--jr-border-subtle)", borderRadius: "var(--jr-radius-group)", pointerEvents: "none" }} />
      {title && (
        <legend style={{ position: "relative", margin: 0, padding: "0 3px", marginLeft: 9,
          background: "var(--jr-panel-bg)", color: "var(--jr-text-secondary)",
          font: "400 var(--jr-text-sm)/1.15 var(--jr-font-ui)" }}>{title}</legend>
      )}
      <div style={{ position: "relative", padding: `${title ? 8 : 12}px 10px 10px`, ...bodyStyle }}>{children}</div>
    </fieldset>
  );
}
