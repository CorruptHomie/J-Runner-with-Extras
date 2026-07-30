import React from "react";

/* DarkTabControl.DrawTab: selected tab = RaisedBg + a 2px accent bar along its bottom edge;
   idle = PanelBg with a 1px BorderSubtle separator on its right. Page area gets a single
   hairline rectangle, never a 3D frame. */
export function Tabs({ tabs = [], value, onChange, children, style, bodyStyle }) {
  return (
    <div style={{ background: "var(--jr-panel-bg)", ...style }}>
      <div style={{ display: "flex" }}>
        {tabs.map((t, i) => {
          const on = t === value;
          return (
            <button key={t} type="button" onClick={() => onChange && onChange(t)}
              style={{ position: "relative", padding: "5px 14px 6px", border: "none",
                borderRight: on ? "none" : "1px solid var(--jr-border-subtle)",
                background: on ? "var(--jr-raised-bg)" : "var(--jr-panel-bg)",
                color: on ? "var(--jr-text-primary)" : "var(--jr-text-secondary)",
                font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)", cursor: "pointer",
                boxShadow: on ? "inset 0 -2px 0 0 var(--jr-accent)" : "none" }}>{t}</button>
          );
        })}
      </div>
      <div style={{ border: "1px solid var(--jr-border-subtle)", padding: "var(--jr-space-6)", ...bodyStyle }}>{children}</div>
    </div>
  );
}
