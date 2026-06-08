const previewStates = new WeakMap();

export function initializeFactionSheetPreviews(root) {
  if (!root || previewStates.has(root)) return;

  const previews = [...root.querySelectorAll("[data-sheet-preview]")];
  const finePointer = window.matchMedia("(hover: hover) and (pointer: fine)");
  const cleanups = [];
  let activePreview = null;
  let suppressedFocusPreview = null;

  const position = (preview) => {
    const trigger = preview.querySelector("[data-sheet-trigger]");
    const popover = preview.querySelector("[data-sheet-popover]");
    if (!trigger || !popover || !finePointer.matches) return;

    const triggerRect = trigger.getBoundingClientRect();
    const width = Math.min(420, window.innerWidth - 32);
    const left = Math.min(
      Math.max(16, triggerRect.left),
      window.innerWidth - width - 16,
    );
    const spaceBelow = window.innerHeight - triggerRect.bottom;
    const top =
      spaceBelow >= Math.min(520, window.innerHeight * 0.72)
        ? triggerRect.bottom + 10
        : Math.max(16, triggerRect.top - popover.offsetHeight - 10);

    preview.style.setProperty("--sheet-preview-left", `${left}px`);
    preview.style.setProperty("--sheet-preview-top", `${top}px`);
  };

  const hide = (preview, restoreFocus = false) => {
    if (!preview) return;

    preview.removeAttribute("data-visible");
    preview.removeAttribute("data-pinned");
    const trigger = preview.querySelector("[data-sheet-trigger]");
    trigger?.setAttribute("aria-expanded", "false");
    if (activePreview === preview) activePreview = null;
    if (restoreFocus) trigger?.focus();
  };

  const show = (preview, pinned = false) => {
    if (activePreview && activePreview !== preview) hide(activePreview);

    activePreview = preview;
    preview.toggleAttribute("data-pinned", pinned);
    preview.setAttribute("data-visible", "");
    preview
      .querySelector("[data-sheet-trigger]")
      ?.setAttribute("aria-expanded", "true");
    position(preview);
  };

  for (const preview of previews) {
    const trigger = preview.querySelector("[data-sheet-trigger]");
    if (!trigger) continue;

    const onPointerEnter = () => {
      if (finePointer.matches)
        show(preview, preview.hasAttribute("data-pinned"));
    };
    const onPointerLeave = () => {
      if (
        finePointer.matches &&
        !preview.hasAttribute("data-pinned") &&
        !preview.contains(document.activeElement)
      )
        hide(preview);
    };
    const onFocusIn = () => {
      if (suppressedFocusPreview === preview) {
        suppressedFocusPreview = null;
        return;
      }
      show(preview, preview.hasAttribute("data-pinned"));
    };
    const onFocusOut = () =>
      requestAnimationFrame(() => {
        if (
          !preview.hasAttribute("data-pinned") &&
          !preview.contains(document.activeElement)
        )
          hide(preview);
      });
    const onClick = (event) => {
      event.preventDefault();
      const pinned = !preview.hasAttribute("data-pinned");
      if (pinned) show(preview, true);
      else hide(preview);
    };

    preview.addEventListener("pointerenter", onPointerEnter);
    preview.addEventListener("pointerleave", onPointerLeave);
    preview.addEventListener("focusin", onFocusIn);
    preview.addEventListener("focusout", onFocusOut);
    trigger.addEventListener("click", onClick);
    cleanups.push(() => {
      preview.removeEventListener("pointerenter", onPointerEnter);
      preview.removeEventListener("pointerleave", onPointerLeave);
      preview.removeEventListener("focusin", onFocusIn);
      preview.removeEventListener("focusout", onFocusOut);
      trigger.removeEventListener("click", onClick);
    });
  }

  const onDocumentPointerDown = (event) => {
    if (activePreview && !activePreview.contains(event.target))
      hide(activePreview);
  };
  const onDocumentKeyDown = (event) => {
    if (event.key === "Escape" && activePreview) {
      event.preventDefault();
      suppressedFocusPreview = activePreview;
      hide(activePreview, true);
    }
  };
  const onResize = () => {
    if (activePreview) position(activePreview);
  };

  document.addEventListener("pointerdown", onDocumentPointerDown);
  document.addEventListener("keydown", onDocumentKeyDown);
  window.addEventListener("resize", onResize);
  cleanups.push(() => {
    document.removeEventListener("pointerdown", onDocumentPointerDown);
    document.removeEventListener("keydown", onDocumentKeyDown);
    window.removeEventListener("resize", onResize);
  });

  previewStates.set(root, cleanups);
}

export function disposeFactionSheetPreviews(root) {
  const cleanups = previewStates.get(root);
  if (!cleanups) return;

  for (const cleanup of cleanups) cleanup();
  previewStates.delete(root);
}
