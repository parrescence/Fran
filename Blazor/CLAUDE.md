# CLAUDE.md (Blazor)

Guidance for working in `Blazor/` specifically — the Blazor Razor Class Library
style-library within the `FactoryAspects` repo. See the root `CLAUDE.md` first for
repo-wide policy (branching, publishing, the multi-library layout); this file only
covers what's unique to this library.

## This library ships standalone — treat it that way

Built on the assumption it's consumed outside any one app — as a package (currently
via **GitHub Packages**, `https://nuget.pkg.github.com/bencalvin/index.json`, not
NuGet.org), or imported as source. `README.md` (this folder's) is the consumer-facing
high-level overview and is packed into the `.nupkg` itself (`<None Include=
"README.md" Pack="true" .../>` in `FactoryAspects.csproj`) — keep it accurate, not
just this file. Full step-by-step/reference detail lives under `docs/` instead (not
packed into the `.nupkg` — link to it from README with relative paths, not absolute
GitHub URLs, so each branch's README stays self-contained).

Consequences for any change here:

- **No app-specific coupling, ever.** No reference to any consuming app's project,
  type, or domain model, no `ProjectReference` to anything outside this repo. If a
  component needs data or behavior, it comes in via a `[Parameter]`/`EventCallback`,
  full stop.
- **No hardcoded brand/app defaults.** `BrandText` on `AppHeader`/`AppFooter`/
  `SidebarShell`/`StandardShell` is `[Parameter, EditorRequired]` with an empty-string
  default — every consumer supplies its own. Follow the same pattern for any new
  parameter that would otherwise bake in one consumer's branding/copy.
- **`PackageId` (`FactoryAspects.csproj`) is pinned to `FactoryAspects`, and
  `RootNamespace`/`AssemblyName` match it too.** Razor Class Library static assets are
  served at `_content/{PackageId}/...` — `theme.css`/`theme.js`/`sidebar.js` are
  referenced that way from every consumer's `index.html`. Renaming `PackageId`
  without updating every one of those references (in every consumer) silently 404s
  the CSS/JS. Component/type names (`FaButton`, `FaCard`, `FaToggle<TValue>`,
  `FaIcon`, ...) are a separate concern from the package/namespace identity — don't
  conflate "rename the package" with "rename a component."
- **`theme.js`/`sidebar.js` are plain vanilla JS, not Blazor JS interop** — IIFEs using
  only `localStorage`/`document.documentElement`, invoked via plain `onclick="..."`/
  `onchange="..."` HTML attributes (`ThemeSwitcher.cs`, `PaletteSwitcher.cs`,
  `AppSidebar.cs`), not `IJSRuntime.InvokeVoidAsync`. Deliberate: this is pure
  client-side UI state with nothing to keep in sync on the Blazor side. Keep new
  purely-visual client state in this style rather than wiring up JS interop for it.
- **RCL static assets aren't auto-injected into the host page.** Adding a new CSS/JS
  file here means `docs/install.md`'s wiring step (and every real consumer's
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
style consistent:

- **Stable ids are field initializers, generated once, never regenerated inside
  `BuildRenderTree`/`OnParametersSet`/any per-render path.** e.g. `private readonly
  string _id = $"fa-input-{Guid.NewGuid():N}";`. A `Guid.NewGuid()` called during
  render produces a new id on every re-render, which silently breaks anything that
  keys off that id — `@key` diffing, JS `getElementById` lookups, `<label for>`
  pairing.
- **Class-list building goes through `Internal.ClassNames.Combine(...)`**
  (`Internal/ClassNames.cs`) instead of ad hoc string concatenation — `ClassNames
  .Combine("fa-btn", VariantClass, Small ? "fa-btn-sm" : null, CssClass)`. Named
  `ClassNames`, not `CssClass` — most components already have a `CssClass` parameter,
  which would shadow a same-named type inside their own methods. Preserve each
  component's existing attribute-splat order when converting it (explicit attributes
  vs. `builder.AddMultipleAttributes(AdditionalAttributes)`) — don't silently change
  which one wins if a caller passes a conflicting `class` via `AdditionalAttributes`.
- **Swappable string/formatting behavior goes through an injected interface**
  (`[Inject] ISomeService`), not `new SomeHelper()` constructed inline inside the
  component — keeps it consumer-overridable and testable.
- **Vanilla-JS, not Blazor JS interop, for client-only visual state.** But check for a
  pure-Blazor answer first, since one often exists and needs no JS file at all:
  `FaDatePicker`'s calendar popup looks JS-shaped (open/close, outside-click-to-close,
  positioning) but ships with zero JavaScript — open/closed is a plain bool field, and
  "close when focus leaves the control" is a native `@onfocusout` + short
  grace-period delay instead of a document click listener reaching back into Blazor
  over JS interop. Only reach for a `wwwroot/js/<component>.js` IIFE when the
  behavior genuinely can't be expressed in Blazor's own event model.

## Publishing this library

Bump `<Version>` in `FactoryAspects.csproj` as part of normal `dev` work (semantic
`major.minor.hotfix`), on every change that reaches `main` — the patch (`z`) number by
default, `minor`/`major` only when a consumer explicitly calls for it. That version
rides unchanged through the `dev → test → main` promotion; don't bump it again at the
`test → main` step. Package versions are immutable once published (GitHub Packages
rejects re-publishing an existing version), so leaving `<Version>` unchanged across
several commits doesn't queue those changes up for consumers — it just means none of
them are reachable until the next bump. See the root `CLAUDE.md` for the branching
model and how `.github/workflows/publish.yml`/`ci.yml` build this project.

## Themes

Fourteen color palettes ship in the one `theme.css`, picked via `data-fa-palette` on
`<html>` — a second, independent axis from the existing light/dark/colorblind
`data-theme` mode switch, so every palette × mode combination needs its own dark-mode
block (`:root[data-fa-palette="X"][data-theme="dark"]`, plus the
`prefers-color-scheme` equivalent) rather than just a light-mode override. Colorblind
mode stays palette-agnostic on purpose (see its comment in `theme.css`) — one
known-safe accent/danger substitution reused across every palette, not fourteen
separate ones. The full palette list, and how a consumer picks one
(`<PaletteSwitcher>`, `window.faSetPalette(...)`, or a build-time attribute), is
documented in `docs/install.md`/`docs/palette-switcher.md` — don't duplicate that
detail here, just the two things a contributor actually needs: every palette needs
both mode blocks, and colorblind mode never gets a palette-specific variant.

Each palette's color choices are worked out first in `.themes/` in this folder — a
**gitignored**, local-only folder of Markdown design docs, not shipped in the package
and not committed. Once a palette is wired into `theme.css`, `.themes/`'s copy of it
is just historical design rationale, not the source of truth — `theme.css` is. Don't
assume `.themes/` exists when cloning fresh elsewhere.

## Components inventory

See `docs/index.md` — keep it in sync when adding/removing a component (this file for
contributor-facing rules, `docs/index.md` for the consumer-facing component list, each
entry linking to its own usage-example page under `docs/`). The README doesn't
duplicate the component list — it just points to `docs/index.md`.

`FaToggle<TValue>` (`Components/FaToggle.cs`) is the one component with real runtime
validation: it throws `ArgumentException` in `OnParametersSet` if fewer than two
`Options` are supplied. `Options` is a plain `IReadOnlyList<(string Title, TValue
Value)>` — a `System.ValueTuple`, deliberately not a custom DTO type, to avoid forcing
consumers to reference a FactoryAspects-specific model type just to build a list of
options. `FaRadioGroup<TValue>` mirrors the same Options-tuple shape.

`FaInput<TValue>`, `FaSelect<TValue>`, `FaTextarea`, `FaCheckbox`, `FaDatePicker`, and
`FaCurrency` are all `InputBase<TValue>`-derived (directly or via `InputTextArea`/
`InputCheckbox`) — they only work inside an `EditForm`/`EditContext`. Don't assume any
of these are exercised by a particular consumer just because they exist here — check
that consumer's own code for actual `EditForm` usage before relying on it.
