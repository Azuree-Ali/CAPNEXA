export interface PaletteContent {
  title: string;
  search: string;
  empty: string;
  close: string;
  shortcut: string;
}
export const palette: PaletteContent = {
  title: "Workspace search",
  search: "Find a page",
  empty: "No matching pages.",
  close: "Close",
  shortcut: "Search pages (Ctrl or Command + K)",
};
