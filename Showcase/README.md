# FactoryAspects Showcase

A live, in-repo demo of `Blazor/` (and, over time, every other style library in this
repo) — every component rendered for real, with the actual markup used to produce
it. Not published anywhere yet: pull this branch and run it locally.

Two projects:

- **`Showcase.Web.Client`** — a standalone Blazor WebAssembly app. References
  `Blazor/FactoryAspects.csproj` directly via `<ProjectReference>` (not the
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

Follow the existing pages' shape (`Showcase.Web.Client/Pages/*.razor`) — one page
per doc grouping from `Blazor/docs/index.md`, each section showing the component
live plus the markup used, matching what that component's own `Blazor/docs/*.md`
page documents. Add the new nav link in `Layout/NavMenu.razor`.

## Once this gets a real deployment

An Azure Static Web App is planned for `Showcase.Web.Client` (with `Showcase.Web.Api`
as its linked API), but isn't wired up yet — no workflow, no `staticwebapp.config.json`.
Until then this only runs locally, and `Showcase.Web.Api` is intentionally not
production-hardened (see the CORS/auth note above) — harden it for real before it's
reachable from anywhere but localhost.
