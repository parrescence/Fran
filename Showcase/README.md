# Fran Showcase

A live, in-repo demo of `Blazor/` (and, over time, every other style library in this
repo) — every component rendered for real, with the actual markup used to produce
it. Not published anywhere yet: pull this branch and run it locally.

See [`CLAUDE.md`](CLAUDE.md) for the conventions behind how this app is structured
(page-per-component layout, naming, palette gallery, page templates) — this file is
just what it is and how to run it.

Two projects:

- **`Showcase.Web.Client`** — a standalone Blazor WebAssembly app. References
  `Blazor/Fran.csproj` directly via `<ProjectReference>` (not the
  published GitHub Packages package), so it always shows whatever's currently on
  this branch, in-progress work included. A real consumer app outside this repo
  installs the package instead — see
  [`Blazor/docs/install.md`](../Blazor/docs/install.md).
- **`Showcase.Web.Api`** — an Azure Functions app (isolated worker, in-memory
  sample data, no auth, no real database) that backs the client's "pulled from the
  database" demos: `FaGrid`'s `ItemsProvider`, `FaCarousel`'s `ItemsProvider`, and
  `FaLoginForm`'s `OnSubmit`. Everything else on the client runs entirely
  client-side.

## Running it locally

Requires the [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
(`func`) in addition to the .NET SDK. Two terminals, both from this `Showcase/`
folder:

```bash
cd Showcase.Web.Api
func start
```

```bash
cd Showcase.Web.Client
dotnet run --launch-profile https
```

Then open the client at **https://localhost:7095**. The API listens on
**http://localhost:7071** — `Showcase.Web.Client/wwwroot/appsettings.json`'s
`ApiBaseUrl` points there already, and `Showcase.Web.Api/local.settings.json`'s
`Host.CORS` already allows the client's dev origin. Change all three together if
you run either on a different port.

Try the login form with `demo` / `password` (anything else fails on purpose — see
`Showcase.Web.Api/AuthFunctions.cs`).

## Deployment

Deployed on every push to `dev` (`.github/workflows/deploy-showcase.yml`) — only a
dev environment exists so far; the intended end state is one environment per
branch (`dev` → dev resources, `test` → test resources, `main`/a future `prod` →
prod resources, each its own resource group), added to the trigger once those
resources actually exist. For now:
`Showcase.Web.Client` to an Azure Static Web App (Free tier, **no login required**
— that's the public showcase), `Showcase.Web.Api` to an Azure Function App on a
Flex Consumption plan. The client calls the API directly over HTTPS
(`wwwroot/appsettings.Production.json`'s `ApiBaseUrl`) rather than through an SWA
linked/managed backend — Free tier SWA doesn't support linking an externally
hosted Function App, and nothing here needs the extra cost of Standard tier for
that. `Showcase.Web.Api` stays intentionally not hardened beyond CORS (see the
no-auth note above) — it's still purely a stand-in with sample data, not a real
backend; harden it for real before it holds anything that matters.

Both are in `Fran-RG-EastUS2-DEV` in the Parrescence-Dev subscription, deployed via
a GitHub Actions OIDC login (`Fran-MI-EastUS2-DEV`, a user-assigned managed identity
with a federated credential trusting this repo — no stored Azure secret for the API
deploy job; the SWA deploy job still uses its own deployment-token secret, which is
how the `static-web-apps-deploy` action authenticates). The Function App itself
(`Fran-API-WestUS2-DEV`, with its own `stfranshowcasewus2dev` storage account) is in
West US 2, not East US 2 like the resource group's name suggests — Flex Consumption
plan creation was blocked in East US 2 by a quota shared with `vince`'s existing
plans there, and West US 2 wasn't. The resource group itself stays in East US 2.
