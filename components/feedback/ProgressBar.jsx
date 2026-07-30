import React from "react";

/* XboxFillProgressBar.DrawBarMode: 20px track on #121214, radius = clamp(2, h/2, 8),
   accent fill clipped to the rounded path, 1px Border on top, centred % text in
   TextPrimary. Marquee mode reads "Working..." with the track filled. */
export function ProgressBar({ value = 0, max = 100, marquee, height = 20, showText = true, style }) {
  const frac = marquee ? 1 : Math.max(0, Math.min(1, value / max));
  const radius = Math.max(2, Math.min(8, Math.round(height / 2)));
  return (
    <div style={{ position: "relative", height, borderRadius: radius, background: "var(--jr-track-bg)",
      border: "1px solid var(--jr-border)", overflow: "hidden", ...style }}>
      <div style={{ position: "absolute", inset: 0, width: `${frac * 100}%`, background: "var(--jr-accent)",
        transition: "width var(--jr-duration-fill) var(--jr-ease-fill)" }} />
      {showText && (
        <div style={{ position: "relative", height: "100%", display: "flex", alignItems: "center",
          justifyContent: "center", color: "var(--jr-text-primary)", font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)" }}>
          {marquee ? "Working..." : `${Math.round(frac * 100)}%`}
        </div>
      )}
    </div>
  );
}
