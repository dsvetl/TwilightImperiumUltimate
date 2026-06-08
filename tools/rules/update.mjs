import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { existsSync } from "node:fs";
import {
  mkdtemp,
  mkdir,
  readFile,
  readdir,
  rm,
  writeFile,
} from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, join, resolve } from "node:path";

const root = resolve(import.meta.dirname, "../..");
const toolDir = resolve(root, "tools/rules");
const sourcesPath = join(toolDir, "sources.json");
const legacyPath = join(toolDir, "legacy/deployed-rule-id-map.json");
const outputPath = join(
  root,
  "src/TwilightImperiumUltimate.Web/wwwroot/data/rules-reference.json",
);
const factionOutputPath = join(
  root,
  "src/TwilightImperiumUltimate.Web/wwwroot/data/faction-rulings.json",
);
const componentOutputPath = join(
  root,
  "src/TwilightImperiumUltimate.Web/wwwroot/data/component-rulings.json",
);
const cacheDir = join(toolDir, ".cache");

const manifest = JSON.parse(await readFile(sourcesPath, "utf8"));
const source = manifest.sources.find((item) => item.id === "tirules");
if (!source)
  throw new Error("sources.json does not define the tirules source.");

await mkdir(cacheDir, { recursive: true });
const archivePath = join(cacheDir, `tirules-${source.revision}.tar.gz`);
if (!existsSync(archivePath)) await download(source.archiveUrl, archivePath);
await verifySha256(archivePath, source.archiveSha256);

const tempDir = await mkdtemp(join(tmpdir(), "ti4-rules-"));
try {
  execFileSync("tar", [
    "-xzf",
    archivePath,
    "-C",
    tempDir,
    "--strip-components=1",
  ]);

  const legacyResponse = JSON.parse(await readFile(legacyPath, "utf8"));
  const legacyRules = legacyResponse?.data?.items ?? [];
  const legacyByKey = new Map(
    legacyRules.map((rule) => [categoryKey(rule.ruleCategory), rule]),
  );

  const rulesDir = join(tempDir, source.contentPath);
  const files = (await readdir(rulesDir))
    .filter((file) => /^R_.+\.md$/.test(file))
    .sort();
  const rules = [];

  for (const file of files) {
    const markdown = await readFile(join(rulesDir, file), "utf8");
    const rule = parseRule(file, markdown);
    const legacy = legacyByKey.get(rule.key);
    if (legacy) {
      rule.legacyId = legacy.id;
      rule.legacyCategory = legacy.ruleCategory;
      legacyByKey.delete(rule.key);
    }

    rule.sourceIds = ["tirules"];
    rule.authoritySourceIds = manifest.thundersEdgeRuleKeys.includes(rule.key)
      ? ["ffg-thunders-edge"]
      : ["ffg-lrr-2.0"];
    rules.push(rule);
  }

  if (legacyByKey.size > 0) {
    throw new Error(
      `Upstream is missing legacy categories: ${[...legacyByKey.values()]
        .map((rule) => rule.ruleCategory)
        .join(", ")}`,
    );
  }

  const keys = new Set(rules.map((rule) => rule.key));
  for (const key of manifest.thundersEdgeRuleKeys) {
    if (!keys.has(key))
      throw new Error(`Thunder's Edge topic '${key}' is missing upstream.`);
  }

  const output = {
    schemaVersion: 1,
    sourceRevision: source.revision,
    sources: manifest.sources,
    rules: rules.sort((left, right) => left.title.localeCompare(right.title)),
  };
  await writeFile(outputPath, `${JSON.stringify(output, null, 2)}\n`);

  const factions = await parseDocuments(
    join(tempDir, "astro-site/src/content/docs/factions"),
    /^F_.+\.md$/,
    "faction",
  );
  const components = await parseDocuments(
    join(tempDir, "astro-site/src/content/docs/components"),
    /^C_.+\.md$/,
    "component",
  );
  await writeFile(
    factionOutputPath,
    `${JSON.stringify(generatedCatalog(source, factions), null, 2)}\n`,
  );
  await writeFile(
    componentOutputPath,
    `${JSON.stringify(generatedCatalog(source, components), null, 2)}\n`,
  );
  console.log(
    `Imported ${rules.length} rules, ${factions.length} factions, and ${components.length} component catalogs.`,
  );
} finally {
  await rm(tempDir, { recursive: true, force: true });
}

async function download(url, destination) {
  const response = await fetch(url, { redirect: "follow" });
  if (!response.ok)
    throw new Error(`Failed to download ${url}: HTTP ${response.status}`);
  await writeFile(destination, Buffer.from(await response.arrayBuffer()));
}

async function verifySha256(path, expected) {
  const actual = createHash("sha256")
    .update(await readFile(path))
    .digest("hex");
  if (actual !== expected)
    throw new Error(`Checksum mismatch for ${basename(path)}: ${actual}`);
}

function parseRule(file, markdown) {
  const normalized = markdown.replaceAll("\r\n", "\n");
  const title = normalized.match(/^title:\s*(.+)$/m)?.[1]?.trim();
  if (!title) throw new Error(`${file} has no frontmatter title.`);

  const rulesMarkdown = section(normalized, "Rules Reference");
  const notesMarkdown = section(normalized, "Notes");
  const relatedMarkdown = section(normalized, "Related Topics");
  return {
    key: file.slice(2, -3).replaceAll("_", "-"),
    title,
    contentHtml: markdownToHtml(rulesMarkdown),
    notesHtml: markdownToHtml(notesMarkdown),
    relatedKeys: [...relatedMarkdown.matchAll(/\]\(\.\.\/r_([^)]+)\)/g)].map(
      (match) => match[1].replaceAll("_", "-"),
    ),
  };
}

async function parseDocuments(directory, pattern, kind) {
  const files = (await readdir(directory))
    .filter((file) => pattern.test(file))
    .sort();
  return Promise.all(
    files.map(async (file) => {
      const markdown = await readFile(join(directory, file), "utf8");
      const normalized = markdown.replaceAll("\r\n", "\n");
      const title = normalized.match(/^title:\s*(.+)$/m)?.[1]?.trim();
      if (!title) throw new Error(`${file} has no frontmatter title.`);

      const body = normalized.replace(/^---[\s\S]*?---\s*/, "");
      const sections = [
        ...body.matchAll(/^##\s+(.+?)\s*$([\s\S]*?)(?=^##\s+|(?![\s\S]))/gm),
      ]
        .map((match) => ({
          title: match[1].replace(/<\/?sub>/g, "").trim(),
          html: markdownToHtml(match[2].trim()),
        }))
        .filter((section) => section.html);
      return {
        key: file.slice(2, -3).replaceAll("_", "-"),
        kind,
        title,
        sections,
        sourceId: "tirules",
        sourcePath: `${kind === "faction" ? "factions" : "components"}/${file}`,
      };
    }),
  );
}

function generatedCatalog(source, items) {
  return {
    schemaVersion: 1,
    sourceRevision: source.revision,
    source: {
      id: source.id,
      repository: source.repository,
      revision: source.revision,
      license: source.license,
    },
    items,
  };
}

function section(markdown, heading) {
  const marker = `## ${heading}`;
  const start = markdown.indexOf(marker);
  if (start < 0) return "";
  const contentStart = start + marker.length;
  const next = markdown.indexOf("\n## ", contentStart);
  return markdown.slice(contentStart, next < 0 ? undefined : next).trim();
}

function markdownToHtml(markdown) {
  if (!markdown) return "";

  const lines = markdown.split("\n");
  const output = [];
  const lists = [];
  let paragraph = [];

  const closeParagraph = () => {
    if (paragraph.length === 0) return;
    output.push(`<p>${inline(paragraph.join(" "))}</p>`);
    paragraph = [];
  };
  const closeLists = (depth = 0) => {
    while (lists.length > depth) output.push(`</${lists.pop()}>`);
  };

  for (const rawLine of lines) {
    const line = rawLine.trimEnd();
    if (!line.trim()) {
      closeParagraph();
      continue;
    }

    const match = line.match(/^(\s*)(\d+\.|-)\s+(.+)$/);
    if (match) {
      closeParagraph();
      const depth = Math.floor(match[1].length / 4);
      const type = match[2] === "-" ? "ul" : "ol";
      while (lists.length > depth + 1) output.push(`</${lists.pop()}>`);
      if (lists.length === depth + 1 && lists.at(-1) !== type)
        output.push(`</${lists.pop()}>`);
      while (lists.length < depth + 1) {
        lists.push(type);
        output.push(`<${type}${type === "ol" ? ' class="lrr"' : ""}>`);
      }
      output.push(`<li>${inline(match[3])}</li>`);
      continue;
    }

    closeLists();
    paragraph.push(line.trim());
  }

  closeParagraph();
  closeLists();
  return output.join("\n");
}

function inline(value) {
  return value
    .replace(/\[([^\]]+)\]\([^)]+\)/g, "$1")
    .replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>")
    .replace(/\*([^*]+)\*/g, "<em>$1</em>");
}

function categoryKey(value) {
  const aliases = {
    TechnologySystem: "technology-sc",
  };
  if (aliases[value]) return aliases[value];
  return value.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase();
}
