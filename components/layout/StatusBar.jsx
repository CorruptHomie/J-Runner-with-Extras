import React from "react";

/* StatusStrip case in Theme.StyleControl: PanelBg with TextPrimary items. MainForm shows
   the bundled tool versions here ("XeBuild: 1.21   Dashlaunch: 3.21"). */
export function StatusBar({ items = [], style }) {
  return (
    <div style={{ display: "flex", alignItems: "center", gap: "var(--jr-space-9)", height: "var(--jr-statusbar-h)",
      padding: "0 var(--jr-space-6)", background: "var(--jr-panel-bg)", color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-xs)/1 var(--jr-font-ui)", ...style }}>
      {items.map((it, i) => (
        <span key={i}>
          <span style={{ color: "var(--jr-text-secondary)" }}>{it.label}: </span>{it.value}
        </span>
      ))}
    </div>
  );
}
