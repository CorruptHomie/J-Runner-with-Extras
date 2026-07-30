import React from "react";

/* TextBoxBase case in Theme.StyleControl: FieldBg, TextPrimary, BorderStyle.FixedSingle
   (a 1px square border - fields are NOT rounded). RichTextBox goes borderless instead. */
export function TextField({ label, labelWidth = 90, value, placeholder, readOnly, disabled, mono, onChange, width, style, ...rest }) {
  const input = (
    <input value={value} placeholder={placeholder} readOnly={readOnly} disabled={disabled}
      onChange={onChange ? (e) => onChange(e.target.value) : undefined}
      style={{ flex: width ? "none" : 1, width, minHeight: "var(--jr-control-h)", boxSizing: "border-box",
        background: disabled ? "var(--jr-panel-bg)" : "var(--jr-field-bg)",
        color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
        border: "1px solid var(--jr-border)", borderRadius: 0, padding: "0 var(--jr-space-3)",
        font: `400 var(--jr-text-sm)/1.2 ${mono ? "var(--jr-font-mono)" : "var(--jr-font-ui)"}`, outline: "none", ...style }} {...rest} />
  );
  if (!label) return input;
  return (
    <label style={{ display: "flex", alignItems: "center", gap: "var(--jr-space-4)" }}>
      <span style={{ width: labelWidth, flex: "none", textAlign: "right", color: "var(--jr-text-primary)",
        font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }}>{label}</span>
      {input}
    </label>
  );
}
