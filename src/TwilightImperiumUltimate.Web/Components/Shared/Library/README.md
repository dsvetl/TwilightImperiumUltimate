# Library UI

This folder contains presentation primitives shared by feature areas. Components
must not fetch data, parse routes, or contain game-specific classification.

## Foundations

Global tokens live in `wwwroot/css/library-ui.css`.

- `--ti-space-*`: neutral space backgrounds.
- `--ti-surface*`: content surfaces and interaction states.
- `--ti-text*`: primary, secondary, and muted text.
- `--ti-blue`: interactive state and focus.
- `--ti-gold`: hierarchy and official-source emphasis.
- `--ti-border*`, `--ti-shadow`, `--ti-radius-*`: shared geometry.

## Components

- `LibraryShell`: responsive page width and skip link.
- `OverviewIntro`: compact introduction for overview pages.
- `GlobalSearch`: site-wide search opened from the top bar.
- `FilterChips`: compact single-value filters.
- `ContentState`: loading, empty, unavailable, and not-found states.

Feature components own their semantic cards and detail views. Promote a pattern
into this folder only after at least two features use the same behavior.

## Rules

1. Use tokens instead of local color literals.
2. Keep one `h1` per route.
3. Interactive targets must be at least 42px high.
4. Every async view distinguishes loading, empty, error, and success.
5. Mobile layouts must work at 320px without page-level horizontal overflow.
6. Respect `prefers-reduced-motion`.
