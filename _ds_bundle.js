/* @ds-bundle: {"format":4,"namespace":"JRunnerPremiumDesignSystem_5a2f40","components":[{"name":"Button","sourcePath":"components/controls/Button.jsx"},{"name":"Checkbox","sourcePath":"components/controls/Checkbox.jsx"},{"name":"NumberField","sourcePath":"components/controls/NumberField.jsx"},{"name":"Radio","sourcePath":"components/controls/Radio.jsx"},{"name":"Select","sourcePath":"components/controls/Select.jsx"},{"name":"SplitButton","sourcePath":"components/controls/SplitButton.jsx"},{"name":"TextField","sourcePath":"components/controls/TextField.jsx"},{"name":"DataTable","sourcePath":"components/data/DataTable.jsx"},{"name":"DeviceCard","sourcePath":"components/feedback/DeviceCard.jsx"},{"name":"FlashOverlay","sourcePath":"components/feedback/FlashOverlay.jsx"},{"name":"LogConsole","sourcePath":"components/feedback/LogConsole.jsx"},{"name":"MessageDialog","sourcePath":"components/feedback/MessageDialog.jsx"},{"name":"ProgressBar","sourcePath":"components/feedback/ProgressBar.jsx"},{"name":"GroupBox","sourcePath":"components/layout/GroupBox.jsx"},{"name":"MenuDropdown","sourcePath":"components/layout/MenuDropdown.jsx"},{"name":"StatusBar","sourcePath":"components/layout/StatusBar.jsx"},{"name":"Tabs","sourcePath":"components/layout/Tabs.jsx"},{"name":"TitleBar","sourcePath":"components/layout/TitleBar.jsx"}],"sourceHashes":{"components/controls/Button.jsx":"3c8af34f522a","components/controls/Checkbox.jsx":"87cb320ebb03","components/controls/NumberField.jsx":"7da26bfab0e3","components/controls/Radio.jsx":"51748b3beae9","components/controls/Select.jsx":"f441b205cdec","components/controls/SplitButton.jsx":"0ecc756bb5b4","components/controls/TextField.jsx":"cd20a791b0bd","components/data/DataTable.jsx":"c65f667d5765","components/feedback/DeviceCard.jsx":"77bf9878476b","components/feedback/FlashOverlay.jsx":"3e8a670beafe","components/feedback/LogConsole.jsx":"f21ab6dcd04f","components/feedback/MessageDialog.jsx":"77d2513b40ab","components/feedback/ProgressBar.jsx":"ed80eb8df2fe","components/layout/GroupBox.jsx":"4dc87dcc8a0a","components/layout/MenuDropdown.jsx":"a296461be78a","components/layout/StatusBar.jsx":"df920bc3ee78","components/layout/Tabs.jsx":"9527720964fd","components/layout/TitleBar.jsx":"097e4fdc6967","motion.js":"8f24cb975e6e","motion2.js":"0e43424acd9f","site.js":"e0101536d13e","tweaks.js":"09f5af360c01","ui_kits/jrunner-app/MainWindow.jsx":"00a26d942466","ui_kits/jrunner-app/dialogs.jsx":"ecf1444ec0b8","ui_kits/jrunner-app/panels.jsx":"e8e35345bf3f","ui_kits/xell-customizer/XellCustomizer.jsx":"7e0554658c4b","xfall.js":"5efc6fc47874"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.JRunnerPremiumDesignSystem_5a2f40 = window.JRunnerPremiumDesignSystem_5a2f40 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/controls/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Theme.StyleButton + Theme.Button_Paint: flat fill, 1px hairline, radius = clamp(3, h/5, 8).
   Idle border is BorderSubtle and only brightens to Border on hover - that subtle swap is
   the whole hover affordance alongside the fill step. No transition: WinForms repaints. */
const FILL = {
  default: "var(--jr-raised-bg)",
  primary: "var(--jr-accent)",
  danger: "var(--jr-danger)"
};
const HOVER = {
  default: "var(--jr-hover-bg)",
  primary: "var(--jr-accent-light)",
  danger: "#e0736a"
};
const PRESS = {
  default: "var(--jr-pressed-bg)",
  primary: "var(--jr-accent-dim)",
  danger: "#b8483f"
};
function Button({
  variant = "default",
  size = "md",
  disabled,
  block,
  icon,
  children,
  onClick,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const [down, setDown] = React.useState(false);
  const h = size === "lg" ? 32 : size === "xl" ? 34 : size === "sm" ? 22 : 26;
  const radius = Math.max(3, Math.min(8, Math.round(h / 5)));
  const accented = variant !== "default";
  const bg = disabled ? "var(--jr-panel-bg)" : down ? PRESS[variant] : hover ? HOVER[variant] : FILL[variant];
  const border = disabled ? "var(--jr-border-subtle)" : accented ? bg : hover ? "var(--jr-border)" : "var(--jr-border-subtle)";
  const fg = disabled ? "var(--jr-text-disabled)" : accented ? "var(--jr-text-on-accent)" : "var(--jr-text-primary)";
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    disabled: disabled,
    onClick: disabled ? undefined : onClick,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => {
      setHover(false);
      setDown(false);
    },
    onMouseDown: () => setDown(true),
    onMouseUp: () => setDown(false),
    style: {
      display: block ? "flex" : "inline-flex",
      width: block ? "100%" : undefined,
      alignItems: "center",
      justifyContent: "center",
      gap: "var(--jr-space-3)",
      minHeight: h,
      padding: `0 ${size === "sm" ? 8 : 12}px`,
      background: bg,
      color: fg,
      border: `1px solid ${border}`,
      borderRadius: radius,
      font: `${accented ? 700 : 400} var(--jr-text-sm)/1.2 var(--jr-font-ui)`,
      textAlign: "center",
      cursor: disabled ? "default" : "pointer",
      transition: "none",
      ...style
    }
  }, rest), icon, children);
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/Button.jsx", error: String((e && e.message) || e) }); }

// components/controls/Checkbox.jsx
try { (() => {
/* Theme.CheckBox_Paint: 13px box, 3px radius, accent fill when checked, tick stroked in
   #141812 at 2px with round caps. Unchecked = FieldBg + Border. */
function Checkbox({
  checked,
  onChange,
  disabled,
  label,
  style
}) {
  const on = checked && !disabled;
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: "var(--jr-space-3)",
      cursor: disabled ? "default" : "pointer",
      ...style
    },
    onClick: () => !disabled && onChange && onChange(!checked)
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 13,
      height: 13,
      flex: "none",
      borderRadius: "var(--jr-radius-glyph)",
      background: on ? "var(--jr-accent)" : "var(--jr-field-bg)",
      border: `1px solid ${on ? "var(--jr-accent)" : "var(--jr-border)"}`,
      display: "flex",
      alignItems: "center",
      justifyContent: "center"
    }
  }, checked && /*#__PURE__*/React.createElement("svg", {
    width: "11",
    height: "11",
    viewBox: "0 0 13 13",
    "aria-hidden": "true"
  }, /*#__PURE__*/React.createElement("polyline", {
    points: "3,6 5,9 10,4",
    fill: "none",
    strokeWidth: "2",
    strokeLinecap: "round",
    strokeLinejoin: "round",
    stroke: disabled ? "var(--jr-text-disabled)" : "var(--jr-tick)"
  }))), label && /*#__PURE__*/React.createElement("span", {
    style: {
      color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }, label));
}
Object.assign(__ds_scope, { Checkbox });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/Checkbox.jsx", error: String((e && e.message) || e) }); }

// components/controls/NumberField.jsx
try { (() => {
/* NumericUpDown case: FieldBg, FixedSingle border, spin buttons stacked at the right. */
function NumberField({
  value = 0,
  min = 0,
  max = 99,
  onChange,
  width = 44,
  disabled,
  style
}) {
  const step = d => {
    if (disabled || !onChange) return;
    onChange(Math.max(min, Math.min(max, value + d)));
  };
  const spin = {
    flex: 1,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    width: 15,
    background: "var(--jr-raised-bg)",
    color: "var(--jr-text-primary)",
    border: "none",
    borderLeft: "1px solid var(--jr-border)",
    fontSize: 7,
    lineHeight: 1,
    cursor: disabled ? "default" : "pointer"
  };
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "inline-flex",
      width,
      minHeight: "var(--jr-control-h)",
      background: "var(--jr-field-bg)",
      border: "1px solid var(--jr-border)",
      ...style
    }
  }, /*#__PURE__*/React.createElement("input", {
    value: value,
    readOnly: true,
    disabled: disabled,
    style: {
      flex: 1,
      minWidth: 0,
      background: "transparent",
      color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
      border: "none",
      outline: "none",
      padding: "0 var(--jr-space-2)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      display: "flex",
      flexDirection: "column"
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => step(1),
    style: {
      ...spin,
      borderBottom: "1px solid var(--jr-border)"
    }
  }, "\u25B2"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => step(-1),
    style: spin
  }, "\u25BC")));
}
Object.assign(__ds_scope, { NumberField });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/NumberField.jsx", error: String((e && e.message) || e) }); }

// components/controls/Radio.jsx
try { (() => {
/* Theme.RadioButton_Paint: 13px circle, FieldBg fill, border turns Accent when checked,
   inner dot is the accent inflated by -4 on each side (5px). */
function Radio({
  checked,
  onChange,
  disabled,
  label,
  name,
  style
}) {
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: "var(--jr-space-3)",
      cursor: disabled ? "default" : "pointer",
      ...style
    },
    onClick: () => !disabled && onChange && onChange(true)
  }, /*#__PURE__*/React.createElement("span", {
    "data-name": name,
    style: {
      width: 13,
      height: 13,
      flex: "none",
      borderRadius: "50%",
      background: disabled ? "var(--jr-panel-bg)" : "var(--jr-field-bg)",
      border: `1px solid ${checked ? "var(--jr-accent)" : "var(--jr-border)"}`,
      display: "flex",
      alignItems: "center",
      justifyContent: "center"
    }
  }, checked && /*#__PURE__*/React.createElement("span", {
    style: {
      width: 5,
      height: 5,
      borderRadius: "50%",
      background: disabled ? "var(--jr-text-disabled)" : "var(--jr-accent)"
    }
  })), label && /*#__PURE__*/React.createElement("span", {
    style: {
      color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }, label));
}
Object.assign(__ds_scope, { Radio });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/Radio.jsx", error: String((e && e.message) || e) }); }

// components/controls/Select.jsx
try { (() => {
/* ComboBox case: DrawMode.OwnerDrawFixed, FieldBg closed display, AccentDim on the
   highlighted row. FlatStyle.Flat, so the same square 1px border as a text field. */
function Select({
  label,
  labelWidth = 90,
  value,
  options = [],
  placeholder = "None Selected",
  onChange,
  disabled,
  width,
  style
}) {
  const [open, setOpen] = React.useState(false);
  const [hoverIdx, setHoverIdx] = React.useState(-1);
  const field = /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      flex: width ? "none" : 1,
      width
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    disabled: disabled,
    onClick: () => setOpen(!open),
    style: {
      width: "100%",
      minHeight: "var(--jr-control-h)",
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      gap: 6,
      background: disabled ? "var(--jr-panel-bg)" : "var(--jr-field-bg)",
      color: value ? "var(--jr-text-primary)" : "var(--jr-text-secondary)",
      border: "1px solid var(--jr-border)",
      borderRadius: 0,
      padding: "0 var(--jr-space-2) 0 var(--jr-space-3)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)",
      cursor: disabled ? "default" : "pointer",
      ...style
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap"
    }
  }, value || placeholder), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 9,
      color: "var(--jr-text-secondary)"
    }
  }, "\u25BC")), open && /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      zIndex: 20,
      top: "100%",
      left: 0,
      right: 0,
      background: "var(--jr-field-bg)",
      border: "1px solid var(--jr-border)",
      maxHeight: 180,
      overflowY: "auto"
    }
  }, options.map((o, i) => /*#__PURE__*/React.createElement("div", {
    key: o,
    onMouseEnter: () => setHoverIdx(i),
    onMouseLeave: () => setHoverIdx(-1),
    onClick: () => {
      onChange && onChange(o);
      setOpen(false);
    },
    style: {
      padding: "3px var(--jr-space-3)",
      background: hoverIdx === i ? "var(--jr-accent-dim)" : "transparent",
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.4 var(--jr-font-ui)",
      cursor: "pointer"
    }
  }, o))));
  if (!label) return field;
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "var(--jr-space-4)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: labelWidth,
      flex: "none",
      textAlign: "right",
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }, label), field);
}
Object.assign(__ds_scope, { Select });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/Select.jsx", error: String((e && e.message) || e) }); }

// components/controls/SplitButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Theme keeps WinForms' own flat rendering for SplitButton: RaisedBg fill, 1px Border,
   HoverBg / PressedBg fills, and a divider + caret drawn on the right. */
function SplitButton({
  children,
  onClick,
  onOpen,
  open,
  disabled,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(null);
  const base = {
    background: disabled ? "var(--jr-panel-bg)" : "var(--jr-raised-bg)",
    color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)"
  };
  return /*#__PURE__*/React.createElement("div", _extends({
    style: {
      display: "inline-flex",
      minHeight: 26,
      border: "1px solid var(--jr-border)",
      borderRadius: "var(--jr-radius-btn)",
      overflow: "hidden",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)",
      ...base,
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("button", {
    type: "button",
    disabled: disabled,
    onClick: onClick,
    onMouseEnter: () => setHover("main"),
    onMouseLeave: () => setHover(null),
    style: {
      ...base,
      background: hover === "main" && !disabled ? "var(--jr-hover-bg)" : base.background,
      border: "none",
      padding: "0 12px",
      font: "inherit",
      color: "inherit",
      cursor: disabled ? "default" : "pointer"
    }
  }, children), /*#__PURE__*/React.createElement("span", {
    style: {
      width: 1,
      background: "var(--jr-border)"
    }
  }), /*#__PURE__*/React.createElement("button", {
    type: "button",
    disabled: disabled,
    onClick: onOpen,
    "aria-label": "More options",
    onMouseEnter: () => setHover("caret"),
    onMouseLeave: () => setHover(null),
    style: {
      ...base,
      background: (hover === "caret" || open) && !disabled ? "var(--jr-hover-bg)" : base.background,
      border: "none",
      width: 22,
      padding: 0,
      color: "inherit",
      cursor: disabled ? "default" : "pointer",
      font: "9px/1 var(--jr-font-ui)"
    }
  }, "\u25BC"));
}
Object.assign(__ds_scope, { SplitButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/SplitButton.jsx", error: String((e && e.message) || e) }); }

// components/controls/TextField.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* TextBoxBase case in Theme.StyleControl: FieldBg, TextPrimary, BorderStyle.FixedSingle
   (a 1px square border - fields are NOT rounded). RichTextBox goes borderless instead. */
function TextField({
  label,
  labelWidth = 90,
  value,
  placeholder,
  readOnly,
  disabled,
  mono,
  onChange,
  width,
  style,
  ...rest
}) {
  const input = /*#__PURE__*/React.createElement("input", _extends({
    value: value,
    placeholder: placeholder,
    readOnly: readOnly,
    disabled: disabled,
    onChange: onChange ? e => onChange(e.target.value) : undefined,
    style: {
      flex: width ? "none" : 1,
      width,
      minHeight: "var(--jr-control-h)",
      boxSizing: "border-box",
      background: disabled ? "var(--jr-panel-bg)" : "var(--jr-field-bg)",
      color: disabled ? "var(--jr-text-disabled)" : "var(--jr-text-primary)",
      border: "1px solid var(--jr-border)",
      borderRadius: 0,
      padding: "0 var(--jr-space-3)",
      font: `400 var(--jr-text-sm)/1.2 ${mono ? "var(--jr-font-mono)" : "var(--jr-font-ui)"}`,
      outline: "none",
      ...style
    }
  }, rest));
  if (!label) return input;
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "var(--jr-space-4)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: labelWidth,
      flex: "none",
      textAlign: "right",
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }, label), input);
}
Object.assign(__ds_scope, { TextField });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/controls/TextField.jsx", error: String((e && e.message) || e) }); }

// components/data/DataTable.jsx
try { (() => {
/* DataGridView / ListView theming: RaisedBg headers with a BorderSubtle rule, FieldBg rows
   zebra-striped against #1a1a1e, selection in AccentDim (never the OS highlight). */
function DataTable({
  columns = [],
  rows = [],
  selectedIndex = -1,
  onSelect,
  style
}) {
  const cols = columns.map(c => typeof c === "string" ? {
    key: c,
    label: c
  } : c);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--jr-panel-bg)",
      border: "1px solid var(--jr-border-subtle)",
      ...style
    }
  }, /*#__PURE__*/React.createElement("table", {
    style: {
      width: "100%",
      borderCollapse: "collapse",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)",
      color: "var(--jr-text-primary)"
    }
  }, /*#__PURE__*/React.createElement("thead", null, /*#__PURE__*/React.createElement("tr", null, cols.map(c => /*#__PURE__*/React.createElement("th", {
    key: c.key,
    style: {
      textAlign: "left",
      padding: "5px 6px",
      background: "var(--jr-raised-bg)",
      borderBottom: "1px solid var(--jr-border-subtle)",
      borderRight: "1px solid var(--jr-border-subtle)",
      font: "inherit",
      fontWeight: 400,
      whiteSpace: "nowrap"
    }
  }, c.label || c.key)))), /*#__PURE__*/React.createElement("tbody", null, rows.map((r, i) => /*#__PURE__*/React.createElement("tr", {
    key: i,
    onClick: () => onSelect && onSelect(i),
    style: {
      background: selectedIndex === i ? "var(--jr-accent-dim)" : i % 2 ? "var(--jr-row-alt-bg)" : "var(--jr-field-bg)",
      cursor: onSelect ? "pointer" : "default"
    }
  }, cols.map(c => /*#__PURE__*/React.createElement("td", {
    key: c.key,
    style: {
      padding: "4px 6px",
      borderRight: "1px solid var(--jr-border-subtle)",
      fontFamily: c.mono ? "var(--jr-font-mono)" : "inherit",
      whiteSpace: "nowrap"
    }
  }, r[c.key])))))));
}
Object.assign(__ds_scope, { DataTable });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/DataTable.jsx", error: String((e && e.message) || e) }); }

// components/feedback/DeviceCard.jsx
try { (() => {
/* The large panel right of the Nand column on MainForm. Shows the detected flasher's photo,
   or the muted "No flasher detected" line when nothing is connected. */
function DeviceCard({
  image,
  name,
  detail,
  empty = "No flasher detected",
  height = 118,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      gap: "var(--jr-space-3)",
      height,
      background: "var(--jr-panel-bg)",
      border: "1px solid var(--jr-border-subtle)",
      borderRadius: "var(--jr-radius-group)",
      padding: "var(--jr-space-4)",
      textAlign: "center",
      ...style
    }
  }, image ? /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement("img", {
    src: image,
    alt: name || "",
    style: {
      maxHeight: height - 52,
      maxWidth: "82%",
      objectFit: "contain"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }, name), detail && /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-xs)/1.2 var(--jr-font-ui)"
    }
  }, detail)) : /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)"
    }
  }, empty));
}
Object.assign(__ds_scope, { DeviceCard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/DeviceCard.jsx", error: String((e && e.message) || e) }); }

// components/feedback/FlashOverlay.jsx
try { (() => {
/* UI.FlashProgressOverlay: full-window sheet at 97% opacity on WindowBg. "Writing NAND" in
   15pt bold, the console mark up to 280px square rendered greyscale with a full-colour copy
   clipped to the bottom N% - colour literally rises as the write progresses - then the %
   beneath it and a muted warning line. */
function FlashOverlay({
  value = 0,
  max = 100,
  caption = "Writing NAND",
  hint = "Do not disconnect the flasher or close J-Runner.",
  logo = "assets/xbox-sphere.png",
  size = 280,
  style
}) {
  const frac = Math.max(0, Math.min(1, value / max));
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      inset: 0,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      gap: 14,
      background: "var(--jr-window-bg)",
      opacity: "var(--jr-overlay-opacity)",
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-primary)",
      font: "700 var(--jr-text-xl)/1.2 var(--jr-font-ui)"
    }
  }, caption), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      width: size,
      height: size
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: logo,
    alt: "",
    style: {
      position: "absolute",
      inset: 0,
      width: "100%",
      height: "100%",
      objectFit: "contain",
      filter: "grayscale(1)"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      left: 0,
      right: 0,
      bottom: 0,
      height: `${frac * 100}%`,
      overflow: "hidden",
      transition: "height var(--jr-duration-fill) var(--jr-ease-fill)"
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: logo,
    alt: "",
    style: {
      position: "absolute",
      left: 0,
      bottom: 0,
      width: size,
      height: size,
      objectFit: "contain"
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)"
    }
  }, Math.round(frac * 100), "%"), /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)"
    }
  }, hint));
}
Object.assign(__ds_scope, { FlashOverlay });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/FlashOverlay.jsx", error: String((e && e.message) || e) }); }

// components/feedback/LogConsole.jsx
try { (() => {
/* The MainForm RichTextBox: borderless (Theme drops FixedSingle for RichTextBox because the
   system border renders white), FieldBg well, per-line severity colours from
   CommunicationManager.MessageColor. */
const SEV = {
  ok: "var(--jr-log-ok)",
  info: "var(--jr-log-info)",
  warn: "var(--jr-log-warn)",
  error: "var(--jr-log-error)",
  plain: "var(--jr-text-primary)",
  muted: "var(--jr-text-secondary)"
};
function LogConsole({
  lines = [],
  height = 220,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      height,
      overflowY: "auto",
      background: "var(--jr-field-bg)",
      border: "1px solid var(--jr-border)",
      padding: "var(--jr-space-4)",
      font: "400 var(--jr-text-sm)/var(--jr-console-leading) var(--jr-font-ui)",
      whiteSpace: "pre-wrap",
      wordBreak: "break-word",
      ...style
    }
  }, lines.map((l, i) => {
    const text = typeof l === "string" ? l : l.text;
    const sev = typeof l === "string" ? "plain" : l.severity || "plain";
    return /*#__PURE__*/React.createElement("div", {
      key: i,
      style: {
        color: SEV[sev]
      }
    }, text);
  }));
}
Object.assign(__ds_scope, { LogConsole });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/LogConsole.jsx", error: String((e && e.message) || e) }); }

// components/feedback/MessageDialog.jsx
try { (() => {
/* UI.MessageDialog: 400x190, PanelBg, 12px rounded region, 1px Border outline, owner dimmed
   at 45% black. Title 11pt bold TextPrimary at (20,18); body TextSecondary at (20,52);
   buttons 90x32 bottom-right, primary = accent. */
function MessageDialog({
  title,
  message,
  kind = "ok",
  onYes,
  onNo,
  onOk,
  width = 400,
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    role: "dialog",
    "aria-label": title,
    style: {
      width,
      background: "var(--jr-panel-bg)",
      border: "1px solid var(--jr-border)",
      borderRadius: "var(--jr-radius-dialog)",
      boxShadow: "var(--jr-shadow-dialog)",
      padding: "18px 20px 20px",
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--jr-text-primary)",
      font: "700 var(--jr-text-lg)/1.25 var(--jr-font-ui)"
    }
  }, title), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 10,
      minHeight: 46,
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-sm)/1.45 var(--jr-font-ui)",
      textWrap: "pretty"
    }
  }, message), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      justifyContent: "flex-end",
      gap: "var(--jr-space-5)",
      marginTop: 14
    }
  }, kind === "yesno" ? /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(__ds_scope.Button, {
    variant: "primary",
    size: "lg",
    style: {
      width: 90
    },
    onClick: onYes
  }, "Yes"), /*#__PURE__*/React.createElement(__ds_scope.Button, {
    size: "lg",
    style: {
      width: 90
    },
    onClick: onNo
  }, "No")) : /*#__PURE__*/React.createElement(__ds_scope.Button, {
    variant: "primary",
    size: "lg",
    style: {
      width: 90
    },
    onClick: onOk
  }, "Ok")));
}
Object.assign(__ds_scope, { MessageDialog });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/MessageDialog.jsx", error: String((e && e.message) || e) }); }

// components/feedback/ProgressBar.jsx
try { (() => {
/* XboxFillProgressBar.DrawBarMode: 20px track on #121214, radius = clamp(2, h/2, 8),
   accent fill clipped to the rounded path, 1px Border on top, centred % text in
   TextPrimary. Marquee mode reads "Working..." with the track filled. */
function ProgressBar({
  value = 0,
  max = 100,
  marquee,
  height = 20,
  showText = true,
  style
}) {
  const frac = marquee ? 1 : Math.max(0, Math.min(1, value / max));
  const radius = Math.max(2, Math.min(8, Math.round(height / 2)));
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      height,
      borderRadius: radius,
      background: "var(--jr-track-bg)",
      border: "1px solid var(--jr-border)",
      overflow: "hidden",
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      inset: 0,
      width: `${frac * 100}%`,
      background: "var(--jr-accent)",
      transition: "width var(--jr-duration-fill) var(--jr-ease-fill)"
    }
  }), showText && /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      height: "100%",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)"
    }
  }, marquee ? "Working..." : `${Math.round(frac * 100)}%`));
}
Object.assign(__ds_scope, { ProgressBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/ProgressBar.jsx", error: String((e && e.message) || e) }); }

// components/layout/GroupBox.jsx
try { (() => {
/* Theme.GroupBox_Paint: PanelBg fill, 6px rounded BorderSubtle frame whose top edge starts
   at half the caption height, with a gap punched out for the caption. Caption is
   TextSecondary, sits at x=11, and straddles the border line. */
function GroupBox({
  title,
  children,
  style,
  bodyStyle
}) {
  return /*#__PURE__*/React.createElement("fieldset", {
    style: {
      position: "relative",
      margin: 0,
      padding: 0,
      border: "none",
      background: "var(--jr-panel-bg)",
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      inset: `${title ? 7 : 0}px 0 0 0`,
      border: "1px solid var(--jr-border-subtle)",
      borderRadius: "var(--jr-radius-group)",
      pointerEvents: "none"
    }
  }), title && /*#__PURE__*/React.createElement("legend", {
    style: {
      position: "relative",
      margin: 0,
      padding: "0 3px",
      marginLeft: 9,
      background: "var(--jr-panel-bg)",
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-sm)/1.15 var(--jr-font-ui)"
    }
  }, title), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      padding: `${title ? 8 : 12}px 10px 10px`,
      ...bodyStyle
    }
  }, children));
}
Object.assign(__ds_scope, { GroupBox });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/layout/GroupBox.jsx", error: String((e && e.message) || e) }); }

// components/layout/MenuDropdown.jsx
try { (() => {
/* JRunnerToolStripRenderer: dropdown surface = PanelBg with a 1px Border rectangle;
   rows have no idle fill and round to 4px on hover (HoverBg) / press (PressedBg). */
function MenuDropdown({
  items = [],
  onSelect,
  style
}) {
  const [hover, setHover] = React.useState(-1);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 180,
      background: "var(--jr-panel-bg)",
      border: "1px solid var(--jr-border)",
      padding: "var(--jr-space-1)",
      ...style
    }
  }, items.map((it, i) => it === "-" ? /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      height: 1,
      background: "var(--jr-border)",
      margin: "3px 4px"
    }
  }) : /*#__PURE__*/React.createElement("div", {
    key: i,
    onMouseEnter: () => setHover(i),
    onMouseLeave: () => setHover(-1),
    onClick: () => onSelect && onSelect(it),
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      gap: 12,
      padding: "4px 10px",
      borderRadius: "var(--jr-radius-menu)",
      background: hover === i ? "var(--jr-hover-bg)" : "transparent",
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-sm)/1.3 var(--jr-font-ui)",
      cursor: "pointer"
    }
  }, /*#__PURE__*/React.createElement("span", null, typeof it === "string" ? it : it.label), typeof it !== "string" && it.submenu && /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 9,
      color: "var(--jr-text-primary)"
    }
  }, "\u25B6"))));
}
Object.assign(__ds_scope, { MenuDropdown });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/layout/MenuDropdown.jsx", error: String((e && e.message) || e) }); }

// components/layout/StatusBar.jsx
try { (() => {
/* StatusStrip case in Theme.StyleControl: PanelBg with TextPrimary items. MainForm shows
   the bundled tool versions here ("XeBuild: 1.21   Dashlaunch: 3.21"). */
function StatusBar({
  items = [],
  style
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "var(--jr-space-9)",
      height: "var(--jr-statusbar-h)",
      padding: "0 var(--jr-space-6)",
      background: "var(--jr-panel-bg)",
      color: "var(--jr-text-primary)",
      font: "400 var(--jr-text-xs)/1 var(--jr-font-ui)",
      ...style
    }
  }, items.map((it, i) => /*#__PURE__*/React.createElement("span", {
    key: i
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--jr-text-secondary)"
    }
  }, it.label, ": "), it.value)));
}
Object.assign(__ds_scope, { StatusBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/layout/StatusBar.jsx", error: String((e && e.message) || e) }); }

// components/layout/Tabs.jsx
try { (() => {
/* DarkTabControl.DrawTab: selected tab = RaisedBg + a 2px accent bar along its bottom edge;
   idle = PanelBg with a 1px BorderSubtle separator on its right. Page area gets a single
   hairline rectangle, never a 3D frame. */
function Tabs({
  tabs = [],
  value,
  onChange,
  children,
  style,
  bodyStyle
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--jr-panel-bg)",
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex"
    }
  }, tabs.map((t, i) => {
    const on = t === value;
    return /*#__PURE__*/React.createElement("button", {
      key: t,
      type: "button",
      onClick: () => onChange && onChange(t),
      style: {
        position: "relative",
        padding: "5px 14px 6px",
        border: "none",
        borderRight: on ? "none" : "1px solid var(--jr-border-subtle)",
        background: on ? "var(--jr-raised-bg)" : "var(--jr-panel-bg)",
        color: on ? "var(--jr-text-primary)" : "var(--jr-text-secondary)",
        font: "400 var(--jr-text-sm)/1.2 var(--jr-font-ui)",
        cursor: "pointer",
        boxShadow: on ? "inset 0 -2px 0 0 var(--jr-accent)" : "none"
      }
    }, t);
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      border: "1px solid var(--jr-border-subtle)",
      padding: "var(--jr-space-6)",
      ...bodyStyle
    }
  }, children));
}
Object.assign(__ds_scope, { Tabs });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/layout/Tabs.jsx", error: String((e && e.message) || e) }); }

// components/layout/TitleBar.jsx
try { (() => {
/* MainForm.Chrome: 36px bar on WindowBg, the menu strip inlined into it, a 1px
   BorderSubtle hairline underneath, and 46px-wide chrome buttons on the right.
   No maximize - the layout is absolutely positioned and does not reflow. */
function TitleBar({
  menu = [],
  activeMenu,
  onMenu,
  right,
  onMinimize,
  onClose,
  logo,
  style
}) {
  const [hover, setHover] = React.useState(null);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      display: "flex",
      alignItems: "stretch",
      height: "var(--jr-titlebar-h)",
      background: "var(--jr-window-bg)",
      borderBottom: "1px solid var(--jr-border-subtle)",
      font: "400 var(--jr-text-sm)/1 var(--jr-font-ui)",
      userSelect: "none",
      ...style
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 2,
      paddingLeft: 4
    }
  }, logo && /*#__PURE__*/React.createElement("img", {
    src: logo,
    alt: "",
    style: {
      width: 16,
      height: 16,
      margin: "0 6px 0 4px",
      objectFit: "contain"
    }
  }), menu.map(m => {
    const on = activeMenu === m || hover === m;
    return /*#__PURE__*/React.createElement("button", {
      key: m,
      type: "button",
      onClick: () => onMenu && onMenu(m),
      onMouseEnter: () => setHover(m),
      onMouseLeave: () => setHover(null),
      style: {
        position: "relative",
        height: 24,
        padding: "0 10px",
        border: "none",
        borderRadius: 10,
        background: on ? "var(--jr-hover-bg)" : "transparent",
        color: "var(--jr-text-primary)",
        font: "inherit",
        cursor: "pointer",
        boxShadow: on ? "inset 0 -2px 0 0 var(--jr-accent)" : "none"
      }
    }, m);
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), right && /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      color: "var(--jr-text-secondary)",
      paddingRight: 8
    }
  }, right), /*#__PURE__*/React.createElement(ChromeButton, {
    kind: "min",
    onClick: onMinimize
  }), /*#__PURE__*/React.createElement(ChromeButton, {
    kind: "close",
    onClick: onClose
  }));
}
function ChromeButton({
  kind,
  onClick
}) {
  const [h, setH] = React.useState(false);
  const close = kind === "close";
  return /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onClick,
    "aria-label": close ? "Close" : "Minimize",
    onMouseEnter: () => setH(true),
    onMouseLeave: () => setH(false),
    style: {
      width: "var(--jr-chrome-btn-w)",
      border: "none",
      cursor: "pointer",
      background: h ? close ? "var(--jr-danger)" : "var(--jr-hover-bg)" : "transparent",
      color: h ? close ? "#fff" : "var(--jr-chrome-glyph-hover)" : "var(--jr-chrome-glyph)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center"
    }
  }, /*#__PURE__*/React.createElement("svg", {
    width: "11",
    height: "11",
    viewBox: "0 0 11 11",
    "aria-hidden": "true",
    stroke: "currentColor",
    strokeWidth: "1.3",
    fill: "none"
  }, close ? /*#__PURE__*/React.createElement("g", null, /*#__PURE__*/React.createElement("line", {
    x1: "0.5",
    y1: "0.5",
    x2: "10.5",
    y2: "10.5"
  }), /*#__PURE__*/React.createElement("line", {
    x1: "10.5",
    y1: "0.5",
    x2: "0.5",
    y2: "10.5"
  })) : /*#__PURE__*/React.createElement("line", {
    x1: "0.5",
    y1: "5.5",
    x2: "10.5",
    y2: "5.5"
  })));
}
Object.assign(__ds_scope, { TitleBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/layout/TitleBar.jsx", error: String((e && e.message) || e) }); }

// motion.js
try { (() => {
/* Motion layer — the site's animation system.
   Grounded in two things the app actually does: the flash overlay's "colour rising through
   greyscale", and the drifting X's. Everything else is restraint plus timing.
   Every effect is gated on prefers-reduced-motion. */
(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const raf = window.requestAnimationFrame;

  /* ══ 1. Boot sequence — a short POST-code splash, once per session ══════════════ */
  function boot() {
    if (reduce || sessionStorage.getItem("jr.booted")) return;
    if (!document.body.classList.contains("is-home")) return;
    sessionStorage.setItem("jr.booted", "1");
    const el = document.createElement("div");
    el.className = "bootveil";
    el.innerHTML = '<div class="bootinner">' + '<div class="bootmark"><img src="assets/logo-jr2.png" alt=""></div>' + '<div class="bootlog"></div>' + '<div class="bootbar"><i></i></div>' + "</div>";
    document.body.appendChild(el);
    document.body.classList.add("booting");
    const LINES = ["init nand", "init network", "detect flasher", "ready"];
    const log = el.querySelector(".bootlog");
    let i = 0;
    const step = () => {
      if (i < LINES.length) {
        const d = document.createElement("div");
        d.textContent = "  * " + LINES[i];
        log.appendChild(d);
        i++;
        setTimeout(step, 130);
      } else {
        setTimeout(() => {
          el.classList.add("gone");
          document.body.classList.remove("booting");
          setTimeout(() => el.remove(), 700);
        }, 180);
      }
    };
    setTimeout(step, 220);
  }

  /* ══ 2. Headline word reveal — words rise out of a mask, staggered ══════════════ */
  function splitHeadlines() {
    document.querySelectorAll("h1, [data-split]").forEach(h => {
      if (h.dataset.split === "done") return;
      const walk = node => {
        [...node.childNodes].forEach(n => {
          if (n.nodeType === 3) {
            const frag = document.createDocumentFragment();
            n.textContent.split(/(\s+)/).forEach(tok => {
              if (!tok.trim()) {
                frag.appendChild(document.createTextNode(tok));
                return;
              }
              const w = document.createElement("span");
              w.className = "w";
              const inner = document.createElement("i");
              inner.textContent = tok;
              w.appendChild(inner);
              frag.appendChild(w);
            });
            node.replaceChild(frag, n);
          } else if (n.nodeType === 1 && !n.classList.contains("w")) {
            walk(n);
          }
        });
      };
      walk(h);
      h.querySelectorAll(".w i").forEach((el, k) => el.style.transitionDelay = k * 55 + "ms");
      h.dataset.split = "done";
      h.classList.add("split");
      if (reduce) h.classList.add("shown");
    });
  }

  /* ══ 3. One observer, several behaviours ════════════════════════════════════════ */
  const io = new IntersectionObserver(entries => {
    entries.forEach(e => {
      if (!e.isIntersecting) return;
      e.target.classList.add("shown");
      io.unobserve(e.target);
    });
  }, {
    rootMargin: "0px 0px -10% 0px",
    threshold: 0.15
  });
  const watch = (sel, cls) => document.querySelectorAll(sel).forEach(el => {
    if (cls) el.classList.add(cls);
    if (reduce) el.classList.add("shown");else io.observe(el);
  });

  /* ══ 4. Colour rising through greyscale — the app's own flash motif ═════════════ */
  function riseImages() {
    document.querySelectorAll(".shot, figure.gitem, .frow .visual, .device").forEach(el => el.classList.add("rise"));
    watch(".rise");
  }

  /* ══ 5. Scroll progress rail in the header ══════════════════════════════════════ */
  function progressRail() {
    const hdr = document.querySelector("header.site");
    if (!hdr) return;
    const rail = document.createElement("i");
    rail.className = "scrollrail";
    hdr.appendChild(rail);
    const on = () => {
      const max = document.documentElement.scrollHeight - window.innerHeight;
      rail.style.transform = "scaleX(" + (max > 0 ? window.scrollY / max : 0) + ")";
    };
    window.addEventListener("scroll", on, {
      passive: true
    });
    window.addEventListener("resize", on);
    on();
  }

  /* ══ 6. Nav scroll-spy with a sliding underline ═════════════════════════════════ */
  function scrollSpy() {
    const nav = document.querySelector("nav.top");
    if (!nav) return;
    const links = [...nav.querySelectorAll('a[href*="#"]')];
    const targets = links.map(a => {
      const id = a.getAttribute("href").split("#")[1];
      const el = id && document.getElementById(id);
      return el ? {
        a,
        el
      } : null;
    }).filter(Boolean);
    if (!targets.length) return;
    const slider = document.createElement("i");
    slider.className = "navslider";
    nav.appendChild(slider);
    let current = null;
    const move = a => {
      if (!a) {
        slider.style.opacity = 0;
        return;
      }
      const r = a.getBoundingClientRect(),
        n = nav.getBoundingClientRect();
      slider.style.opacity = 1;
      slider.style.width = r.width + "px";
      slider.style.transform = "translateX(" + (r.left - n.left) + "px)";
    };
    const on = () => {
      let found = null;
      for (const t of targets) {
        const r = t.el.getBoundingClientRect();
        if (r.top <= 140 && r.bottom > 140) found = t.a;
      }
      if (found !== current) {
        current = found;
        move(found);
      }
    };
    window.addEventListener("scroll", on, {
      passive: true
    });
    window.addEventListener("resize", () => move(current));
    on();
  }

  /* ══ 7. Magnetic buttons + press ripple ═════════════════════════════════════════ */
  function magnetic() {
    if (reduce) return;
    document.querySelectorAll(".btn").forEach(b => {
      b.addEventListener("pointermove", e => {
        const r = b.getBoundingClientRect();
        const dx = (e.clientX - (r.left + r.width / 2)) / r.width;
        const dy = (e.clientY - (r.top + r.height / 2)) / r.height;
        b.style.transform = "translate(" + dx * 5 + "px," + dy * 4 + "px)";
        b.style.setProperty("--mx", (e.clientX - r.left) / r.width * 100 + "%");
      });
      b.addEventListener("pointerleave", () => b.style.transform = "");
    });
  }

  /* ══ 8. Hero cursor spotlight ═══════════════════════════════════════════════════ */
  function spotlight() {
    const hero = document.querySelector(".hero");
    if (!hero || reduce) return;
    hero.classList.add("haslight");
    hero.addEventListener("pointermove", e => {
      const r = hero.getBoundingClientRect();
      hero.style.setProperty("--lx", e.clientX - r.left + "px");
      hero.style.setProperty("--ly", e.clientY - r.top + "px");
    });
  }

  /* ══ 9. Hero parallax on scroll ═════════════════════════════════════════════════ */
  function heroParallax() {
    const wrap = document.querySelector(".hero .shotwrap");
    if (!wrap || reduce) return;
    let ticking = false;
    const on = () => {
      if (ticking) return;
      ticking = true;
      raf(() => {
        const y = Math.min(window.scrollY, 700);
        wrap.style.setProperty("--py", y * 0.06 + "px");
        wrap.style.setProperty("--ps", 1 - Math.min(y / 4200, 0.035));
        ticking = false;
      });
    };
    window.addEventListener("scroll", on, {
      passive: true
    });
    on();
  }

  /* ══ 10. CRT treatment on the XeLL preview ══════════════════════════════════════ */
  function crt() {
    const s = document.querySelector(".xscreen");
    if (s) s.classList.add("crt");
  }

  /* ══ 11. Docs: step numbers light up as you pass them ═══════════════════════════ */
  function steps() {
    const items = document.querySelectorAll(".steps li");
    if (!items.length) return;
    const so = new IntersectionObserver(es => es.forEach(e => e.target.classList.toggle("active", e.isIntersecting)), {
      rootMargin: "-40% 0px -40% 0px"
    });
    items.forEach(li => reduce ? li.classList.add("active") : so.observe(li));
  }

  /* ══ 12. Docs TOC scroll-spy ════════════════════════════════════════════════════ */
  function tocSpy() {
    const toc = document.querySelector(".toc");
    if (!toc) return;
    const links = [...toc.querySelectorAll("a")];
    const map = links.map(a => ({
      a,
      el: document.getElementById(a.getAttribute("href").slice(1))
    })).filter(x => x.el);
    const on = () => {
      let cur = map[0];
      for (const m of map) if (m.el.getBoundingClientRect().top <= 160) cur = m;
      links.forEach(a => a.classList.toggle("on", cur && a === cur.a));
    };
    window.addEventListener("scroll", on, {
      passive: true
    });
    on();
  }

  /* ══ 13. Internal page transition ═══════════════════════════════════════════════ */
  function pageOut() {
    if (reduce) return;
    document.querySelectorAll('a[href$=".html"]').forEach(a => {
      a.addEventListener("click", e => {
        if (e.metaKey || e.ctrlKey || e.shiftKey || a.target) return;
        e.preventDefault();
        document.body.classList.add("leaving");
        setTimeout(() => location.href = a.href, 240);
      });
    });
  }

  /* ══ boot it all ════════════════════════════════════════════════════════════════ */
  function init() {
    splitHeadlines();
    watch("h1.split, h2, .eyebrow");
    riseImages();
    watch(".release, .callout, table.spec, .steps");
    progressRail();
    scrollSpy();
    magnetic();
    spotlight();
    heroParallax();
    crt();
    steps();
    tocSpy();
    pageOut();
    boot();
    raf(() => document.body.classList.add("ready"));
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);else init();
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "motion.js", error: String((e && e.message) || e) }); }

// motion2.js
try { (() => {
/* Motion layer 2 — physicality pass. Inert until site.css gains the .m2-* rules and the
   pages load this file. Everything is gated on prefers-reduced-motion. */
(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const raf = window.requestAnimationFrame;

  /* ══ 1. 3D tilt on the hardware cards ═══════════════════════════════════════════ */
  function deviceTilt() {
    if (reduce) return;
    document.querySelectorAll(".device").forEach(d => {
      d.classList.add("m2-tilt");
      d.addEventListener("pointermove", e => {
        const r = d.getBoundingClientRect();
        const px = (e.clientX - r.left) / r.width - 0.5;
        const py = (e.clientY - r.top) / r.height - 0.5;
        d.style.setProperty("--rx", -py * 12 + "deg");
        d.style.setProperty("--ry", px * 14 + "deg");
        d.style.setProperty("--gx", (px + 0.5) * 100 + "%");
        d.style.setProperty("--gy", (py + 0.5) * 100 + "%");
      });
      d.addEventListener("pointerleave", () => {
        d.style.setProperty("--rx", "0deg");
        d.style.setProperty("--ry", "0deg");
      });
    });
  }

  /* ══ 2. Gallery marquee — duplicated track, pauses on hover ═════════════════════ */
  function galleryMarquee() {
    const g = document.querySelector(".gallery");
    if (!g || reduce) return;
    const items = [...g.children];
    if (items.length < 3) return;
    const viewport = document.createElement("div");
    viewport.className = "m2-marquee";
    const track = document.createElement("div");
    track.className = "m2-track";
    items.forEach(it => track.appendChild(it));
    viewport.appendChild(track);
    g.replaceWith(viewport);

    // One "set" is the original items. Clone whole sets until the track is at least
    // twice the viewport wide, then translate by exactly one set for a seamless loop.
    const TILE = 330,
      GAP = 16;
    const setW = items.length * (TILE + GAP);
    const copies = Math.max(2, Math.ceil(window.innerWidth * 2 / setW) + 1);
    for (let c = 1; c < copies; c++) items.forEach(it => track.appendChild(it.cloneNode(true)));
    track.style.setProperty("--shift", setW + "px");
    track.style.setProperty("--dur", items.length * 7 + "s");

    // Re-bind the lightbox on the cloned tiles.
    const box = document.querySelector("[data-lightbox]");
    if (box) {
      const img = box.querySelector("img"),
        cap = box.querySelector("figcaption");
      track.querySelectorAll("[data-shot]").forEach(el => {
        el.addEventListener("click", () => {
          img.src = el.querySelector("img").src;
          cap.textContent = el.dataset.shot;
          box.classList.add("open");
        });
      });
    }
    // Colour arrives per tile as it enters the viewport. Clones start already lit so the
    // loop never shows a greyscale tile on later passes.
    const io = new IntersectionObserver(es => es.forEach(e => e.isIntersecting && e.target.classList.add("shown")), {
      threshold: 0.3
    });
    track.querySelectorAll("figure").forEach(f => {
      f.removeAttribute("data-reveal");
      f.classList.add("in", "rise");
      io.observe(f);
    });
  }

  /* ══ 3. Section number gutters — 01 / 02 / 03, scrubbing with scroll ════════════ */
  function sectionGutters() {
    const sections = [...document.querySelectorAll("section[id]")];
    if (!sections.length) return;
    sections.forEach((s, i) => {
      const g = document.createElement("i");
      g.className = "m2-gutter";
      g.dataset.n = String(i + 1).padStart(2, "0");
      s.appendChild(g);
      s.classList.add("m2-hasgutter");
    });
    if (reduce) return;
    let ticking = false;
    const on = () => {
      if (ticking) return;
      ticking = true;
      raf(() => {
        const mid = window.innerHeight / 2;
        sections.forEach(s => {
          const r = s.getBoundingClientRect();
          const active = r.top < mid && r.bottom > mid;
          s.classList.toggle("m2-active", active);
          const p = Math.max(0, Math.min(1, (mid - r.top) / Math.max(1, r.height)));
          s.style.setProperty("--sp", p);
        });
        ticking = false;
      });
    };
    window.addEventListener("scroll", on, {
      passive: true
    });
    on();
  }

  /* ══ 4. Accent trail cursor — snaps to interactive elements ═════════════════════ */
  function trailCursor() {
    if (reduce || window.matchMedia("(pointer: coarse)").matches) return;
    const dot = document.createElement("i");
    dot.className = "m2-cursor";
    document.body.appendChild(dot);
    let tx = innerWidth / 2,
      ty = innerHeight / 2,
      cx = tx,
      cy = ty;
    let tw = 10,
      th = 10,
      cw = 10,
      ch = 10,
      tr = 999,
      cr = 999,
      snapped = false;
    addEventListener("pointermove", e => {
      const hit = e.target.closest("a, button, .device, figure.gitem, .preset, .twopt");
      if (hit) {
        const r = hit.getBoundingClientRect();
        tx = r.left + r.width / 2;
        ty = r.top + r.height / 2;
        tw = r.width + 6;
        th = r.height + 6;
        tr = parseFloat(getComputedStyle(hit).borderTopLeftRadius) || 0;
        tr = Math.min(tr + 3, Math.min(tw, th) / 2);
        snapped = true;
      } else {
        tx = e.clientX;
        ty = e.clientY;
        tw = th = 10;
        tr = 999;
        snapped = false;
      }
      dot.classList.toggle("snap", snapped);
    }, {
      passive: true
    });
    (function loop() {
      const k = snapped ? 0.24 : 0.34;
      cx += (tx - cx) * k;
      cy += (ty - cy) * k;
      cw += (tw - cw) * 0.22;
      ch += (th - ch) * 0.22;
      cr += (Math.min(tr, 999) - cr) * 0.22;
      dot.style.transform = `translate(${cx}px,${cy}px) translate(-50%,-50%)`;
      dot.style.width = cw + "px";
      dot.style.height = ch + "px";
      dot.style.borderRadius = Math.min(cr, Math.min(cw, ch) / 2) + "px";
      raf(loop);
    })();
  }

  /* ══ 5. Count-up ticking in the log console ═════════════════════════════════════ */
  function logCounters() {
    const log = document.querySelector("[data-log]");
    if (!log || reduce) return;
    // Any number inside a freshly-added line ticks up to its value.
    const animate = node => {
      const m = node.textContent.match(/(\d+)%/);
      if (!m) return;
      const target = +m[1],
        tpl = node.textContent;
      let v = 0;
      const id = setInterval(() => {
        v = Math.min(target, v + Math.max(1, Math.round(target / 18)));
        node.textContent = tpl.replace(/\d+%/, v + "%");
        if (v >= target) clearInterval(id);
      }, 45);
    };
    new MutationObserver(muts => muts.forEach(mu => mu.addedNodes.forEach(n => n.nodeType === 1 && animate(n)))).observe(log, {
      childList: true
    });
  }
  function init() {
    deviceTilt();
    galleryMarquee();
    sectionGutters();
    trailCursor();
    logCounters();
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);else init();
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "motion2.js", error: String((e && e.message) || e) }); }

// site.js
try { (() => {
/* Scroll reveals, the live log ticker, the XeLL preview and the lightbox. */
(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---- reveal on scroll ---- */
  const io = new IntersectionObserver(entries => {
    entries.forEach(e => {
      if (e.isIntersecting) {
        e.target.classList.add("in");
        io.unobserve(e.target);
      }
    });
  }, {
    rootMargin: "0px 0px -8% 0px",
    threshold: 0.08
  });
  document.querySelectorAll("[data-reveal]").forEach((el, i) => {
    el.style.setProperty("--d", i % 6 * 60 + "ms");
    if (reduce) el.classList.add("in");else io.observe(el);
  });

  /* ---- hero screenshot tilt ---- */
  const shot = document.querySelector("[data-tilt]");
  if (shot && !reduce) {
    shot.addEventListener("pointermove", e => {
      const r = shot.getBoundingClientRect();
      const px = (e.clientX - r.left) / r.width - 0.5,
        py = (e.clientY - r.top) / r.height - 0.5;
      const t = `perspective(1400px) rotateY(${px * 5}deg) rotateX(${-py * 4}deg) translateY(-4px)`;
      shot.style.transform = t;
      shot.parentElement.style.setProperty("--tilt", t);
    });
    shot.addEventListener("pointerleave", () => {
      shot.style.transform = "";
      shot.parentElement.style.removeProperty("--tilt");
    });
  }

  /* ---- live session log ---- */
  const log = document.querySelector("[data-log]");
  if (log) {
    const SCRIPTED = [["J-Runner Premium", ""], ["Session: 07/30/2026 3:27:15", ""], ["Version: 4.0.0pre1", ""], ["Status: Up to date", "ok"], ["", ""], ["PicoFlasher detected on COM4", "ok"], ["Reading Nand... pass 1 of 2", "info"], ["Nand read OK — 0 bad blocks", "ok"], ["CB Type: Corona 4GB · LDV 12", "info"], ["Building XeBuild image (17559, glitch2)...", "info"], ["Image written to output\\updflash.bin", "ok"], ["Ready to write. Disconnect nothing.", "warn"]];
    let i = 0;
    const push = () => {
      const [text, sev] = SCRIPTED[i % SCRIPTED.length];
      const line = document.createElement("div");
      line.className = "logline" + (sev ? " sev-" + sev : "");
      line.textContent = text || "\u00a0";
      log.appendChild(line);
      log.scrollTop = log.scrollHeight;
      while (log.children.length > 40) log.removeChild(log.firstChild);
      i++;
    };
    SCRIPTED.forEach(() => {});
    for (let k = 0; k < 5; k++) push();
    if (!reduce) setInterval(push, 1400);
  }

  /* ---- XeLL preview cycling ---- */
  const xell = document.querySelector("[data-xell]");
  if (xell) {
    const PRESETS = [{
      id: "default",
      name: "Default",
      bg: "4E44D8",
      fg: "FFFFFF"
    }, {
      id: "swizzy",
      name: "Swizzy",
      bg: "000000",
      fg: "FF6600"
    }, {
      id: "xtudo",
      name: "XTUDO",
      bg: "000000",
      fg: "FF66FF"
    }, {
      id: "classic",
      name: "Classic",
      bg: "000000",
      fg: "008000"
    }];
    const screen = xell.querySelector("[data-xell-screen]");
    const btns = xell.querySelectorAll("[data-preset]");
    let idx = 0;
    const apply = n => {
      idx = n;
      const p = PRESETS[n];
      screen.style.background = "#" + p.bg;
      screen.style.color = "#" + p.fg;
      btns.forEach((b, j) => b.classList.toggle("on", j === n));
    };
    btns.forEach((b, j) => b.addEventListener("click", () => {
      apply(j);
      clearInterval(timer);
    }));
    apply(0);
    let timer = reduce ? null : setInterval(() => apply((idx + 1) % PRESETS.length), 3200);
  }

  /* ---- lightbox ---- */
  const box = document.querySelector("[data-lightbox]");
  if (box) {
    const img = box.querySelector("img"),
      cap = box.querySelector("figcaption");
    document.querySelectorAll("[data-shot]").forEach(el => {
      el.addEventListener("click", () => {
        img.src = el.querySelector("img").src;
        cap.textContent = el.dataset.shot;
        box.classList.add("open");
      });
    });
    const close = () => box.classList.remove("open");
    box.addEventListener("click", close);
    document.addEventListener("keydown", e => {
      if (e.key === "Escape") close();
    });
  }

  /* ---- header condense ---- */
  const hdr = document.querySelector("[data-header]");
  if (hdr) {
    const on = () => hdr.classList.toggle("stuck", window.scrollY > 24);
    window.addEventListener("scroll", on, {
      passive: true
    });
    on();
  }
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "site.js", error: String((e && e.message) || e) }); }

// tweaks.js
try { (() => {
/* Tweaks — three controls that reshape the page's feel, not its pixels.
   Hidden unless opened; state persists under one namespaced localStorage key. */
(function () {
  const KEY = "jr.site.tweaks";
  const DEFAULTS = {
    storm: "drift",
    accent: "green",
    rhythm: "showcase"
  };
  const ACCENTS = {
    green: {
      name: "J-Runner",
      a: "#74c757",
      light: "#a1e082",
      dim: "#468037",
      on: "#141414"
    },
    blue: {
      name: "XeLL",
      a: "#4E44D8",
      light: "#7a72e6",
      dim: "#332c9e",
      on: "#ffffff"
    },
    orange: {
      name: "Swizzy",
      a: "#FF6600",
      light: "#ff8c3d",
      dim: "#a34200",
      on: "#141414"
    },
    pink: {
      name: "XTUDO",
      a: "#FF66FF",
      light: "#ff99ff",
      dim: "#a340a3",
      on: "#141414"
    }
  };
  const STORM = {
    off: {
      name: "Still",
      count: 0,
      speed: 0,
      scale: 1,
      fade: 0
    },
    drift: {
      name: "Drift",
      count: 1,
      speed: 1,
      scale: 1,
      fade: 1
    },
    blizzard: {
      name: "Blizzard",
      count: 2.6,
      speed: 2.4,
      scale: 1.5,
      fade: 1
    }
  };
  const RHYTHM = {
    showcase: "Showcase",
    workbench: "Workbench",
    terminal: "Terminal"
  };
  let state = {
    ...DEFAULTS
  };
  try {
    Object.assign(state, JSON.parse(localStorage.getItem(KEY) || "{}"));
  } catch (e) {}
  function apply() {
    const a = ACCENTS[state.accent] || ACCENTS.green;
    const r = document.documentElement.style;
    r.setProperty("--jr-accent", a.a);
    r.setProperty("--jr-accent-light", a.light);
    r.setProperty("--jr-accent-dim", a.dim);
    r.setProperty("--jr-text-on-accent", a.on);
    r.setProperty("--jr-log-ok", a.light);
    document.body.dataset.rhythm = state.rhythm;
    window.__xfall && window.__xfall.set(STORM[state.storm] || STORM.drift);
    document.querySelectorAll("[data-tw]").forEach(el => {
      el.classList.toggle("on", state[el.dataset.tw] === el.dataset.val);
    });
    try {
      localStorage.setItem(KEY, JSON.stringify(state));
    } catch (e) {}
  }
  function group(label, hint, key, opts) {
    return '<div class="twgroup"><div class="twlabel">' + label + '</div><div class="twhint">' + hint + '</div><div class="twrow">' + opts.map(([val, name]) => '<button class="twopt" data-tw="' + key + '" data-val="' + val + '">' + name + "</button>").join("") + "</div></div>";
  }
  const panel = document.createElement("div");
  panel.className = "tweaks";
  panel.innerHTML = '<button class="twtoggle" aria-label="Tweaks">Tweaks</button>' + '<div class="twbody">' + '<div class="twhead">Tweaks<button class="twclose" aria-label="Close">✕</button></div>' + group("Background", "How hard the X's fall behind everything.", "storm", Object.entries(STORM).map(([k, v]) => [k, v.name])) + group("Accent", "The one brand colour, borrowed from the XeLL presets.", "accent", Object.entries(ACCENTS).map(([k, v]) => [k, v.name])) + group("Rhythm", "Type scale, spacing and corners as one decision.", "rhythm", Object.entries(RHYTHM)) + "</div>";
  document.body.appendChild(panel);
  panel.querySelector(".twtoggle").addEventListener("click", () => panel.classList.toggle("open"));
  panel.querySelector(".twclose").addEventListener("click", () => panel.classList.remove("open"));
  panel.querySelectorAll("[data-tw]").forEach(b => b.addEventListener("click", () => {
    state[b.dataset.tw] = b.dataset.val;
    apply();
  }));
  apply();
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "tweaks.js", error: String((e && e.message) || e) }); }

// ui_kits/jrunner-app/MainWindow.jsx
try { (() => {
const {
  TitleBar,
  StatusBar,
  MenuDropdown,
  MessageDialog,
  FlashOverlay
} = window.JRunnerPremiumDesignSystem_5a2f40;
const MENUS = {
  Tools: ["Address Calculator", "CPU Key Generator", "SMC Config Editor", "-", "Hex Editor", "Keyvault Decrypter"],
  Nand: ["Read Nand", "Write Nand", "Create ECC", "-", "Extract Files", "Nand Compare"],
  Advanced: ["Program Glitch Chip", "XeLL Customizer", "-", "Create Donor Nand", "Patch Keyvault", "-", "Settings"]
};
const INITIAL_LOG = ["J-Runner Premium", "Session: 07/30/2026 3:27:15", "Version: 4.0.0devpre4", {
  text: "Status: Up to date",
  severity: "ok"
}, "", {
  text: "XeLL Customizer: Starting local server on port 2222...",
  severity: "muted"
}, {
  text: "XeLL Customizer: [@octokit/request] \"POST /repos/stackflow85/xell-builder/actions/workflows/build.yml/dispatches\" is deprecated.",
  severity: "muted"
}];
function MainWindow() {
  const [menu, setMenu] = React.useState(null);
  const [modal, setModal] = React.useState(null);
  const [progress, setProgress] = React.useState(0);
  const [state, setState] = React.useState({
    reads: 2,
    iface: "USB",
    cpuKey: "",
    flasher: null,
    eta: "",
    kernel: "",
    console: "",
    cb: null,
    ip: "192.168.12.",
    xbTab: "XeBuild",
    infoTab: "Nand Info",
    nand: {},
    kv: {},
    badBlocks: [],
    log: INITIAL_LOG,
    progress: 0
  });
  const set = patch => setState(s => ({
    ...s,
    ...patch
  }));
  const say = line => setState(s => ({
    ...s,
    log: [...s.log, line]
  }));

  // Detect a flasher a moment after load, exactly like the real app's device poll.
  React.useEffect(() => {
    const t = setTimeout(() => {
      set({
        flasher: "../../assets/device-picoflasher.png"
      });
      say({
        text: "PicoFlasher detected on COM4",
        severity: "ok"
      });
    }, 900);
    return () => clearTimeout(t);
  }, []);
  const startFlash = () => {
    setModal(null);
    setProgress(0);
    setModal("flashing");
    let v = 0;
    const id = setInterval(() => {
      v += 4;
      setProgress(v);
      if (v >= 100) {
        clearInterval(id);
        setTimeout(() => {
          setModal("done");
          say({
            text: "Nand write complete.",
            severity: "ok"
          });
        }, 700);
      }
    }, 120);
  };
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      width: 832,
      height: 697,
      background: "var(--jr-window-bg)",
      boxShadow: "var(--jr-shadow-window)",
      display: "flex",
      flexDirection: "column",
      overflow: "hidden"
    },
    onClick: () => setMenu(null)
  }, /*#__PURE__*/React.createElement("div", {
    onClick: e => e.stopPropagation()
  }, /*#__PURE__*/React.createElement(TitleBar, {
    logo: "../../assets/logo-jr.png",
    menu: Object.keys(MENUS),
    activeMenu: menu,
    onMenu: m => setMenu(menu === m ? null : m),
    right: /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 12,
        paddingRight: 4
      }
    }, "V4.0.0devpre4"),
    onMinimize: () => {},
    onClose: () => {}
  }), menu && /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      zIndex: 40,
      top: 34,
      left: Object.keys(MENUS).indexOf(menu) * 0 + 8 + Object.keys(MENUS).slice(0, Object.keys(MENUS).indexOf(menu)).reduce((a, m) => a + m.length * 7 + 20, 26)
    }
  }, /*#__PURE__*/React.createElement(MenuDropdown, {
    items: MENUS[menu],
    onSelect: it => {
      setMenu(null);
      if (it === "Program Glitch Chip") setModal("glitch");else if (it === "Write Nand") setModal("confirm");else say({
        text: it + ": not available in this recreation.",
        severity: "muted"
      });
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      gap: 10,
      padding: "10px 10px 0",
      minHeight: 0
    }
  }, /*#__PURE__*/React.createElement(NandColumn, {
    state: {
      ...state,
      progress
    },
    set: set,
    onWrite: () => setModal("confirm"),
    onProgram: () => setModal("glitch")
  }), /*#__PURE__*/React.createElement(SidePanel, {
    state: state,
    set: set
  })), /*#__PURE__*/React.createElement(StatusBar, {
    items: [{
      label: "XeBuild",
      value: "1.21"
    }, {
      label: "Dashlaunch",
      value: "3.21"
    }]
  }), (modal === "confirm" || modal === "done" || modal === "glitch") && /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      inset: 0,
      background: "var(--jr-scrim)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      zIndex: 60
    }
  }, modal === "confirm" && /*#__PURE__*/React.createElement(MessageDialog, {
    kind: "yesno",
    title: "Confirm Flash",
    message: "Are you sure? Did you make sure to select the correct options and patches?",
    onYes: startFlash,
    onNo: () => setModal(null)
  }), modal === "done" && /*#__PURE__*/React.createElement(MessageDialog, {
    title: "Flash Complete",
    message: "Remember to disconnect the flasher from the computer before booting!",
    onOk: () => setModal(null)
  }), modal === "glitch" && /*#__PURE__*/React.createElement(GlitchChipDialog, {
    onClose: () => setModal(null)
  })), modal === "flashing" && /*#__PURE__*/React.createElement(FlashOverlay, {
    value: progress,
    logo: "../../assets/xbox-sphere.png",
    size: 240
  }));
}
Object.assign(window, {
  MainWindow
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/jrunner-app/MainWindow.jsx", error: String((e && e.message) || e) }); }

// ui_kits/jrunner-app/dialogs.jsx
try { (() => {
const {
  Button,
  Radio
} = window.JRunnerPremiumDesignSystem_5a2f40;

/* Recreation of Forms/GlitchChipProgrammer.cs — 420x300, PanelBg, 10px rounded, its own
   36px WindowBg title bar with a ✕ (U+2715) close, and an accent Program button. */
function GlitchChipDialog({
  onClose
}) {
  const [variant, setVariant] = React.useState("RPicoRGH");
  const [status, setStatus] = React.useState(null);
  const [busy, setBusy] = React.useState(false);
  const program = () => {
    setBusy(true);
    setStatus({
      text: "Looking for a Pico in BOOTSEL mode...",
      ok: null
    });
    setTimeout(() => {
      setStatus({
        text: "Programmed successfully. Unplug and replug the Pico.",
        ok: true
      });
      setBusy(false);
    }, 1400);
  };
  return /*#__PURE__*/React.createElement("div", {
    style: {
      width: 420,
      background: "var(--jr-panel-bg)",
      border: "1px solid var(--jr-border)",
      borderRadius: "var(--jr-radius-dialog-sm)",
      boxShadow: "var(--jr-shadow-dialog)",
      overflow: "hidden"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      height: 36,
      background: "var(--jr-window-bg)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      paddingLeft: 14,
      font: "700 var(--jr-text-md)/1 var(--jr-font-ui)"
    }
  }, "Program Glitch Chip"), /*#__PURE__*/React.createElement("span", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement("button", {
    onClick: onClose,
    style: {
      width: 36,
      height: 36,
      background: "none",
      border: "none",
      cursor: "pointer",
      color: "var(--jr-chrome-glyph)",
      font: "13px var(--jr-font-ui)"
    }
  }, "\u2715")), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "14px 20px 20px"
    }
  }, /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      color: "var(--jr-text-secondary)",
      font: "400 var(--jr-text-sm)/1.5 var(--jr-font-ui)",
      textWrap: "pretty"
    }
  }, "Flashes a Raspberry Pi Pico set to BOOTSEL mode with RGH 1.2 glitcher firmware. Hold the BOOTSEL button while plugging the Pico into this PC, then click Program."), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6,
      margin: "16px 0 0 4px"
    }
  }, /*#__PURE__*/React.createElement(Radio, {
    checked: variant === "RPicoRGH",
    onChange: () => setVariant("RPicoRGH"),
    label: "RPicoRGH (recommended)"
  }), /*#__PURE__*/React.createElement(Radio, {
    checked: variant === "PicoRGH",
    onChange: () => setVariant("PicoRGH"),
    label: "PicoRGH (original/legacy)"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      minHeight: 38,
      marginTop: 16,
      font: "400 var(--jr-text-sm)/1.4 var(--jr-font-ui)",
      color: status ? status.ok === true ? "var(--jr-accent)" : status.ok === false ? "var(--jr-danger)" : "var(--jr-text-secondary)" : "transparent"
    }
  }, status ? status.text : "."), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "xl",
    disabled: busy,
    onClick: program,
    style: {
      width: 110
    }
  }, "Program")));
}
Object.assign(window, {
  GlitchChipDialog
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/jrunner-app/dialogs.jsx", error: String((e && e.message) || e) }); }

// ui_kits/jrunner-app/panels.jsx
try { (() => {
const {
  Button,
  SplitButton,
  TextField,
  Select,
  NumberField,
  Checkbox,
  Radio,
  GroupBox,
  Tabs,
  DeviceCard,
  ProgressBar,
  LogConsole,
  DataTable
} = window.JRunnerPremiumDesignSystem_5a2f40;
const CB_TYPES = ["Retail", "Glitch", "Glitch2", "Glitch2m", "JTAG", "DEVGL"];
function NandColumn({
  state,
  set,
  onWrite,
  onProgram
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 8,
      width: 470
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8,
      alignItems: "flex-start"
    }
  }, /*#__PURE__*/React.createElement(GroupBox, {
    title: "Nand",
    style: {
      flex: 1
    },
    bodyStyle: {
      display: "flex",
      gap: 6
    }
  }, ["Read Nand", "Create ECC", "Write ECC", "Create XeBuild Image"].map(t => /*#__PURE__*/React.createElement(Button, {
    key: t,
    style: {
      width: 72,
      height: 52,
      whiteSpace: "normal",
      lineHeight: 1.25
    }
  }, t)), /*#__PURE__*/React.createElement(Button, {
    style: {
      width: 72,
      height: 52,
      whiteSpace: "normal",
      lineHeight: 1.25
    },
    onClick: onWrite
  }, "Write Nand")), /*#__PURE__*/React.createElement(GroupBox, {
    title: "Glitch Chip",
    style: {
      width: 96
    },
    bodyStyle: {
      display: "flex"
    }
  }, /*#__PURE__*/React.createElement(Button, {
    style: {
      width: 74,
      height: 52,
      whiteSpace: "normal",
      lineHeight: 1.25
    },
    onClick: onProgram
  }, "Program Timing File"))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8,
      alignItems: "flex-start"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 8,
      width: 250
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8,
      alignItems: "flex-start"
    }
  }, /*#__PURE__*/React.createElement(GroupBox, {
    title: "Nand Reads",
    style: {
      width: 66
    },
    bodyStyle: {
      padding: "8px 8px 10px"
    }
  }, /*#__PURE__*/React.createElement(NumberField, {
    value: state.reads,
    onChange: v => set({
      reads: v
    }),
    width: 44
  })), /*#__PURE__*/React.createElement(GroupBox, {
    title: "Glitch Chip Programming",
    style: {
      flex: 1
    },
    bodyStyle: {
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Radio, {
    checked: state.iface === "USB",
    onChange: () => set({
      iface: "USB"
    }),
    label: "USB"
  }), /*#__PURE__*/React.createElement(Radio, {
    checked: state.iface === "LPT",
    onChange: () => set({
      iface: "LPT"
    }),
    label: "LPT"
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(4,1fr)",
      gap: 6
    }
  }, ["CPU Key Database", "Create Donor", "Extract Files", "Patch Keyvault"].map(t => /*#__PURE__*/React.createElement(Button, {
    key: t,
    style: {
      height: 46,
      whiteSpace: "normal",
      lineHeight: 1.25,
      padding: "0 4px"
    }
  }, t)))), /*#__PURE__*/React.createElement(DeviceCard, {
    style: {
      flex: 1
    },
    height: 112,
    image: state.flasher,
    name: state.flasher ? "PicoFlasher" : undefined,
    detail: state.flasher ? "RP2040 · COM4" : undefined
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8,
      alignItems: "flex-start"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Button, {
    style: {
      width: 88
    }
  }, "Load Source"), /*#__PURE__*/React.createElement(TextField, null)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Button, {
    style: {
      width: 88
    }
  }, "Load Extra"), /*#__PURE__*/React.createElement(TextField, null)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6,
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 88,
      fontSize: 12,
      paddingLeft: 4
    }
  }, "CPU Key:"), /*#__PURE__*/React.createElement(TextField, {
    mono: true,
    value: state.cpuKey,
    onChange: v => set({
      cpuKey: v
    })
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6,
      width: 78
    }
  }, /*#__PURE__*/React.createElement(Button, {
    block: true,
    style: {
      height: 46,
      whiteSpace: "normal",
      lineHeight: 1.25
    }
  }, "Nand Compare"), /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "Reload"))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8,
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 56,
      fontSize: 12,
      color: "var(--jr-text-primary)"
    }
  }, "Progress"), /*#__PURE__*/React.createElement(ProgressBar, {
    value: state.progress,
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement(TextField, {
    width: 70,
    readOnly: true,
    value: state.eta
  })), /*#__PURE__*/React.createElement(LogConsole, {
    height: 186,
    lines: state.log
  }));
}
function XeBuildPanel({
  state,
  set
}) {
  return /*#__PURE__*/React.createElement(GroupBox, {
    title: "XeBuild",
    bodyStyle: {
      padding: "6px 8px 10px"
    }
  }, /*#__PURE__*/React.createElement(Tabs, {
    tabs: ["XeBuild", "XB Settings", "Patches", "Dashlaunch"],
    value: state.xbTab,
    onChange: t => set({
      xbTab: t
    }),
    bodyStyle: {
      padding: 10,
      minHeight: 104,
      borderTop: "none"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 12
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      marginBottom: 4
    }
  }, "Kernel Version"), /*#__PURE__*/React.createElement(Select, {
    width: 92,
    value: state.kernel,
    options: ["17559", "17544", "16767", "14719"],
    onChange: v => set({
      kernel: v
    }),
    placeholder: "\u2014\u2014\u2014\u2014"
  })), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      marginBottom: 4
    }
  }, "Console Type"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6,
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement(Select, {
    width: 104,
    value: state.console,
    options: ["Falcon", "Jasper", "Trinity", "Corona", "Corona 4GB"],
    onChange: v => set({
      console: v
    })
  }), /*#__PURE__*/React.createElement(Button, {
    size: "sm",
    style: {
      width: 22,
      padding: 0
    }
  }, "?")))), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 120,
      display: "flex",
      flexDirection: "column",
      gap: 5
    }
  }, CB_TYPES.map(t => /*#__PURE__*/React.createElement(Radio, {
    key: t,
    disabled: !state.console,
    checked: state.cb === t,
    onChange: () => set({
      cb: t
    }),
    label: t
  }))))));
}
const NAND_FIELDS = [["Console", "CB Type"], ["2BL [CB_A]", null], ["2BL [CB_B]", null], ["4BL [CD]", null], ["5BL [CE]", null], ["6BL [CF] Patch 0", "6BL [CF] Patch 1"], ["7BL [CG] Patch 0", "7BL [CG] Patch 1"]];
function NandInfoPanel({
  state,
  set
}) {
  return /*#__PURE__*/React.createElement(Tabs, {
    tabs: ["Nand Info", "KV Info", "Bad Blocks"],
    value: state.infoTab,
    onChange: t => set({
      infoTab: t
    }),
    bodyStyle: {
      padding: "12px 10px",
      minHeight: 236
    }
  }, state.infoTab === "Nand Info" && /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 8
    }
  }, NAND_FIELDS.map(([a, b], i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "grid",
      gridTemplateColumns: "1fr 1fr",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement(TextField, {
    label: a,
    labelWidth: 92,
    value: state.nand[a] || "",
    readOnly: true
  }), b ? /*#__PURE__*/React.createElement(TextField, {
    label: b,
    labelWidth: 92,
    value: state.nand[b] || "",
    readOnly: true
  }) : /*#__PURE__*/React.createElement("span", null))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "1fr 1fr",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6,
      alignItems: "center",
      justifyContent: "flex-end"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12
    }
  }, "LDV"), /*#__PURE__*/React.createElement(TextField, {
    width: 30,
    readOnly: true
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12
    }
  }, "PD"), /*#__PURE__*/React.createElement(TextField, {
    width: 78,
    readOnly: true
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6,
      alignItems: "center",
      justifyContent: "flex-end"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12
    }
  }, "LDV"), /*#__PURE__*/React.createElement(TextField, {
    width: 30,
    readOnly: true
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 12
    }
  }, "PD"), /*#__PURE__*/React.createElement(TextField, {
    width: 78,
    readOnly: true
  })))), state.infoTab === "KV Info" && /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 8
    }
  }, ["Console ID", "Console Serial", "Console Type", "Manufacturing Date", "Game Region", "DVD Key"].map(l => /*#__PURE__*/React.createElement(TextField, {
    key: l,
    label: l,
    labelWidth: 116,
    readOnly: true,
    value: state.kv[l] || "",
    mono: l === "DVD Key"
  }))), state.infoTab === "Bad Blocks" && /*#__PURE__*/React.createElement(DataTable, {
    columns: [{
      key: "block",
      label: "Block",
      mono: true
    }, {
      key: "addr",
      label: "Address",
      mono: true
    }, {
      key: "kind",
      label: "Type"
    }],
    rows: state.badBlocks
  }));
}
function SidePanel({
  state,
  set
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 8,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement(XeBuildPanel, {
    state: state,
    set: set
  }), /*#__PURE__*/React.createElement(NandInfoPanel, {
    state: state,
    set: set
  }), /*#__PURE__*/React.createElement(SplitButton, {
    onClick: () => {},
    onOpen: () => {},
    style: {
      alignSelf: "stretch"
    }
  }, "Show Working Folder"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "1fr 1fr",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "New Session"), /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "Restart")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "Settings"), /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "Exit"))), /*#__PURE__*/React.createElement(GroupBox, {
    bodyStyle: {
      display: "flex",
      flexDirection: "column",
      gap: 6,
      padding: 10
    }
  }, /*#__PURE__*/React.createElement(TextField, {
    label: "IP:",
    labelWidth: 20,
    value: state.ip,
    onChange: v => set({
      ip: v
    })
  }), /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "Get CPU Key"), /*#__PURE__*/React.createElement(Button, {
    block: true
  }, "Scan IP")));
}
Object.assign(window, {
  NandColumn,
  XeBuildPanel,
  NandInfoPanel,
  SidePanel
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/jrunner-app/panels.jsx", error: String((e && e.message) || e) }); }

// ui_kits/xell-customizer/XellCustomizer.jsx
try { (() => {
const CONSOLE_COLORS = {
  RED: "FF0000",
  BLUE: "4E44D8",
  GREEN: "008000",
  BLACK: "000000",
  WHITE: "FFFFFF",
  GREY: "C0C0C0",
  BROWN: "993300",
  PURPLE: "9900FF",
  YELLOW: "FFFF00",
  ORANGE: "FF6600",
  PINK: "FF66FF"
};
const PRESETS = [{
  id: "default",
  name: "Default Theme",
  description: "Classic XeLL blue background with white text",
  bg: "4E44D8",
  fg: "FFFFFF"
}, {
  id: "swizzy",
  name: "Swizzy Theme",
  description: "Black background with orange text - Swizzy's favorite!",
  bg: "000000",
  fg: "FF6600"
}, {
  id: "xtudo",
  name: "XTUDO Theme",
  description: "Black background with pink text - Niceshot's favorite!",
  bg: "000000",
  fg: "FF66FF"
}, {
  id: "classic",
  name: "Classic Theme",
  description: "Black background with green text",
  bg: "000000",
  fg: "008000"
}];
const ASCII = `   __  __     ____  ____
  |  \\/  |   |  __||  __|
  |      | ___| |__ | |__
  |  |\\/| |/ _ \\  __||  __|
  |__|  |_|\\___/____||____|`;
const CONSOLE_TEXT = ({
  ascii
}) => ["  * Xenos FB with 148x41 (1280x720) at 0x9e000000 initialized.", "", "XeLL - Xenon linux loader second stage v0.993-git-7526b02 2026-07-30 (LibXenon.org)", "", "Built with GCC 13.3.0 and Binutils 2.44", "", ascii, "", "        Free60.org XeLL - Xenon Linux Loader v0.993-git-7526b02", "         Special Corona & Winchester Compatible XeLL version", "", "  * nand init", "  * network init", "  * initializing lwip 1.4.1...", "Reinit PHY...", "Waiting for link...link still down.", "  * requesting dhcp...................."].join("\n");
const isDark = hex => {
  const r = parseInt(hex.slice(0, 2), 16),
    g = parseInt(hex.slice(2, 4), 16),
    b = parseInt(hex.slice(4, 6), 16);
  return (r * 299 + g * 587 + b * 114) / 1000 < 128;
};
const Card = ({
  children,
  style
}) => /*#__PURE__*/React.createElement("div", {
  style: {
    background: "var(--xc-card)",
    border: "1px solid var(--xc-border)",
    borderRadius: 14,
    boxShadow: "0 4px 6px -1px rgb(0 0 0 / .2)",
    marginBottom: 24,
    ...style
  }
}, children);
const CardHeader = ({
  icon,
  title,
  description
}) => /*#__PURE__*/React.createElement("div", {
  style: {
    padding: "24px 24px 12px"
  }
}, /*#__PURE__*/React.createElement("div", {
  style: {
    display: "flex",
    alignItems: "center",
    gap: 8,
    font: "600 16px/1.4 var(--jr-font-ui)",
    color: "var(--xc-foreground)"
  }
}, /*#__PURE__*/React.createElement("i", {
  "data-lucide": icon,
  style: {
    width: 20,
    height: 20,
    color: "var(--xc-icon)"
  }
}), title), /*#__PURE__*/React.createElement("div", {
  style: {
    marginTop: 6,
    font: "400 14px/1.4 var(--jr-font-ui)",
    color: "var(--xc-muted-foreground)"
  }
}, description));
function XellCustomizer() {
  const [bg, setBg] = React.useState("4E44D8");
  const [fg, setFg] = React.useState("FFFFFF");
  const [tab, setTab] = React.useState("presets");
  const [ascii, setAscii] = React.useState(ASCII);
  React.useEffect(() => {
    if (window.lucide) window.lucide.createIcons();
  });
  const TabBtn = ({
    id,
    label
  }) => /*#__PURE__*/React.createElement("button", {
    onClick: () => setTab(id),
    style: {
      padding: "7px 12px",
      borderRadius: 8,
      border: "none",
      cursor: "pointer",
      background: tab === id ? "var(--xc-card)" : "transparent",
      color: tab === id ? "var(--xc-foreground)" : "var(--xc-muted-foreground)",
      font: "500 14px/1 var(--jr-font-ui)",
      boxShadow: tab === id ? "0 1px 2px rgb(0 0 0/.3)" : "none"
    }
  }, label);
  const swatchGrid = (setter, current) => /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(6,1fr)",
      gap: 8
    }
  }, Object.entries(CONSOLE_COLORS).map(([name, val]) => /*#__PURE__*/React.createElement("button", {
    key: name,
    onClick: () => setter(val),
    style: {
      height: 40,
      display: "flex",
      alignItems: "center",
      justifyContent: "flex-start",
      gap: 4,
      padding: "0 10px",
      background: "#" + val,
      color: isDark(val) ? "#fff" : "#0f172a",
      border: "1px solid var(--xc-border)",
      borderRadius: 8,
      cursor: "pointer",
      font: "500 12px/1 var(--jr-font-ui)",
      textTransform: "uppercase"
    }
  }, name.toLowerCase(), current === val && /*#__PURE__*/React.createElement("i", {
    "data-lucide": "check",
    style: {
      width: 14,
      height: 14
    }
  }))));
  return /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: 1024,
      margin: "0 auto",
      padding: "32px 16px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: "center",
      marginBottom: 40
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "inline-flex",
      padding: 8,
      borderRadius: 999,
      background: "rgb(30 58 138 / .3)"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "terminal",
    style: {
      width: 24,
      height: 24,
      color: "#60a5fa"
    }
  })), /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: "16px 0 8px",
      font: "700 36px/1.1 var(--jr-font-ui)",
      background: "linear-gradient(to right,var(--xc-heading-from),var(--xc-heading-to))",
      WebkitBackgroundClip: "text",
      backgroundClip: "text",
      color: "transparent"
    }
  }, "XeLL Theme Customizer"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      color: "var(--xc-muted-foreground)",
      font: "400 16px/1.5 var(--jr-font-ui)"
    }
  }, "Customize the appearance of your XeLL console with colors and ASCII art"), /*#__PURE__*/React.createElement("p", {
    style: {
      marginTop: 12,
      font: "400 14px/1 var(--jr-font-ui)",
      color: "var(--xc-foreground)"
    }
  }, /*#__PURE__*/React.createElement("strong", null, "Thanks to:"), " ", /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: "#60a5fa"
    }
  }, "Cancer_"))), /*#__PURE__*/React.createElement(Card, null, /*#__PURE__*/React.createElement(CardHeader, {
    icon: "monitor",
    title: "Console Preview",
    description: "Live preview of your XeLL console"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "0 24px 24px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      border: "8px solid #1e293b",
      borderRadius: 10,
      boxShadow: "0 10px 15px -3px rgb(0 0 0/.4)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      aspectRatio: "16 / 9",
      overflow: "auto",
      padding: "8px 32px",
      whiteSpace: "pre",
      font: "400 13px/1.15 var(--jr-font-mono)",
      background: "#" + bg,
      color: "#" + fg
    }
  }, CONSOLE_TEXT({
    ascii
  }))))), /*#__PURE__*/React.createElement(Card, null, /*#__PURE__*/React.createElement(CardHeader, {
    icon: "layout",
    title: "Settings",
    description: "Customize your console appearance"
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "0 24px 24px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(4,1fr)",
      gap: 4,
      padding: 4,
      borderRadius: 10,
      background: "var(--xc-muted)",
      marginBottom: 20
    }
  }, /*#__PURE__*/React.createElement(TabBtn, {
    id: "presets",
    label: "Presets"
  }), /*#__PURE__*/React.createElement(TabBtn, {
    id: "background",
    label: "Background"
  }), /*#__PURE__*/React.createElement(TabBtn, {
    id: "foreground",
    label: "Text"
  }), /*#__PURE__*/React.createElement(TabBtn, {
    id: "ascii",
    label: "ASCII Art"
  })), tab === "presets" && /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "1fr 1fr",
      gap: 16
    }
  }, PRESETS.map(p => /*#__PURE__*/React.createElement("button", {
    key: p.id,
    onClick: () => {
      setBg(p.bg);
      setFg(p.fg);
    },
    style: {
      textAlign: "left",
      padding: 16,
      borderRadius: 10,
      cursor: "pointer",
      background: "rgb(255 255 255 / .04)",
      border: "1px solid var(--xc-border)",
      color: "var(--xc-foreground)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      font: "600 14px/1 var(--jr-font-ui)"
    }
  }, p.name), /*#__PURE__*/React.createElement("span", {
    style: {
      display: "flex",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 24,
      height: 24,
      borderRadius: 999,
      background: "#" + p.bg,
      border: "1px solid #cbd5e1"
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      width: 24,
      height: 24,
      borderRadius: 999,
      background: "#" + p.fg,
      border: "1px solid #cbd5e1"
    }
  }))), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: "6px 0 0",
      font: "400 12px/1.4 var(--jr-font-ui)",
      color: "var(--xc-muted-foreground)"
    }
  }, p.description)))), tab === "background" && swatchGrid(setBg, bg), tab === "foreground" && swatchGrid(setFg, fg), tab === "ascii" && /*#__PURE__*/React.createElement("textarea", {
    value: ascii,
    onChange: e => setAscii(e.target.value),
    placeholder: "Enter your ASCII art here...",
    style: {
      width: "100%",
      minHeight: 150,
      boxSizing: "border-box",
      padding: 12,
      borderRadius: 8,
      background: "rgb(255 255 255 / .04)",
      border: "1px solid var(--xc-border)",
      color: "var(--xc-foreground)",
      font: "400 13px/1.3 var(--jr-font-mono)",
      resize: "vertical"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      height: 1,
      background: "var(--xc-border)",
      margin: "24px 0"
    }
  }), /*#__PURE__*/React.createElement("button", {
    style: {
      width: "100%",
      height: 40,
      borderRadius: 8,
      border: "none",
      cursor: "pointer",
      background: "var(--xc-foreground)",
      color: "var(--xc-card)",
      font: "500 14px/1 var(--jr-font-ui)"
    }
  }, "Generate Custom XeLL Build"))), /*#__PURE__*/React.createElement("p", {
    style: {
      textAlign: "center",
      color: "var(--xc-muted-foreground)",
      font: "400 13px/1.4 var(--jr-font-ui)"
    }
  }, "XeLL Theme Customizer \u2014 Created by ", /*#__PURE__*/React.createElement("a", {
    href: "https://github.com/barrenechea",
    style: {
      color: "#60a5fa"
    }
  }, "barrenechea")));
}
Object.assign(window, {
  XellCustomizer
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/xell-customizer/XellCustomizer.jsx", error: String((e && e.message) || e) }); }

// xfall.js
try { (() => {
/* Falling X glyphs — a scaled-up, full-page version of UI/SnowfallBackground.cs.
   Same three depth tiers, same sway, same round-capped crossed strokes. */
(function () {
  const c = document.createElement("canvas");
  c.id = "xfall";
  document.body.appendChild(c);
  const ctx = c.getContext("2d");
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const TIERS = [{
    size: 5,
    colour: "#202025",
    w: 1
  }, {
    size: 8,
    colour: "#26262c",
    w: 1.4
  }, {
    size: 12,
    colour: "#2d2d34",
    w: 1.8
  }];
  let W = 0,
    H = 0,
    dpr = 1,
    flakes = [];
  let mult = {
    count: 1,
    speed: 1,
    scale: 1,
    fade: 1
  };
  function make(seeded) {
    const t = TIERS[Math.random() * 3 | 0];
    const tier = TIERS.indexOf(t);
    return {
      x: Math.random() * W,
      y: seeded ? Math.random() * H : -t.size * 2,
      speed: 0.6 + tier * 0.35 + Math.random() * 0.4,
      size: t.size,
      colour: t.colour,
      lw: t.w,
      drift: 6 + Math.random() * 16,
      phase: Math.random() * Math.PI * 2
    };
  }
  function resize() {
    dpr = Math.min(2, window.devicePixelRatio || 1);
    W = window.innerWidth;
    H = window.innerHeight;
    c.width = W * dpr;
    c.height = H * dpr;
    c.style.width = W + "px";
    c.style.height = H + "px";
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    const base = Math.max(40, Math.min(140, W * H / 12000 | 0));
    const count = Math.round(base * mult.count);
    while (flakes.length < count) flakes.push(make(true));
    flakes.length = count;
  }
  function draw() {
    ctx.clearRect(0, 0, W, H);
    ctx.lineCap = "round";
    for (const f of flakes) {
      if (!reduce) {
        f.y += f.speed * mult.speed;
        f.phase += 0.02 * mult.speed;
        if (f.y - f.size > H) Object.assign(f, make(false));
      }
      const x = f.x + Math.sin(f.phase) * f.drift,
        y = f.y,
        s = f.size * mult.scale;
      ctx.globalAlpha = mult.fade;
      ctx.strokeStyle = f.colour;
      ctx.lineWidth = f.lw * mult.scale;
      ctx.beginPath();
      ctx.moveTo(x - s, y - s);
      ctx.lineTo(x + s, y + s);
      ctx.moveTo(x + s, y - s);
      ctx.lineTo(x - s, y + s);
      ctx.stroke();
    }
    requestAnimationFrame(draw);
  }
  window.__xfall = {
    set(m) {
      mult = Object.assign({
        count: 1,
        speed: 1,
        scale: 1,
        fade: 1
      }, m);
      resize();
    }
  };
  window.addEventListener("resize", resize);
  resize();
  draw();
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "xfall.js", error: String((e && e.message) || e) }); }

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Checkbox = __ds_scope.Checkbox;

__ds_ns.NumberField = __ds_scope.NumberField;

__ds_ns.Radio = __ds_scope.Radio;

__ds_ns.Select = __ds_scope.Select;

__ds_ns.SplitButton = __ds_scope.SplitButton;

__ds_ns.TextField = __ds_scope.TextField;

__ds_ns.DataTable = __ds_scope.DataTable;

__ds_ns.DeviceCard = __ds_scope.DeviceCard;

__ds_ns.FlashOverlay = __ds_scope.FlashOverlay;

__ds_ns.LogConsole = __ds_scope.LogConsole;

__ds_ns.MessageDialog = __ds_scope.MessageDialog;

__ds_ns.ProgressBar = __ds_scope.ProgressBar;

__ds_ns.GroupBox = __ds_scope.GroupBox;

__ds_ns.MenuDropdown = __ds_scope.MenuDropdown;

__ds_ns.StatusBar = __ds_scope.StatusBar;

__ds_ns.Tabs = __ds_scope.Tabs;

__ds_ns.TitleBar = __ds_scope.TitleBar;

})();
