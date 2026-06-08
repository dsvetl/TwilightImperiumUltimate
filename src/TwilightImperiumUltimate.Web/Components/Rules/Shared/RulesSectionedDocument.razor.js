export function buildSectionIndex(root, detailed = false) {
  const index = root?.querySelector("[data-section-index]");
  const content = root?.querySelector("[data-section-content]");
  if (!index || !content) return;

  const sections = new Map();
  for (const heading of content.querySelectorAll("h1")) {
    const type = heading
      .querySelector("sub")
      ?.textContent?.replace(/[()]/g, "")
      .trim();
    const title = heading.textContent
      ?.replace(heading.querySelector("sub")?.textContent || "", "")
      .trim();
    const label = type || (detailed ? "Abilities" : title) || "Section";
    const occurrence = (sections.get(label)?.occurrence || 0) + 1;
    heading.id = `rule-section-${slug(label)}-${occurrence}`;
    const section = sections.get(label) || {
      anchor: heading.id,
      items: [],
      occurrence: 0,
    };
    section.items.push({
      anchor: heading.id,
      title: title || label,
    });
    sections.set(label, {
      ...section,
      occurrence,
    });
  }

  const groups = [...sections].map(([label, section]) =>
    detailed
      ? createDetailedGroup(label, section.items)
      : createLink(label, section.anchor),
  );

  index.classList.toggle("section-index--detailed", detailed);
  index.replaceChildren(...groups);
  index.hidden = groups.length === 0;

  const fragment = decodeURIComponent(window.location.hash.slice(1));
  const target = fragment && document.getElementById(fragment);
  if (target && root.contains(target))
    requestAnimationFrame(() => target.scrollIntoView());
}

function createDetailedGroup(label, items) {
  const group = document.createElement("section");
  group.className = "section-index-group";

  const title = document.createElement("strong");
  title.textContent = label;
  group.append(title);

  const links = document.createElement("div");
  links.replaceChildren(
    ...items.map((item) => createLink(item.title, item.anchor)),
  );
  group.append(links);
  return group;
}

function createLink(label, anchor) {
  const link = document.createElement("a");
  link.href = `${window.location.pathname}${window.location.search}#${anchor}`;
  link.textContent = label;
  return link;
}

function slug(value) {
  return (
    value
      .normalize("NFKD")
      .replace(/[\u0300-\u036f]/g, "")
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-|-$/g, "") || "section"
  );
}
