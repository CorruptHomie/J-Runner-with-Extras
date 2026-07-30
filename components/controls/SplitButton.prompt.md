Primary action plus a caret that opens alternates — the only place J-Runner nests actions inside a button.

```jsx
<SplitButton onClick={read} onOpen={() => setMenu(true)} open={menu}>Show Working Folder</SplitButton>
```

Keeps WinForms' flat rendering: 1px `--jr-border` all round, a 1px divider before the caret, hover fills each half independently.
