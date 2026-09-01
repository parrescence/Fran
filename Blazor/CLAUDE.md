# CLAUDE.md (Blazor)

Guidance for working in `Blazor/` specifically — the Blazor Razor Class Library
style-library within the `FaFa` repo. See the root `CLAUDE.md` first for
repo-wide policy (branching, publishing, the multi-library layout); this file only
covers what's unique to this library.

## This library ships standalone — treat it that way

Built on the assumption it's consumed outside any one app — as a package (currently
via **GitHub Packages**, `https://nuget.pkg.github.com/parrescence/index.json`, not
NuGet.org), or imported as source. `README.md` (this folder's) is the consumer-facing
high-level overview and is packed into the `.nupkg` itself (`<None Include=
"README.md" Pack="true" .../>` in `FaFa.csproj`) — keep it accurate, not
just this file. Full step-by-step/reference detail lives under `docs/` instead (not
packed into the `.nupkg` — link to it from README with relative paths, not absolute
GitHub URLs, so each branch's README stays self-contained).

Consequences for any change here:

- **No app-specific coupling, ever.** No reference to any consuming app's project,
  type, or domain model, no `ProjectReference` to anything outside this repo. If a
  component needs data or behavior, it comes in via a `[Parameter]`/`EventCallback`,
  full stop.
- **No hardcoded brand/app defaults.** `BrandText` on `FaHeader`/`FaFooter`/
  `FaSidebarShell`/`FaStandardShell` is `[Parameter, EditorRequired]` with an empty-string
  default — every consumer supplies its own. Follow the same pattern for any new
  parameter that would otherwise bake in one consumer's branding/copy.
- **`PackageId` (`FaFa.csproj`) is pinned to `FaFa`, and
  `RootNamespace`/`AssemblyName` match it too.** Razor Class Library static assets are
  served at `_content/{PackageId}/...` — `fa-styles.css`/`theme.js`/`sidebar.js` are
  referenced that way from every consumer's `index.html`. Renaming `PackageId`
  without updating every one of those references (in every consumer) silently 404s
  the CSS/JS. Component/type names (`FaButton`, `FaCard`, `FaToggle<TValue>`,
  `FaIcon`, ...) are a separate concern from the package/namespace identity — don't
  conflate "rename the package" with "rename a component."
- **`theme.js`/`sidebar.js` are plain vanilla JS, not Blazor JS interop** — IIFEs using
  only `localStorage`/`document.documentElement`, invoked via plain `onclick="..."`/
  `onchange="..."` HTML attributes (`FaThemeSwitcher.cs`, `FaPaletteSwitcher.cs`,
  `FaSidebar.cs`), not `IJSRuntime.InvokeVoidAsync`. Deliberate: this is pure
  client-side UI state with nothing to keep in sync on the Blazor side. Keep new
  purely-visual client state in this style rather than wiring up JS interop for it.
- **RCL static assets aren't auto-injected into the host page.** Adding a new CSS/JS
  file here means `docs/install.md`'s wiring step (and every real consumer's
  `index.html`) needs the corresponding `<link>`/`<script>` tag added by hand —
  `dotnet pack` bundles the file, it doesn't wire up the tag for you.
- **No license is set yet** (`FaFa.csproj`'s `PackageLicenseExpression` is
  intentionally absent) — pick one before this is relied on by any consumer outside
  `parrescence`'s own accounts.

## Folder layout: components vs. supporting types

`Components/`, `Layout/`, `Templates/`, and `Icons/` hold only actual renderable components —
`ComponentBase`/`InputBase<TValue>` subclasses a consumer uses as a markup tag
(`<FaButton>`, `<FaHeader>`, `<FaIcon>`, ...). Supporting types a consumer references
in their own C# (not as a tag) live in two separate folders instead:

`Components/` is itself split into five subfolders, purely by what a component
*is* rather than what it does internally — same "physical organization only, not a
namespace change" rule as `Enums`/`Models` below applies here too, so this split was
a zero-risk, no-version-bump move:

- **`Components/Elements/`** — small, mostly-presentational building blocks:
  `FaButton`, `FaCard`, `FaBadge`, `FaAvatar`, `FaTabs`, `FaAccordion`,
  `FaBreadcrumb`, `FaPagination`, `FaDivider`, `FaChip`, `FaEmptyState`,
  `FaCodeBlock`.
- **`Components/Forms/`** — anything that collects or edits input, from a single
  `InputBase<TValue>` field up through a whole `<EditForm>`-wrapping composite:
  `FaInput`, `FaSelect`, `FaSearchSelect`, `FaDropdown`, `FaTextarea`, `FaCheckbox`,
  `FaRadioGroup`, `FaToggle`, `FaDate`, `FaDateRange`, `FaCurrency`, `FaFile`,
  `FaForm`, `FaLoginForm`, `FaLogoutForm`.
- **`Components/Feedback/`** — communicates state rather than taking input:
  `FaAlert`, `FaModal`, `FaProgress`, `FaSpinner`, `FaLoadingDots`, `FaHelixLoader`,
  `FaPongLoader`, `FaTooltip`, `FaPopover`, `FaToastHost`, `FaSkeleton`.
- **`Components/Data/`** — renders a collection: `FaTable`, `FaGrid`, `FaCarousel`.
- **`Components/Chrome/`** — app-shell controls, not page content:
  `FaThemeSwitcher`, `FaPaletteSwitcher`.

`Services/` is a sixth, sibling folder (not a `Components/` subfolder) for
non-component types that still ship as part of the public API but aren't
`ComponentBase` subclasses — today just `FaToastService`, a scoped injectable
service `FaToastHost` subscribes to (register it with
`services.AddScoped<FaToastService>()`; see `docs/fa-toast.md`). Namespace still
`FaFa.Components`, same physical-organization-only rule as everywhere else in this
section.

`Templates/` is a separate top-level folder, sibling to `Components`/`Layout`, with
its own `namespace FaFa.Templates` — not folded into `FaFa.Layout` even though every
template wraps `FaStandardShell`/`FaSidebarShell`, because the two are a different
kind of thing for a consumer to reach for: `Layout/` is the shell primitives
themselves (header/sidebar/footer, the two shells), `Templates/` is a handful of
full-page compositions built on top of them (`FaDashboardTemplate`,
`FaFormTemplate`, `FaHomeTemplate`, `FaAuthTemplate`) — a page-header row, a
centered form card, a hero band, a chrome-free auth card, respectively. Every
shell-level parameter a template exposes (brand/auth props, `Sidebar`,
`FooterContent`, and each bar's own `FaNavPosition`) is a straight pass-through
using the shell's own parameter names, so switching between the raw shell and a
template is a rename, not a rewrite. See `docs/page-templates.md` for the
consumer-facing how-to.

Every one of those still declares `namespace FaFa.Components;` regardless of which
subfolder it physically lives in — a consumer's existing `@using FaFa.Components`
keeps resolving every one of them unchanged. When adding a new component, place it
in whichever of the five subfolders matches its role; don't invent a sixth without
a reason (a component that only sort-of fits one of these belongs in the closest
match, not a new single-purpose folder).

Supporting types a consumer references in their own C# (not as a tag) live in two
more folders instead:

- **`Enums/`** — `FaButtonVariant`, `FaBadgeVariant`, `FaAlertVariant`,
  `FaTogglePosition`, `FaIconColor`, `FaIconName`, `FaSkeletonVariant`,
  `FaTooltipPosition` (shared by `FaTooltip` and `FaPopover`), `FaToastPosition`,
  `FaAlign`, `FaCodeLanguage`, `FaFormMode`, `FaLoaderVariant`, `FaModalPosition`,
  `FaModalSize`, `FaNavPosition`, `FaProgressDirection`.
- **`Models/`** — `FaDateRangeValue`, `FaGridColumn<TItem>`, `FaGridRequest`,
  `FaGridResult<TItem>`, `FaToastMessage`, `FaLoginRequest` (DTOs/records passed to
  or bound by a specific component).

Folder placement is purely physical organization — it does **not** change a type's
namespace. `FaButtonVariant` still declares `namespace FaFa.Components;`
even though the file lives in `Enums/`; `FaIconColor`/`FaIconName` still declare
`namespace FaFa.Icons;` even though the file lives in `Enums/` rather than
`Icons/`. This is deliberate: a consumer's existing `@using FaFa.Components`/
`.Icons` keeps resolving every type it always did — moving a file between these
folders is never a breaking API change and never needs a version bump for that reason
alone. When adding a new enum/DTO, put it in `Enums`/`Models` but keep its namespace
declaration matching whichever component/feature area it belongs to, not the new
folder name.

`Rendering/` is different from both: `internal` (not `public`) helper types that
support building a component's output — `CssClassNames`, `FaAlignClassNames`,
`CodeHighlighter` (backs `FaCodeBlock`'s syntax highlighting), and
`FaValidationMessageRenderer` (see Validation below) today, potentially other
HTML/CSS/JS-interop-support helpers later. Nothing in a consumer's own code can ever
reference an `internal` type, so — unlike `Enums`/`Models` above — `Rendering/`'s
namespace *does* match the folder (`namespace FaFa.Rendering;`) with no
breaking-change concern, since there's no public API surface to break.

## Component authoring: C# builder, not markup

Components are authored as plain C# — `ComponentBase`/`InputBase<TValue>` subclasses
overriding `BuildRenderTree(RenderTreeBuilder builder)` directly — not `.razor` markup
files. This applies to every component in `Components/`, `Layout/`, and `Icons/` —
not to the plain data types in `Enums/`/`Models/`, which have no render tree at all.
(`_Imports.razor` is project config, not a component, and stays.) Rules that keep this
style consistent:

- **Stable ids are field initializers, generated once, never regenerated inside
  `BuildRenderTree`/`OnParametersSet`/any per-render path.** e.g. `private readonly
  string _id = $"fa-input-{Guid.NewGuid():N}";`. A `Guid.NewGuid()` called during
  render produces a new id on every re-render, which silently breaks anything that
  keys off that id — `@key` diffing, JS `getElementById` lookups, `<label for>`
  pairing.
- **Class-list building goes through `Rendering.CssClassNames.Combine(...)`**
  (`Rendering/CssClassNames.cs`) instead of ad hoc string concatenation —
  `CssClassNames.Combine("fa-btn", VariantClass, Small ? "fa-btn-sm" : null,
  CssClass)`. Named `CssClassNames`, not plain `ClassNames` (ambiguous next to actual
  C# classes) or `CssClass` (most components already have a `CssClass` parameter,
  which would shadow a same-named type inside their own methods). Preserve each
  component's existing attribute-splat order when converting it (explicit attributes
  vs. `builder.AddMultipleAttributes(AdditionalAttributes)`) — don't silently change
  which one wins if a caller passes a conflicting `class` via `AdditionalAttributes`.
- **Swappable string/formatting behavior goes through an injected interface**
  (`[Inject] ISomeService`), not `new SomeHelper()` constructed inline inside the
  component — keeps it consumer-overridable and testable.
- **Vanilla-JS, not Blazor JS interop, for client-only visual state.** But check for a
  pure-Blazor answer first, since one often exists and needs no JS file at all:
  `FaDate`'s calendar popup looks JS-shaped (open/close, outside-click-to-close,
  positioning) but ships with zero JavaScript — open/closed is a plain bool field, and
  "close when focus leaves the control" is a native `@onfocusout` + short
  grace-period delay instead of a document click listener reaching back into Blazor
  over JS interop. Only reach for a `wwwroot/js/<component>.js` IIFE when the
  behavior genuinely can't be expressed in Blazor's own event model.

## Validation

`Validation/` (namespace `FaFa.Validation`) is a top-level folder, sibling to
`Components/`/`Services/`/`Rendering/`/`Templates/` — same reasoning as
`Templates/` earning its own folder (see above): this is a new *kind* of thing a
consumer implements against (a public API pattern, not a component, not an
internal render helper), not a fit for any of the five `Components/` subfolders.
The two renderable pieces (`FaModelValidator<TModel>`, `FaValidationMessage<TValue>`)
still live in `Components/Forms/` per the "Components/ = only actual renderable
tags" rule above.

Three tiers, most-specific wins, all feeding the same `EditContext`
`ValidationMessageStore` `DataAnnotationsValidator`/`ValidationSummary` already
read from — full detail in `docs/validation.md`:

1. **Root/DTO** — `IFaValidator<TModel>`, one implementation per model, registered
   via `AddFaValidator<TModel, TValidator>()`. Direct analogue of EF Core's
   `IEntityTypeConfiguration<TEntity>`.
2. **Form** — a `ConfigureValidation` delegate (on `FaModelValidator<TModel>`
   directly, or `FaForm<TModel>`'s own parameter of the same name) that runs after
   the root validator on the same `FaValidationBuilder<TModel>`.
3. **Element** — a `Validate` delegate parameter on `FaInput`/`FaSelect`/
   `FaTextarea`/`FaCheckbox`, evaluated independently every render and always
   additive to whatever the other two tiers already produced for that field.

Deliberately **not** a FluentValidation reimplementation — `FaValidationBuilder<TModel>
.Field<TValue>` takes a plain `Func<TModel, TValue>` accessor plus a
`nameof(...)`-string property key, not an `Expression<Func<T,TProp>>` a rule DSL
would need to parse. Keep any future addition to this system on the same "plain
delegates + string keys" side of that line rather than adding expression-tree
parsing later.

**`ShowValidationMessage` on every validatable input defaults to `true` (opt-out,
not opt-in).** Opt-in would leave the exact "have to remember it on every field"
gap that's the whole reason this system exists — before it, none of `FaInput`/
`FaSelect`/`FaTextarea`/`FaCheckbox`/`FaDate`/`FaCurrency` read `EditContext`/
`FieldIdentifier`/`ValidationMessageStore` at all, so no field ever showed its own
error. Any new `InputBase<TValue>`-derived component should follow the same
default, using `FaValidationMessageRenderer.Resolve`/`Render`
(`Rendering/FaValidationMessageRenderer.cs`) the same way the existing six do —
don't reinvent the inline-message rendering per component.

`FaDate.Min`/`Max` and `FaCurrency.Min`/`Max` are component *parameters*, not
bound model fields, so their own range check (`Max` before `Min`) is deliberately
**not** routed through `IFaValidator`/`FaModelValidator` — each checks its own two
parameters directly in `OnParametersSet` and renders the same
`.fa-validation-message` look. Shown, not thrown, unlike `FaToggle<TValue>`'s
`ArgumentException` precedent for a bad parameter combo — `Min`/`Max` are
plausibly still-loading runtime data, so a transient bad combination shouldn't
crash the render tree.

## Publishing this library

Bump `<Version>` in `FaFa.csproj` as part of normal `dev` work (semantic
`major.minor.patch`), on every change that reaches `main` — the patch (`z`) number by
default, `minor`/`major` only when a consumer explicitly calls for it. Each segment's
range: `major` counts from `1` upward with no ceiling (today's `0.x` line is the
conventional "pre-1.0, not yet stable" signal — stays `0.x` until a `1.0.0` is
deliberately decided, not bumped just to satisfy this range); `minor` and `patch`
each run `0`–`9999` before the next tier rolls over (`x.9999.9999` → `(x+1).0.0`,
`x.y.9999` → `x.(y+1).0`) — plenty of headroom that a version number is never the
reason to skip a real minor/major bump. That version rides unchanged through the
`dev → test → main` promotion; don't bump it again at the `test → main` step.
Package versions are immutable once published (GitHub Packages rejects
re-publishing an existing version), so leaving `<Version>` unchanged across several
commits doesn't queue those changes up for consumers — it just means none of them
are reachable until the next bump. See the root `CLAUDE.md` for the branching
model and how `.github/workflows/publish-blazor.yml`/`ci-blazor.yml` build this
project.

## Themes

`fa-styles.css` is **generated, not checked in** — edit
`wwwroot/css/fa-styles.scss`/`wwwroot/css/scss/*.scss` instead, never
`fa-styles.css` directly (it's gitignored; a stale hand-edit there just gets
silently overwritten on the next build). `DartSassBuilder` (a build-time
`PackageReference` in `FaFa.csproj`, not a global CLI tool — nothing extra
to install in CI) compiles `fa-styles.scss` to `fa-styles.css` on every `dotnet
build`/`dotnet pack`, so the compiled file always ends up at
`_content/FaFa/css/fa-styles.css` for consumers to link.

**DartSassBuilder's incremental-build cache only hashes `fa-styles.scss` itself, not
the partials it `@use`s** (`obj/Debug/net10.0/FaFa.csproj.DartSassBuilder.cache`) —
edit a `_<name>.scss` partial without touching `fa-styles.scss` and `dotnet build`
reports success while silently reusing the stale `fa-styles.css` from before your
edit. Easy to lose real time to: the C# side rebuilds fine, a running `dotnet run`
dev server keeps serving the old CSS, and nothing errors. If a CSS change isn't
showing up after a rebuild, delete that cache file (and `wwwroot/css/fa-styles.css`
for good measure) and rebuild — don't trust "Build succeeded" alone for a
partial-only change.

That compiled
*filename* is the thing that can't change again without breaking every consumer's
`<link>` — it was deliberately renamed once already (from an earlier `theme.css`,
before this file was split into partials) specifically so it wouldn't need to be
"theme" just because a theme/palette partial lives inside it; don't rename it again
without a matching migration note in the docs.

`wwwroot/css/scss/` holds one partial per component (`_buttons.scss`,
`_date.scss`, `_dropdown.scss`, ...), each named after — and scoped to — the
same section boundaries the pre-split stylesheet used to have as comment headers,
plus `_palettes.scss` (all twenty-eight palettes' color tokens — the actual "theme"
partial), `_base.scss`, `_layout.scss` (page shells/header/footer/sidebar/theme-
switcher chrome), `_utilities.scss`, and `_responsive.scss`. Partials are plain CSS
content split by component, not by CSS property (no separate "all borders" or "all
flexbox" file) — a component's full style stays in one file. They follow the
standard Sass partial convention (underscore-prefixed, never compiled to their own
`.css`); `fa-styles.scss` at the `wwwroot/css/` root `@use`s each one in source
order and is the only file `FaFa.csproj`'s explicit `<SassFile>` lists, so
a partial can never accidentally get compiled standalone. Adding a new component's
styles means adding its own `_name.scss` partial and one `@use` line in
`fa-styles.scss`, not appending to an existing partial.

**Splitting one file into many partials only stays safe if two things keep holding:**

- **No selector is ever defined in more than one partial**, except `_responsive.scss`'s
  `@media (max-width: 720px)` block intentionally re-declaring a handful of
  selectors to override specific properties on small screens — that's normal
  responsive cascading, not a conflict. Anything else with the same selector in two
  files means whichever partial `fa-styles.scss` `@use`s last silently wins, and the
  other partial's rule is dead code nobody will notice went stale. Before adding a
  selector, check it doesn't already exist elsewhere (`grep -rn ".fa-whatever {"
  wwwroot/css/scss/`).
- **A value that's genuinely a cross-component standard — not a per-component
  design choice — lives in exactly one `--fa-*` custom property, referenced with
  `var(...)` everywhere it's used, never repeated as a literal.** Colors, radii, and
  font already worked this way from the start (`_palettes.scss`'s `:root` block);
  `--fa-border-width` (`2px`), `--fa-transition-fast` (`0.15s ease`, hover/focus
  color changes), and `--fa-transition-medium` (`0.2s ease`, size/layout changes)
  were added the same way after an audit found those three literal values repeated
  verbatim 20+ times each across partials — changing "the standard border weight"
  now means editing one line in `_palettes.scss`, not hunting down every occurrence.
  **Padding/margin/gap values are deliberately NOT part of this** — every
  component's spacing is hand-tuned to that component, not drawn from a shared
  scale, so two components using different padding isn't drift to fix, it's the
  design. Only add a token for a value that's supposed to be identical everywhere
  it appears, not for reuse's own sake.

Twenty-eight color palettes live in `_palettes.scss`, picked via `data-fa-palette` on
`<html>` — a second, independent axis from the existing light/dark/colorblind
`data-theme` mode switch, so every palette × mode combination needs its own dark-mode
block (`:root[data-fa-palette="X"][data-theme="dark"]`, plus the
`prefers-color-scheme` equivalent) rather than just a light-mode override. Colorblind
mode stays palette-agnostic on purpose (see its comment in `_palettes.scss`) — one
known-safe accent/danger substitution reused across every palette, not twenty-eight
separate ones. The full palette list, and how a consumer picks one
(`<FaPaletteSwitcher>`, `window.faSetPalette(...)`, or a build-time attribute), is
documented in `docs/install.md`/`docs/palette-switcher.md` — don't duplicate that
detail here, just the two things a contributor actually needs: every palette needs
both mode blocks, and colorblind mode never gets a palette-specific variant.

Each palette's color choices are worked out first in `.themes/` in this folder — a
**gitignored**, local-only folder of Markdown design docs, not shipped in the package
and not committed. Once a palette is wired into `_palettes.scss`, `.themes/`'s copy
of it is just historical design rationale, not the source of truth —
`_palettes.scss` is. Don't assume `.themes/` exists when cloning fresh elsewhere.
**Adding or changing a palette still means updating `.themes/` in the same change**
when it does exist — a new `<slug>.md` + `<slug>.html` pair (copy an existing
palette's, e.g. `ruckus.md`/`ruckus.html`, as the template), plus a card in
`.themes/index.html` and a row in `.themes/README.md`'s table. Being gitignored makes
it invisible to a diff/PR review, which makes it easy to forget — it isn't optional
just because nothing enforces it.

## Components inventory

See `docs/index.md` — keep it in sync when adding/removing a component (this file for
contributor-facing rules, `docs/index.md` for the consumer-facing component list, each
entry linking to its own usage-example page under `docs/`). The README doesn't
duplicate the component list — it just points to `docs/index.md`.

**Any code change that's user-visible — a rename, a new/changed parameter, a
behavior change, a moved file — gets `docs/` (every page mentioning that component,
`docs/index.md`, `docs/site.html`) and `Showcase/` updated in the same change, not as
a follow-up.** A rename in particular touches more than the component's own doc page:
grep `docs/` and `Showcase/` for the old name before considering the change done —
stale examples/links that still reference it are as broken as a stale demo page (see
root [`CLAUDE.md`](../CLAUDE.md)'s Showcase-sync rule, which this extends to `docs/`).

`FaToggle<TValue>` (`Components/Forms/FaToggle.cs`) is the one component with real runtime
validation: it throws `ArgumentException` in `OnParametersSet` if fewer than two
`Options` are supplied. `Options` is a plain `IReadOnlyList<(string Title, TValue
Value)>` — a `System.ValueTuple`, deliberately not a custom DTO type, to avoid forcing
consumers to reference a FaFa-specific model type just to build a list of
options. `FaRadioGroup<TValue>` mirrors the same Options-tuple shape.

`FaInput<TValue>`, `FaSelect<TValue>`, `FaTextarea`, `FaCheckbox`, `FaDate`, and
`FaCurrency` are all `InputBase<TValue>`-derived (directly or via `InputTextArea`/
`InputCheckbox`) — they only work inside an `EditForm`/`EditContext`. Don't assume any
of these are exercised by a particular consumer just because they exist here — check
that consumer's own code for actual `EditForm` usage before relying on it.
