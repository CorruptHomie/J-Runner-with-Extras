Text input well — dark `--jr-field-bg` recessed against the panel, square corners, 1px `--jr-border`.

```jsx
<TextField label="CPU Key:" mono value={cpuKey} onChange={setCpuKey} />
<TextField placeholder="Load Source" readOnly />
```

Fields are the only surface darker than the window itself — that value drop is what reads as "editable". Use `mono` for keys and hex.
