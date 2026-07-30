import React from "react";

/* Theme.RadioButton_Paint: 13px circle, FieldBg fill, border turns Accent when checked,
   inner dot is the accent inflated by -4 on each side (5px). */
export function Radio({ checked, onChange, disabled, label, name, style }) {
  return (
    <label style={{ display: "inline-flex", alignItems: "center", gap: "var(--jr-space-3)",
      cursor: disabled ? "default" : "pointer", ...style }}
      onClick={() => !disabled && onChange && onChange(true)}>
      <span data-name={name} style={{ width: 13, height: 13, flex: "none", borderRadius: "50%",
        background: disabled ? "var(--jr-panel-bg)" : "var(--jr-field-bg)",
        border: `1px solid ${checked ? "var(--jr-accent)" : "var(--jr-border)"}`,
        display: "flex", alignItems: "center", justifyContent: "center" }}>
        {checked && <span style={{ width: 5, height: 5, borderRadius: "50%",
          background: disabled ? "var(--jr-text-disabled)" : "var(--jr-accent)" }} />}
      </span>
      {label && <span style={{ color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
        font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }}>{label}</span>}
    </label>
  );
}
