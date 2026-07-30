import React from "react";

/* DataGridView / ListView theming: RaisedBg headers with a BorderSubtle rule, FieldBg rows
   zebra-striped against #1a1a1e, selection in AccentDim (never the OS highlight). */
export function DataTable({ columns = [], rows = [], selectedIndex = -1, onSelect, style }) {
  const cols = columns.map((c) => (typeof c === "string" ? { key: c, label: c } : c));
  return (
    <div style={{ background: "var(--jr-panel-bg)", border: "1px solid var(--jr-border-subtle)", ...style }}>
      <table style={{ width: "100%", borderCollapse: "collapse",
        font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)", color: "var(--jr-text-primary)" }}>
        <thead>
          <tr>{cols.map((c) => (
            <th key={c.key} style={{ textAlign: "left", padding: "5px 6px", background: "var(--jr-raised-bg)",
              borderBottom: "1px solid var(--jr-border-subtle)", borderRight: "1px solid var(--jr-border-subtle)",
              font: "inherit", fontWeight: 400, whiteSpace: "nowrap" }}>{c.label || c.key}</th>
          ))}</tr>
        </thead>
        <tbody>
          {rows.map((r, i) => (
            <tr key={i} onClick={() => onSelect && onSelect(i)}
              style={{ background: selectedIndex === i ? "var(--jr-accent-dim)"
                : i % 2 ? "var(--jr-row-alt-bg)" : "var(--jr-field-bg)", cursor: onSelect ? "pointer" : "default" }}>
              {cols.map((c) => (
                <td key={c.key} style={{ padding: "4px 6px", borderRight: "1px solid var(--jr-border-subtle)",
                  fontFamily: c.mono ? "var(--jr-font-mono)" : "inherit", whiteSpace: "nowrap" }}>
                  {r[c.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
