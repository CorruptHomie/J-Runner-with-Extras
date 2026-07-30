import React from "react";

/* UI.FlashProgressOverlay: full-window sheet at 97% opacity on WindowBg. "Writing NAND" in
   15pt bold, the console mark up to 280px square rendered greyscale with a full-colour copy
   clipped to the bottom N% - colour literally rises as the write progresses - then the %
   beneath it and a muted warning line. */
export function FlashOverlay({ value = 0, max = 100, caption = "Writing NAND",
  hint = "Do not disconnect the flasher or close J-Runner.", logo = "assets/xbox-sphere.png", size = 280, style }) {
  const frac = Math.max(0, Math.min(1, value / max));
  return (
    <div style={{ position: "absolute", inset: 0, display: "flex", flexDirection: "column",
      alignItems: "center", justifyContent: "center", gap: 14,
      background: "var(--jr-window-bg)", opacity: "var(--jr-overlay-opacity)", ...style }}>
      <div style={{ color: "var(--jr-text-primary)", font: "700 var(--jr-text-xl)/1.2 var(--jr-font-ui)" }}>{caption}</div>
      <div style={{ position: "relative", width: size, height: size }}>
        <img src={logo} alt="" style={{ position: "absolute", inset: 0, width: "100%", height: "100%",
          objectFit: "contain", filter: "grayscale(1)" }} />
        <div style={{ position: "absolute", left: 0, right: 0, bottom: 0, height: `${frac * 100}%`, overflow: "hidden",
          transition: "height var(--jr-duration-fill) var(--jr-ease-fill)" }}>
          <img src={logo} alt="" style={{ position: "absolute", left: 0, bottom: 0, width: size, height: size, objectFit: "contain" }} />
        </div>
      </div>
      <div style={{ color: "var(--jr-text-secondary)", font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)" }}>{Math.round(frac * 100)}%</div>
      <div style={{ color: "var(--jr-text-secondary)", font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)" }}>{hint}</div>
    </div>
  );
}
