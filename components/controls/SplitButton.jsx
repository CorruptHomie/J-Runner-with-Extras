import React from "react";

/* Theme keeps WinForms' own flat rendering for SplitButton: RaisedBg fill, 1px Border,
   HoverBg / PressedBg fills, and a divider + caret drawn on the right. */
export function SplitButton({ children, onClick, onOpen, open, disabled, style, ...rest }) {
  const [hover, setHover] = React.useState(null);
  const base = { background: disabled ? "var(--jr-panel-bg)" : "var(--jr-raised-bg)", color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)" };
  return (
    <div style={{ display: "inline-flex", minHeight: 26, border: "1px solid var(--jr-border)", borderRadius: "var(--jr-radius-btn)",
      overflow: "hidden", font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)", ...base, ...style }} {...rest}>
      <button type="button" disabled={disabled} onClick={onClick}
        onMouseEnter={() => setHover("main")} onMouseLeave={() => setHover(null)}
        style={{ ...base, background: hover === "main" && !disabled ? "var(--jr-hover-bg)" : base.background,
          border: "none", padding: "0 12px", font: "inherit", color: "inherit", cursor: disabled ? "default" : "pointer" }}>
        {children}
      </button>
      <span style={{ width: 1, background: "var(--jr-border)" }} />
      <button type="button" disabled={disabled} onClick={onOpen} aria-label="More options"
        onMouseEnter={() => setHover("caret")} onMouseLeave={() => setHover(null)}
        style={{ ...base, background: (hover === "caret" || open) && !disabled ? "var(--jr-hover-bg)" : base.background,
          border: "none", width: 22, padding: 0, color: "inherit", cursor: disabled ? "default" : "pointer",
          font: "9px/1 var(--jr-font-ui)" }}>▼</button>
    </div>
  );
}
