import * as React from "react";
/** Menu / context-menu surface. Pass "-" in the items array for a separator. */
export interface MenuDropdownItem { label: string; submenu?: boolean }
export interface MenuDropdownProps {
  items?: Array<string | "-" | MenuDropdownItem>;
  onSelect?: (item: string | MenuDropdownItem) => void;
  style?: React.CSSProperties;
}
export declare function MenuDropdown(props: MenuDropdownProps): JSX.Element;
