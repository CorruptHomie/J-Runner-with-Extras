Tabular records - CPU key database, bad blocks, logical drives. Set `mono` on any column holding keys, serials or hex.

```jsx
<DataTable columns={[{key:"serial",label:"Serial"},{key:"cpukey",label:"CPU Key",mono:true}]} rows={rows} />
```

Selection is `--jr-accent-dim`, deliberately not the bright accent - the theme overrides the OS highlight colour for exactly this reason.
