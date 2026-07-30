Confirm or acknowledge. Only two exist in the product, and both guard a flash - do not spawn dialogs for routine actions.

```jsx
<MessageDialog kind="yesno" title="Confirm Flash"
  message="Are you sure? Did you make sure to select the correct options and patches?" />
<MessageDialog title="Flash Complete"
  message="Remember to disconnect the flasher from the computer before booting!" />
```

The dialog has no title bar; drag comes from the title/body text. Dim the owner window behind it with `--jr-scrim`.
