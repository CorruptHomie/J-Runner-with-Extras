import React from "react";

/* NumericUpDown case: FieldBg, FixedSingle border, spin buttons stacked at the right. */
export function NumberField({ value = 0, min = 0, max = 99, onChange, width = 44, disabled, style }) {
  const step = (d) => { if (disabled || !onChange) return; onChange(Math.max(min, Math.min(max, value + d))); };
  const spin = { flex: 1, display: "flex", alignItems: "center", justifyContent: "center", width: 15,
    background: "var(--jr-raised-bg)", color: "var(--jr-text-primary)", border: "none", borderLeft: "1px solid var(--jr-border)",
    fontSize: 7, lineHeight: 1, cursor: disabled ? "default" : "pointer" };
  return (
    <div style={{ display: "inline-flex", width, minHeight: "var(--jr-control-h)", background: "var(--jr-field-bg)",
      border: "1px solid var(--jr-border)", ...style }}>
      <input value={value} readOnly disabled={disabled}
        style={{ flex: 1, minWidth: 0, background: "transparent", color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
          border: "none", outline: "none", padding: "0 var(--jr-space-2)", font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }} />
      <span style={{ display: "flex", flexDirection: "column" }}>
        <button type="button" onClick={() => step(1)} style={{ ...spin, borderBottom: "1px solid var(--jr-border)" }}>▲</button>
        <button type="button" onClick={() => step(-1)} style={spin}>▼</button>
      </span>
    </div>
  );
}
