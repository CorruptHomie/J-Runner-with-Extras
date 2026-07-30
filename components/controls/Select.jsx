import React from "react";

/* ComboBox case: DrawMode.OwnerDrawFixed, FieldBg closed display, AccentDim on the
   highlighted row. FlatStyle.Flat, so the same square 1px border as a text field. */
export function Select({ label, labelWidth = 90, value, options = [], placeholder = "None Selected", onChange, disabled, width, style }) {
  const [open, setOpen] = React.useState(false);
  const [hoverIdx, setHoverIdx] = React.useState(-1);
  const field = (
    <div style={{ position: "relative", flex: width ? "none" : 1, width }}>
      <button type="button" disabled={disabled} onClick={() => setOpen(!open)}
        style={{ width: "100%", minHeight: "var(--jr-control-h)", display: "flex", alignItems: "center",
          justifyContent: "space-between", gap: 6, background: disabled ? "var(--jr-panel-bg)" : "var(--jr-field-bg)",
          color: value ? "var(--jr-text-primary)" : "var(--jr-text-secondary)", border: "1px solid var(--jr-border)",
          borderRadius: 0, padding: "0 var(--jr-space-2) 0 var(--jr-space-3)",
          font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)", cursor: disabled ? "default" : "pointer", ...style }}>
        <span style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{value || placeholder}</span>
        <span style={{ fontSize: 9, color: "var(--jr-text-secondary)" }}>▼</span>
      </button>
      {open && (
        <div style={{ position: "absolute", zIndex: 20, top: "100%", left: 0, right: 0, background: "var(--jr-field-bg)",
          border: "1px solid var(--jr-border)", maxHeight: 180, overflowY: "auto" }}>
          {options.map((o, i) => (
            <div key={o} onMouseEnter={() => setHoverIdx(i)} onMouseLeave={() => setHoverIdx(-1)}
              onClick={() => { onChange && onChange(o); setOpen(false); }}
              style={{ padding: "3px var(--jr-space-3)", background: hoverIdx === i ? "var(--jr-accent-dim)" : "transparent",
                color: "var(--jr-text-primary)", font: "400 var(--jr-text-sm)/1.4 var(--jr-font-ui)", cursor: "pointer" }}>{o}</div>
          ))}
        </div>
      )}
    </div>
  );
  if (!label) return field;
  return (
    <label style={{ display: "flex", alignItems: "center", gap: "var(--jr-space-4)" }}>
      <span style={{ width: labelWidth, flex: "none", textAlign: "right", color: "var(--jr-text-primary)",
        font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }}>{label}</span>
      {field}
    </label>
  );
}
