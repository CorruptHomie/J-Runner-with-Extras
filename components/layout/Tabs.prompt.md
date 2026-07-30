Tab strip for panel-level switching (XeBuild / XB Settings / Patches; Nand Info / KV Info / Bad Blocks).

```jsx
<Tabs tabs={["Nand Info","KV Info","Bad Blocks"]} value={tab} onChange={setTab}>…</Tabs>
```

The 2px accent underline is the only selection cue that uses colour — idle tabs differ only by text value.
