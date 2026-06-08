# Rules data import

`wwwroot/data/rules-reference.json` is generated data. Do not edit it by hand.

## Sources

- Official FFG documents define the authoritative rules and expansion version.
- `yjmrobert/tirules` provides the maintained, machine-readable Markdown
  compilation used by the importer. It is GPL-3.0 and is pinned by commit and
  archive SHA-256 in `sources.json`.
- `legacy/deployed-rule-id-map.json` is a snapshot of this project's deployed
  API. It preserves numeric IDs used by existing `/game/reference/<id>` links
  and contains no rule text; it is not an external upstream.

The upstream archive is downloaded into `tools/rules/.cache/` and is not mixed
with application source. Only the normalized generated JSON is shipped.

## Update

```bash
node tools/rules/update.mjs
```

The command downloads the exact pinned archive, verifies its SHA-256, imports
every rule, faction, and component Markdown document, checks that no legacy
topic disappeared, and writes:

```text
src/TwilightImperiumUltimate.Web/wwwroot/data/rules-reference.json
src/TwilightImperiumUltimate.Web/wwwroot/data/faction-rulings.json
src/TwilightImperiumUltimate.Web/wwwroot/data/component-rulings.json
```

To refresh the legacy ID snapshot from the deployed API:

```bash
curl -fsS https://api.ti4ultimate.com/api/rules \
  -o tools/rules/legacy/deployed-rule-id-map.json
node tools/rules/update.mjs
```

To adopt a newer upstream revision:

1. Review upstream changes and its license.
2. Update `revision`, `archiveUrl`, and `archiveSha256` in `sources.json`.
3. Run the importer.
4. Review the generated diff, especially newly added or removed topics.
5. Run the Web build and UI QA for `/game/reference`.

The importer intentionally fails if the upstream archive checksum differs or
if a legacy API category cannot be mapped. This prevents silent partial
updates such as omitting Thunder's Edge topics from an enum-bound catalog.
