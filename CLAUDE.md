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
  HTML attributes (`ThemeSwitcher.razor`, `AppSidebar.razor`), not
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

## Publishing

`.github/workflows/publish.yml` packs and pushes to GitHub Packages on every push to
`main`, using the workflow's own `GITHUB_TOKEN` (`permissions: packages: write`) — no
manual PAT needed to publish. Bump `<Version>` in `FactoryAspects.csproj` before a
push that should actually ship a new version; GitHub Packages rejects re-publishing an
existing version number. `.github/workflows/ci.yml` builds + packs (no publish) on
every push/PR as a sanity check.

## Components inventory

See `README.md`'s "What's in here" section — keep both in sync when adding/removing a
component (this file for contributor-facing rules, the README for consumer-facing
docs).

`FaToggle<TValue>` (`Components/FaToggle.razor`) is the one component with real
runtime validation: it throws `ArgumentException` in `OnParametersSet` if fewer than
two `Options` are supplied. `Options` is a plain `IReadOnlyList<(string Title, TValue
Value)>` — a `System.ValueTuple`, deliberately not a custom DTO type, to avoid forcing
consumers to reference a FactoryAspects-specific model type just to build a list of
options.

`FaInput<TValue>`/`FaSelect<TValue>` are the only `InputBase<TValue>`-derived
components — they only work inside an `EditForm`/`EditContext`. As of the split from
FinanceApp, nothing in that example consumer used `EditForm`, so these two were
unexercised there — don't assume they're wired into any particular consumer just
because they exist here.
