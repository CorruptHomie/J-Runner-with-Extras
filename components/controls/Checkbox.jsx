import React from "react";

/* Theme.CheckBox_Paint: 13px box, 3px radius, accent fill when checked, tick stroked in
   #141812 at 2px with round caps. Unchecked = FieldBg + Border. */
export function Checkbox({ checked, onChange, disabled, label, style }) {
  const on = checked && !disabled;
  return (
    <label style={{ display: "inline-flex", alignItems: "center", gap: "var(--jr-space-3)",
      cursor: disabled ? "default" : "pointer", ...style }}
      onClick={() => !disabled && onChange && onChange(!checked)}>
      <span style={{ width: 13, height: 13, flex: "none", borderRadius: "var(--jr-radius-glyph)",
        background: on ? "var(--jr-accent)" : "var(--jr-field-bg)",
        border: `1px solid ${on ? "var(--jr-accent)" : "var(--jr-border)"}`,
        display: "flex", alignItems: "center", justifyContent: "center" }}>
        {checked && (
          <svg width="11" height="11" viewBox="0 0 13 13" aria-hidden="true">
            <polyline points="3,6 5,9 10,4" fill="none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"
              stroke={disabled ? "var(--jr-text-disabled)" : "var(--jr-tick)"} />
          </svg>
        )}
      </span>
      {label && <span style={{ color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
        font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }}>{label}</span>}
    </label>
  );
}
