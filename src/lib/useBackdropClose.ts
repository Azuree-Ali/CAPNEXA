import { useEffect } from "react";

/**
 * Closes any open <dialog data-backdrop-close> when the user clicks outside its box.
 * Opt-in on purpose: edit forms are not given the attribute, so a stray click cannot discard input.
 */
export function useBackdropClose() {
  useEffect(() => {
    function onClick(event: MouseEvent) {
      const dialog = event.target;
      if (!(dialog instanceof HTMLDialogElement) || !dialog.open) return;
      if (!dialog.hasAttribute("data-backdrop-close")) return;
      const box = dialog.getBoundingClientRect();
      const inside =
        event.clientX >= box.left &&
        event.clientX <= box.right &&
        event.clientY >= box.top &&
        event.clientY <= box.bottom;
      if (!inside) dialog.close();
    }
    document.addEventListener("click", onClick);
    return () => document.removeEventListener("click", onClick);
  }, []);
}
