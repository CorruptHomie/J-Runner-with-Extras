Window chrome for any J-Runner surface. There is deliberately no maximize button — every window is a fixed size.

```jsx
<TitleBar logo="assets/logo-jr.png" menu={["Tools","Nand","Advanced"]} right="V4.0.0devpre4" onClose={quit} />
```

Menu items round to 10px on hover and pick up a 2px accent underline; close hovers to `--jr-danger` with a white glyph.
