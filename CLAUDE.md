# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in
this directory.

Read the root `CLAUDE.md` first for the overall architecture. This file covers what's
specific to FinanceApp.UI.

## This project ships standalone — treat it that way

FinanceApp.UI is a generic Blazor Razor Class Library, built on the assumption it will
eventually be consumed outside this repo (as a NuGet package, or imported as source),
not just via `FinanceApp.Web`'s `<ProjectReference>`. See `README.md` in this directory
for the consumer-facing docs (install steps, required manual asset wiring, component
inventory) — that file is packed into the `.nupkg` itself (`PackageReadmeFile`), so
keep it accurate, not just this CLAUDE.md.

Consequences for any change here:

- **No app-specific coupling, ever.** No `using FinanceApp.Shared`/`FinanceApp.Web`,
  no reference to `UserId`/`AccountId`/`Transaction`/`Category`/`Payer`/`Budget`/etc.,
  no `ProjectReference` to any other FinanceApp project. If a component needs data or
  behavior, it comes in via a `[Parameter]`/`EventCallback`, full stop. This has held
  since the project's first commit — keep it that way rather than reaching for "just
  this once" convenience access to app types.
- **No hardcoded brand/app defaults.** `BrandText` on `AppHeader`/`AppFooter`/
  `SidebarShell`/`StandardShell` is `[Parameter, EditorRequired]` with an empty-string
  default, not `= "FinanceApp"` — the consuming app always supplies its own (see
  `FinanceApp.Web`'s `MainLayout.razor`, which passes `BrandText="FinanceApp"`
  explicitly to `SidebarShell`). Follow the same pattern for any new parameter that
  would otherwise bake in FinanceApp's own branding/copy.
- **`PackageId` (`FinanceApp.UI.csproj`) is pinned to `FinanceApp.UI`.** Razor Class
  Library static assets are served at `_content/{PackageId}/...` — `theme.css`/
  `theme.js`/`sidebar.js` are referenced that way from `FinanceApp.Web/wwwroot/
  index.html`. Renaming `PackageId` without updating every one of those references
  (here and in any other consumer) silently 404s the CSS/JS with no obvious error.
- **`theme.js`/`sidebar.js` are plain vanilla JS, not Blazor JS interop** — IIFEs using
  only `localStorage`/`document.documentElement`, invoked via plain `onclick="..."`
  HTML attributes (`ThemeSwitcher.razor`, `AppSidebar.razor`), not
  `IJSRuntime.InvokeVoidAsync`. This is deliberate: collapsed/expanded and
  light/dark/colorblind are pure client-side UI state with nothing to keep in sync on
  the Blazor side. Keep new purely-visual client state in this style rather than
  wiring up JS interop for it.
- **RCL static assets aren't auto-injected into the host page.** Adding a new CSS/JS
  file here means the README's install snippet (and any real consumer's `index.html`)
  needs the corresponding `<link>`/`<script>` tag added by hand — `dotnet pack`
  bundles the file, it doesn't wire up the tag for you.
- **No license is set yet** (`FinanceApp.UI.csproj`'s `PackageLicenseExpression` is
  intentionally absent) — pick one before actually publishing a `.nupkg` anywhere a
  real external consumer would depend on it.

## Components inventory

See `README.md`'s "What's in here" section — keep both in sync when adding/removing a
component (this file for contributor-facing rules, the README for consumer-facing
docs).

`FaToggle<TValue>` (`Components/FaToggle.razor`) is the one component with real
runtime validation: it throws `ArgumentException` in `OnParametersSet` if fewer than
two `Options` are supplied. `Options` is a plain `IReadOnlyList<(string Title, TValue
Value)>` — a `System.ValueTuple`, deliberately not a custom DTO type, to avoid forcing
consumers to reference a FinanceApp.UI-specific model type just to build a list of
options.

`FaInput<TValue>`/`FaSelect<TValue>` are the only `InputBase<TValue>`-derived
components — they only work inside an `EditForm`/`EditContext`. Nothing in
`FinanceApp.Web` uses `EditForm` today (see that project's `CLAUDE.md`), so these two
are currently unexercised by the real app; don't assume they're wired into anything
just because they exist.
