import React from "react";

/* JRunnerToolStripRenderer: dropdown surface = PanelBg with a 1px Border rectangle;
   rows have no idle fill and round to 4px on hover (HoverBg) / press (PressedBg). */
export function MenuDropdown({ items = [], onSelect, style }) {
  const [hover, setHover] = React.useState(-1);
  return (
    <div style={{ minWidth: 180, background: "var(--jr-panel-bg)", border: "1px solid var(--jr-border)",
      padding: "var(--jr-space-1)", ...style }}>
      {items.map((it, i) => it === "-" ? (
        <div key={i} style={{ height: 1, background: "var(--jr-border)", margin: "3px 4px" }} />
      ) : (
        <div key={i} onMouseEnter={() => setHover(i)} onMouseLeave={() => setHover(-1)}
          onClick={() => onSelect && onSelect(it)}
          style={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 12,
            padding: "4px 10px", borderRadius: "var(--jr-radius-menu)",
            background: hover === i ? "var(--jr-hover-bg)" : "transparent",
            color: "var(--jr-text-primary)", font: "400 var(--jr-text-sm)/1.3 var(--jr-font-ui)", cursor: "pointer" }}>
          <span>{typeof it === "string" ? it : it.label}</span>
          {typeof it !== "string" && it.submenu && <span style={{ fontSize: 9, color: "var(--jr-text-primary)" }}>▶</span>}
        </div>
      ))}
    </div>
  );
}
