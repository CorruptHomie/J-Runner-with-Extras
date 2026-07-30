Flat dark action button — use for every clickable action in a J-Runner window; `variant="primary"` (accent green, black label, bold) is reserved for the single confirming action in a dialog.

```jsx
<Button>Read Nand</Button>
<Button variant="primary" size="lg">Yes</Button>
<Button variant="default" disabled>Write Nand</Button>
```

Variants: `default` (RaisedBg + subtle hairline), `primary` (accent), `danger`. Sizes map to real designer heights: sm 22 / md 26 / lg 32 / xl 34. Radius is derived, not fixed: `clamp(3, height/5, 8)`. Never add a transition — the app repaints instantly.
