# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in
this repository.

This is `FactoryAspects` — a standalone Blazor Razor Class Library, split out of the
[FinanceApp](https://github.com/bencalvin/FinanceApp) monorepo (full history carried
over via `git subtree split`) into its own repo/package. There is no root/monorepo
CLAUDE.md above this one to read first; this file is the whole picture.

## This project ships standalone — treat it that way

Built on the assumption it's consumed outside any one app — as a package (currently
via **GitHub Packages**, `https://nuget.pkg.github.com/bencalvin/index.json`, not
NuGet.org), or imported as source. See `README.md` for the consumer-facing docs
(install steps, required manual asset wiring, component inventory) — that file is
packed into the `.nupkg` itself (`PackageReadmeFile`), so keep it accurate, not just
this CLAUDE.md.

Consequences for any change here:

- **No app-specific coupling, ever.** No reference to any consuming app's project or
  type (e.g. FinanceApp's `FinanceApp.Shared`/`.Web`, or concepts like `UserId`/
  `AccountId`/`Transaction`/`Category`/`Payer`/`Budget`), no `ProjectReference` to
  anything outside this repo. If a component needs data or behavior, it comes in via
  a `[Parameter]`/`EventCallback`, full stop. This has held since the project existed
  (originally as `FinanceApp.UI` inside the FinanceApp monorepo) — keep it that way
  rather than reaching for "just this once" convenience access to some consumer's
  types.
- **No hardcoded brand/app defaults.** `BrandText` on `AppHeader`/`AppFooter`/
  `SidebarShell`/`StandardShell` is `[Parameter, EditorRequired]` with an empty-string
  default — every consumer supplies its own (FinanceApp.Web's `MainLayout.razor`
  passes `BrandText="FinanceApp"` explicitly to `SidebarShell`, for example). Follow
  the same pattern for any new parameter that would otherwise bake in one consumer's
  branding/copy.
- **`PackageId` (`FactoryAspects.csproj`) is pinned to `FactoryAspects`, and
  `RootNamespace`/`AssemblyName` match it too.** Razor Class Library static assets are
  served at `_content/{PackageId}/...` — `theme.css`/`theme.js`/`sidebar.js` are
  referenced that way from every consumer's `index.html`. Renaming `PackageId`
  without updating every one of those references (in every consumer) silently 404s
  the CSS/JS with no obvious error. Component/type names themselves (`FaButton`,
  `FaCard`, `FaToggle<TValue>`, `FaIcon`, ...) are a separate concern from the
  package/namespace identity — don't conflate "rename the package" with "rename a
  component," they're independent decisions.
- **`theme.js`/`sidebar.js` are plain vanilla JS, not Blazor JS interop** — IIFEs using
  only `localStorage`/`document.documentElement`, invoked via plain `onclick="..."`
  HTML attributes (`ThemeSwitcher.cs`, `AppSidebar.cs`), not
  `IJSRuntime.InvokeVoidAsync`. This is deliberate: collapsed/expanded and
  light/dark/colorblind are pure client-side UI state with nothing to keep in sync on
  the Blazor side. Keep new purely-visual client state in this style rather than
  wiring up JS interop for it.
- **RCL static assets aren't auto-injected into the host page.** Adding a new CSS/JS
  file here means the README's install snippet (and every real consumer's
  `index.html`) needs the corresponding `<link>`/`<script>` tag added by hand —
  `dotnet pack` bundles the file, it doesn't wire up the tag for you.
- **No license is set yet** (`FactoryAspects.csproj`'s `PackageLicenseExpression` is
  intentionally absent) — pick one before this is relied on by any consumer outside
  `bencalvin`'s own accounts.

## Component authoring: C# builder, not markup

Components are authored as plain C# — `ComponentBase`/`InputBase<TValue>` subclasses
overriding `BuildRenderTree(RenderTreeBuilder builder)` directly — not `.razor` markup
files. This applies to every component in `Components/`, `Layout/`, and `Icons/`.
(`_Imports.razor` is project config, not a component, and stays.) Rules that keep this
style from degrading into the mess it was ported out of:

- **Stable ids are field initializers, generated once, never regenerated inside
  `BuildRenderTree`/`OnParametersSet`/any per-render path.** e.g. `private readonly
  string _id = $"fa-input-{Guid.NewGuid():N}";`. A `Guid.NewGuid()` called during
  render produces a new id on every re-render, which silently breaks anything that
  keys off that id — `@key` diffing, JS `getElementById` lookups, `<label for>`
  pairing. This was the single most common bug in the two source libraries this
  project's components were ported/rewritten from — don't reintroduce it.
- **Class-list building goes through `Internal.ClassNames.Combine(...)`**
  (`Internal/ClassNames.cs`) instead of ad hoc string concatenation repeated in every
  component — `ClassNames.Combine("fa-btn", VariantClass, Small ? "fa-btn-sm" : null,
  CssClass)`. Named `ClassNames`, not `CssClass` — most components have a `CssClass`
  parameter, which would shadow a same-named type inside their own methods. Preserve
  each component's existing attribute-splat order when converting
  it (explicit attributes vs. `builder.AddMultipleAttributes(AdditionalAttributes)`) —
  don't silently change which one wins if a caller passes a conflicting `class` via
  `AdditionalAttributes`.
- **Swappable string/formatting behavior goes through an injected interface**
  (`[Inject] ISomeService`), not `new SomeHelper()` constructed inline inside the
  component — keeps it consumer-overridable and testable.
- **Vanilla-JS, not Blazor JS interop, for client-only visual state** — same rule as
  `theme.js`/`sidebar.js` above: plain IIFEs wired via `onclick`/data attributes, no
  `IJSRuntime.InvokeVoidAsync`/`[JSInvokable]`. But check for a pure-Blazor answer
  first, since one often exists and needs no JS file at all: `FaDatePicker`'s calendar
  popup looks JS-shaped (open/close, outside-click-to-close, positioning) but ships
  with zero JavaScript — open/closed is a plain bool field, and "close when focus
  leaves the control" is a native `@onfocusout` + short grace-period delay (so a
  `focusin` on a sibling field inside the same control cancels the pending close)
  instead of a document click listener reaching back into Blazor over JS interop. Only
  reach for a `wwwroot/js/<component>.js` IIFE when the behavior genuinely can't be
  expressed in Blazor's own event model.

## Branching: dev → test → main

Three long-lived branches, one direction of flow:

- **`dev`** — the default branch on GitHub, and where day-to-day work happens.
  Feature branches merge here first.
- **`test`** — a gate before `main`. Only reachable via a PR from `dev`.
- **`main`** — what consumers actually pull the package from. Only reachable via a
  PR from `test`. Nothing lands here directly.

`test` and `main` are both branch-protected: no direct pushes, a PR is required, and
the `build-and-pack` CI check (`.github/workflows/ci.yml`) must pass before merging.
Required approving reviews are set to 0 (solo maintainer today) — so a green CI check
is what actually gates the merge, not a second pair of eyes. If collaborators join,
raise `required_approving_review_count` on both branches' protection rules.

## Publishing

Bump `<Version>` in `FactoryAspects.csproj` as part of normal `dev` work (semantic
`major.minor.hotfix`, e.g. `0.1.0`) — that version rides unchanged through the
`dev → test → main` promotion; don't bump it again at the `test → main` step.

**Bump on every change that reaches `main`, not just at the end of a batch of work** —
the patch (`z`) number by default (`0.3.0` → `0.3.1` → `0.3.2` → ...), `minor`/`major`
only when a consumer explicitly calls for it. Package versions are immutable once
published (GitHub Packages rejects re-publishing an existing version, and the release
tag below never gets re-pointed), so leaving `<Version>` unchanged across several
commits doesn't "queue up" those changes for consumers — it just means none of them are
reachable at all until the next bump, since the already-published version's contents
can never change. Consumers pin to an exact version and opt into a new one explicitly
(editing their own `<PackageReference>`/`<Version>`) — nothing updates for them
silently, on any restore, no matter how the version policy here is run.

`.github/workflows/publish.yml` packs and pushes to GitHub Packages, but **only on
push to `main`**, using the workflow's own `GITHUB_TOKEN` (`permissions: packages:
write`) — no manual PAT needed. `dev` and `test` never publish a package; GitHub
Packages rejects re-publishing an existing version number, which is the real backstop
against forgetting to bump `<Version>` before a `test → main` merge.
`.github/workflows/ci.yml` builds + packs (no publish) on every push/PR to `dev`,
`test`, and `main` as a sanity check.

After a successful publish, the same workflow also stamps a `vX.Y.Z` git tag (matching
`<Version>`) on the `main` commit that shipped it — skipped if that tag already exists,
never re-pointed once it does. This is the pinnable target for anything consuming this
repo as source rather than as a package (a git submodule, say): check out the tag
instead of tracking `main`'s moving tip, and `vX.Y.Z` is guaranteed to always mean
exactly the code that produced that package version. `v0.3.0` was back-filled by hand
onto the pre-existing `main` tip it corresponds to since it predates this tagging step;
every release from here on gets tagged automatically.

## Themes

Fourteen palettes, all in the one `theme.css` (settled: not separate stylesheets per
theme), picked via `data-fa-palette` on `<html>` — `northwest-fall` (default, no
attribute needed), `southwest-summer`, `northeast-spring`, `midwest-winter`,
`southeast-beach`, `greece-aegean`, `spain-flamenco`, `ireland-emerald`,
`jamaica-blue-mountain`, `japan-indigo`, `korea-celadon`, `china-cinnabar`,
`india-peacock`, `cameroon-rainforest`. This is a second, independent axis from the
existing light/dark/colorblind `data-theme` mode switch — every palette × mode
combination has to work, which is why each palette needs its own dark-mode block
(`:root[data-fa-palette="X"][data-theme="dark"]`, plus the `prefers-color-scheme`
equivalent) rather than just a light-mode override. Colorblind mode stays
palette-agnostic on purpose (see its comment in `theme.css`) — one known-safe
accent/danger substitution reused across every palette, not fourteen separate ones.

`js/theme.js`'s `window.faSetPalette(name)` mirrors `window.faSetTheme(...)`:
persists to `localStorage` (`fa-palette` key) and stamps/removes the attribute. No
bundled `<PaletteSwitcher>` component exists yet — README's "Choosing a theme" covers
both the build-time (hardcode the attribute) and runtime (call `faSetPalette`) paths a
consumer has today.

Each palette's color choices are worked out first in `.themes/` at the repo root — a
**gitignored**, local-only folder of Markdown design docs (one file per theme, a
`--fa-*` variable → hex table each), not shipped in the package and not committed. Once
a palette is wired into `theme.css` (all fourteen are, as of this writing), `.themes/`'s
copy of that palette is just historical design rationale, not the source of truth —
`theme.css` is. Don't assume `.themes/` exists when cloning fresh elsewhere; it's local
reference material, regenerate it (or ask) rather than expecting it to already be
there, and don't treat its absence as a sign a palette isn't real — check `theme.css`.

## Components inventory

See `README.md`'s "What's in here" section — keep both in sync when adding/removing a
component (this file for contributor-facing rules, the README for consumer-facing
docs).

`FaToggle<TValue>` (`Components/FaToggle.cs`) is the one component with real runtime
validation: it throws `ArgumentException` in `OnParametersSet` if fewer than two
`Options` are supplied. `Options` is a plain `IReadOnlyList<(string Title, TValue
Value)>` — a `System.ValueTuple`, deliberately not a custom DTO type, to avoid forcing
consumers to reference a FactoryAspects-specific model type just to build a list of
options. `FaRadioGroup<TValue>` mirrors the same Options-tuple shape.

`FaInput<TValue>`, `FaSelect<TValue>`, `FaTextarea`, `FaCheckbox`, `FaDatePicker`, and
`FaCurrency` are all `InputBase<TValue>`-derived (directly or via `InputTextArea`/
`InputCheckbox`) — they only work inside an `EditForm`/`EditContext`. As of the split
from FinanceApp, nothing in that example consumer used `EditForm`, so none of these
were exercised there — don't assume any of them are wired into any particular consumer
just because they exist here.
