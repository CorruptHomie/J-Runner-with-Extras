import React from "react";

/* Theme.StyleButton + Theme.Button_Paint: flat fill, 1px hairline, radius = clamp(3, h/5, 8).
   Idle border is BorderSubtle and only brightens to Border on hover - that subtle swap is
   the whole hover affordance alongside the fill step. No transition: WinForms repaints. */
const FILL = { default: "var(--jr-raised-bg)", primary: "var(--jr-accent)", danger: "var(--jr-danger)" };
const HOVER = { default: "var(--jr-hover-bg)", primary: "var(--jr-accent-light)", danger: "#e0736a" };
const PRESS = { default: "var(--jr-pressed-bg)", primary: "var(--jr-accent-dim)", danger: "#b8483f" };

export function Button({ variant = "default", size = "md", disabled, block, icon, children, onClick, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  const [down, setDown] = React.useState(false);
  const h = size === "lg" ? 32 : size === "xl" ? 34 : size === "sm" ? 22 : 26;
  const radius = Math.max(3, Math.min(8, Math.round(h / 5)));
  const accented = variant !== "default";
  const bg = disabled ? "var(--jr-panel-bg)" : down ? PRESS[variant] : hover ? HOVER[variant] : FILL[variant];
  const border = disabled ? "var(--jr-border-subtle)" : accented ? bg : hover ? "var(--jr-border)" : "var(--jr-border-subtle)";
  const fg = disabled ? "var(--jr-text-disabled)" : accented ? "var(--jr-text-on-accent)" : "var(--jr-text-primary)";
  return (
    <button type="button" disabled={disabled} onClick={disabled ? undefined : onClick}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => { setHover(false); setDown(false); }}
      onMouseDown={() => setDown(true)} onMouseUp={() => setDown(false)}
      style={{ display: block ? "flex" : "inline-flex", width: block ? "100%" : undefined, alignItems: "center",
        justifyContent: "center", gap: "var(--jr-space-3)", minHeight: h, padding: `0 ${size === "sm" ? 8 : 12}px`,
        background: bg, color: fg, border: `1px solid ${border}`, borderRadius: radius,
        font: `${accented ? 700 : 400} var(--jr-text-sm)/1.2 var(--jr-font-ui)`, textAlign: "center",
        cursor: disabled ? "default" : "pointer", transition: "none", ...style }} {...rest}>
      {icon}{children}
    </button>
  );
}
