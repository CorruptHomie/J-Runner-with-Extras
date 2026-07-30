import * as React from "react";
/**
 * Grid for CPU key database / bad-block / drive listings. Zebra rows, AccentDim selection.
 * @startingPoint section="Data" subtitle="Zebra-striped grid with accent selection" viewport="700x240"
 */
export interface DataTableColumn { key: string; label?: string; mono?: boolean }
export interface DataTableProps {
  columns?: Array<DataTableColumn | string>;
  rows?: Array<Record<string, React.ReactNode>>;
  selectedIndex?: number;
  onSelect?: (index: number) => void;
  style?: React.CSSProperties;
}
export declare function DataTable(props: DataTableProps): JSX.Element;
