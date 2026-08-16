# FaFa Showcase

A live, in-repo demo of `Blazor/` (and, over time, every other style library in this
repo) — every component rendered for real, with the actual markup used to produce
it. Not published anywhere yet: pull this branch and run it locally.

Two projects:

- **`Showcase.Web.Client`** — a standalone Blazor WebAssembly app. References
  `Blazor/FaFa.csproj` directly via `<ProjectReference>` (not the
  published GitHub Packages package), so it always shows whatever's currently on
  this branch, in-progress work included. A real consumer app outside this repo
  installs the package instead — see
  [`Blazor/docs/install.md`](../Blazor/docs/install.md).
- **`Showcase.Web.Api`** — a small local-only ASP.NET Core Web API (in-memory
  sample data, no auth, no real database) that backs the client's "pulled from the
  database" demos: `FaGrid`'s `ItemsProvider`, `FaCarousel`'s `ItemsProvider`, and
  `FaLoginForm`'s `OnSubmit`. Everything else on the client runs entirely
  client-side. **Not meant to be deployed as-is** — it's wide-open CORS and zero
  auth, purely a stand-in so the showcase has something real to call.

## Running it locally

Two terminals, both from this `Showcase/` folder:

```bash
cd Showcase.Web.Api
dotnet run --launch-profile https
```

```bash
cd Showcase.Web.Client
dotnet run --launch-profile https
```

Then open the client at **https://localhost:7095**. The API listens on
**https://localhost:7296** — `Showcase.Web.Client/wwwroot/appsettings.json`'s
`ApiBaseUrl` points there already, and the API's CORS policy already allows the
client's dev origin. Change both together if you run either on a different port.

Try the login form with `demo` / `password` (anything else fails on purpose — see
`Showcase.Web.Api/Program.cs`).

## Adding a page for a new component

Structured to mirror `Blazor/docs/` — one `.razor` page per component
(`Pages/FaButtonPage.razor` → route `/fa-button`, same slug as `Blazor/docs/fa-button.md`),
not one page per category. `Pages/Home.razor` is the category-grouped index, the
live-app equivalent of `Blazor/docs/index.md`; `Layout/NavMenu.razor` groups the same
links into collapsible sidebar sections. Adding a component's page means adding both
that page and its link in `Home.razor` and `NavMenu.razor`, matching whatever
category `Blazor/docs/index.md` puts it in.

Page files are suffixed `Page` (`FaButtonPage.razor`, not `FaButton.razor`) so the
page's own class name doesn't collide with the `<FaButton>` component tag it renders
— naming a page identically to a component it uses is a real `CS0104` ambiguous-
reference risk in Blazor, since both the page's own namespace and the imported
component namespace are in scope unqualified inside that file.

The `Palette` section (`Pages/Palette.razor` plus `Pages/Palette<Name>.razor`) is the
live-app equivalent of `Blazor/.themes/` (gitignored, local-only design docs there) —
`Pages/Palette.razor` mirrors that folder's `index.html` (a swatch-strip card grid,
one card per palette), and each `Pages/Palette<Name>.razor` mirrors its
`<slug>.html` (every `--fa-*` token as a labeled swatch, light mode then dark-mode
overrides). Purely a static reference display — it does not switch the app's own
palette; that's what the sidebar's `<FaPaletteSwitcher>` is for. `PaletteCatalog.cs`
holds every palette's `Light`/`Dark` token dictionaries as literal hex copies of
`_palettes.scss` (a live `var(--fa-*)` read only ever reflects whichever ONE palette
is active on `<html>`, so previewing all of them side by side needs its own copy).
Adding a twenty-fourth palette means adding an entry to `PaletteCatalog.cs` (copy its
`--fa-*` values from `_palettes.scss`) and a matching thin `Pages/Palette<Name>.razor`
that renders `<PaletteDetail Slug="..." />`.

## Once this gets a real deployment

An Azure Static Web App is planned for `Showcase.Web.Client` (with `Showcase.Web.Api`
as its linked API), but isn't wired up yet — no workflow, no `staticwebapp.config.json`.
Until then this only runs locally, and `Showcase.Web.Api` is intentionally not
production-hardened (see the CORS/auth note above) — harden it for real before it's
reachable from anywhere but localhost.
