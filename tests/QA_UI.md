# UI QA

Run the web application at `http://localhost:5166` before this checklist.

## Interaction scroll invariant

For controls that update content on the current route, the viewport must not jump
to the top of the page. This applies to:

- catalog filters;
- faction detail tabs;
- `Show details`, `Show notes`, and hide actions in game catalogs;
- rules quick links.

Browser check:

```sh
agent-browser eval 'window.scrollTo(0, Math.min(900, document.body.scrollHeight / 2)); window.scrollY'
agent-browser click @CONTROL_REF
agent-browser wait 500
agent-browser eval 'window.scrollY'
```

The second value may change enough to reveal the selected target, but must not
become `0` unless the control intentionally navigates to another route.

## Smoke routes

- `/game`: compact overview introduction is visible; no local Overview navigation.
- `/game/cards`: card image is primary; all visible images have `naturalWidth > 0`.
- `/game/cards?type=action`: cards with imported component rulings expose a non-empty `Show notes` panel.
- `/game/technologies`: general and faction technologies are separate; images load.
- `/game/technologies`: technologies with imported component rulings expose a non-empty `Show notes` panel.
- `/game/technologies`: Biotic is the default; switching technology type updates both the active chip and visible cards without render errors.
- `/game/technologies`: empty HTML note containers do not produce `Show notes` buttons.
- `/game/board?type=anomaly`: switch system type away and back; the active chip and URL update without the Blazor error UI.
- `/game/factions?faction=TheArborec`: all seven tabs render content.
- `/game/factions?faction=TheGhostsOfCreuss&info=faq`: imported faction rulings render in the existing FAQ tab; the detailed index includes `Particle Synthesis` as a Breakthrough quick link.
- `/game/factions`, `/game/cards`, `/game/board`, and `/game/technologies`: source/set subchips are data-derived, preserve the primary filter, and do not auto-select a set.
- `/game/board`: Systems and Planets are two lenses over one tile-to-planet dataset; contextual taxonomy and collection controls remain within two visual levels.
- `/game/reference/54`: the legacy numeric URL renders without requesting `/api/rules`; its LRR authority is a linked badge in the document header.
- `/game/reference/breakthroughs`: the Thunder's Edge rule renders with its authority as a linked badge in the document header.
- `/game/reference`: alphabet links retain the route and scroll to the matching letter section.
- `/game/reference/51`: alphabet navigation remains visible and returns to the matching catalog section.
- `/game/components*`: no public route or navigation entry exists.
- `/rules/components*`: legacy links redirect to the canonical cards or technologies catalog.
- `/game/resources`: contains verified official documents only; resource type chips update the visible documents and query string.
- Legacy `/rules/*` library routes redirect with `replace` to their canonical `/game/*` routes.
- Global search: results load without a request to `/api/search/index`.
