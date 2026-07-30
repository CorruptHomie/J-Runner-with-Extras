import React from "react";
import { Button } from "../controls/Button.jsx";

/* UI.MessageDialog: 400x190, PanelBg, 12px rounded region, 1px Border outline, owner dimmed
   at 45% black. Title 11pt bold TextPrimary at (20,18); body TextSecondary at (20,52);
   buttons 90x32 bottom-right, primary = accent. */
export function MessageDialog({ title, message, kind = "ok", onYes, onNo, onOk, width = 400, style }) {
  return (
    <div role="dialog" aria-label={title} style={{ width, background: "var(--jr-panel-bg)",
      border: "1px solid var(--jr-border)", borderRadius: "var(--jr-radius-dialog)",
      boxShadow: "var(--jr-shadow-dialog)", padding: "18px 20px 20px", ...style }}>
      <div style={{ color: "var(--jr-text-primary)", font: "700 var(--jr-text-lg)/1.25 var(--jr-font-ui)" }}>{title}</div>
      <div style={{ marginTop: 10, minHeight: 46, color: "var(--jr-text-secondary)",
        font: "400 var(--jr-text-sm)/1.45 var(--jr-font-ui)", textWrap: "pretty" }}>{message}</div>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: "var(--jr-space-5)", marginTop: 14 }}>
        {kind === "yesno" ? (
          <React.Fragment>
            <Button variant="primary" size="lg" style={{ width: 90 }} onClick={onYes}>Yes</Button>
            <Button size="lg" style={{ width: 90 }} onClick={onNo}>No</Button>
          </React.Fragment>
        ) : (
          <Button variant="primary" size="lg" style={{ width: 90 }} onClick={onOk}>Ok</Button>
        )}
      </div>
    </div>
  );
}
