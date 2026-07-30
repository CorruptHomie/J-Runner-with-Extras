import React from "react";

/* The MainForm RichTextBox: borderless (Theme drops FixedSingle for RichTextBox because the
   system border renders white), FieldBg well, per-line severity colours from
   CommunicationManager.MessageColor. */
const SEV = { ok: "var(--jr-log-ok)", info: "var(--jr-log-info)", warn: "var(--jr-log-warn)",
  error: "var(--jr-log-error)", plain: "var(--jr-text-primary)", muted: "var(--jr-text-secondary)" };

export function LogConsole({ lines = [], height = 220, style }) {
  return (
    <div style={{ height, overflowY: "auto", background: "var(--jr-field-bg)", border: "1px solid var(--jr-border)",
      padding: "var(--jr-space-4)", font: "400 var(--jr-text-sm)/var(--jr-console-leading) var(--jr-font-ui)",
      whiteSpace: "pre-wrap", wordBreak: "break-word", ...style }}>
      {lines.map((l, i) => {
        const text = typeof l === "string" ? l : l.text;
        const sev = typeof l === "string" ? "plain" : (l.severity || "plain");
        return <div key={i} style={{ color: SEV[sev] }}>{text}</div>;
      })}
    </div>
  );
}
