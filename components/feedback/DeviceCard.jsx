import React from "react";

/* The large panel right of the Nand column on MainForm. Shows the detected flasher's photo,
   or the muted "No flasher detected" line when nothing is connected. */
export function DeviceCard({ image, name, detail, empty = "No flasher detected", height = 118, style }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center",
      gap: "var(--jr-space-3)", height, background: "var(--jr-panel-bg)", border: "1px solid var(--jr-border-subtle)",
      borderRadius: "var(--jr-radius-group)", padding: "var(--jr-space-4)", textAlign: "center", ...style }}>
      {image ? (
        <React.Fragment>
          <img src={image} alt={name || ""} style={{ maxHeight: height - 52, maxWidth: "82%", objectFit: "contain" }} />
          <div style={{ color: "var(--jr-text-primary)", font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }}>{name}</div>
          {detail && <div style={{ color: "var(--jr-text-secondary)", font: "400 var(--jr-text-xs)/1.2 var(--jr-font-ui)" }}>{detail}</div>}
        </React.Fragment>
      ) : (
        <div style={{ color: "var(--jr-text-secondary)", font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)" }}>{empty}</div>
      )}
    </div>
  );
}
