import React from "react";

/* MainForm.Chrome: 36px bar on WindowBg, the menu strip inlined into it, a 1px
   BorderSubtle hairline underneath, and 46px-wide chrome buttons on the right.
   No maximize - the layout is absolutely positioned and does not reflow. */
export function TitleBar({ menu = [], activeMenu, onMenu, right, onMinimize, onClose, logo, style }) {
  const [hover, setHover] = React.useState(null);
  return (
    <div style={{ position: "relative", display: "flex", alignItems: "stretch", height: "var(--jr-titlebar-h)",
      background: "var(--jr-window-bg)", borderBottom: "1px solid var(--jr-border-subtle)",
      font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)", userSelect: "none", ...style }}>
      <div style={{ display: "flex", alignItems: "center", gap: 2, paddingLeft: 4 }}>
        {logo && <img src={logo} alt="" style={{ width: 16, height: 16, margin: "0 6px 0 4px", objectFit: "contain" }} />}
        {menu.map((m) => {
          const on = activeMenu === m || hover === m;
          return (
            <button key={m} type="button" onClick={() => onMenu && onMenu(m)}
              onMouseEnter={() => setHover(m)} onMouseLeave={() => setHover(null)}
              style={{ position: "relative", height: 24, padding: "0 10px", border: "none", borderRadius: 10,
                background: on ? "var(--jr-hover-bg)" : "transparent", color: "var(--jr-text-primary)",
                font: "inherit", cursor: "pointer",
                boxShadow: on ? "inset 0 -2px 0 0 var(--jr-accent)" : "none" }}>{m}</button>
          );
        })}
      </div>
      <div style={{ flex: 1 }} />
      {right && <div style={{ display: "flex", alignItems: "center", color: "var(--jr-text-secondary)", paddingRight: 8 }}>{right}</div>}
      <ChromeButton kind="min" onClick={onMinimize} />
      <ChromeButton kind="close" onClick={onClose} />
    </div>
  );
}

function ChromeButton({ kind, onClick }) {
  const [h, setH] = React.useState(false);
  const close = kind === "close";
  return (
    <button type="button" onClick={onClick} aria-label={close ? "Close" : "Minimize"}
      onMouseEnter={() => setH(true)} onMouseLeave={() => setH(false)}
      style={{ width: "var(--jr-chrome-btn-w)", border: "none", cursor: "pointer",
        background: h ? (close ? "var(--jr-danger)" : "var(--jr-hover-bg)") : "transparent",
        color: h ? (close ? "#fff" : "var(--jr-chrome-glyph-hover)") : "var(--jr-chrome-glyph)",
        display: "flex", alignItems: "center", justifyContent: "center" }}>
      <svg width="11" height="11" viewBox="0 0 11 11" aria-hidden="true" stroke="currentColor" strokeWidth="1.3" fill="none">
        {close ? <g><line x1="0.5" y1="0.5" x2="10.5" y2="10.5" /><line x1="10.5" y1="0.5" x2="0.5" y2="10.5" /></g>
               : <line x1="0.5" y1="5.5" x2="10.5" y2="5.5" />}
      </svg>
    </button>
  );
}
