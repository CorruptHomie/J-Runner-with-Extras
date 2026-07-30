The session log. Everything J-Runner does narrates here first - treat it as the product's main output surface, not a debug panel.

```jsx
<LogConsole lines={[
  "J-Runner Premium",
  "Session: 07/30/2026 3:27:15",
  {text:"Status: Up to date", severity:"ok"},
  {text:"XeLL Customizer: Starting local server on port 2222...", severity:"muted"},
]} />
```

Severity colours are the raw console colours (`--jr-log-*`), not the UI accent - they are the one place saturated primaries appear.
