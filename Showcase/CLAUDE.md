# CLAUDE.md

This file provides guidance to Claude Code when working on `Showcase/`. See root
[`CLAUDE.md`](../CLAUDE.md) for what `Showcase/` is (a demo consumer, not a library)
and the Showcase-sync rule that applies whenever a library changes. See
[`Showcase/README.md`](README.md) for what it is and how to run it — this file is
the conventions for how it's built.

## Adding a page for a new component

Structured to mirror `Blazor/docs/` — one `.razor` page per component
(`Pages/Elements/FaButtonPage.razor` → route `/fa-button`, same slug as
`Blazor/docs/fa-button.md`), not one page per category. `Pages/Home.razor` is the
category-grouped index, the live-app equivalent of `Blazor/docs/index.md`;
`Layout/NavMenu.razor` groups the same links into collapsible sidebar sections.
Adding a component's page means adding that page, its link in `Home.razor`, and its
link in `NavMenu.razor`, matching whatever category `Blazor/docs/index.md` puts it
in.

Page files live under one subfolder per category — `Pages/Elements/`, `Pages/Forms/`,
`Pages/Feedback/`, `Pages/Data/`, `Pages/Chrome/`, `Pages/Templates/`,
`Pages/Palettes/` — mirroring the same split `Blazor/Components/` itself uses (see
[`Blazor/CLAUDE.md`](../Blazor/CLAUDE.md)), plus two Showcase-only buckets for the
page templates and the palette gallery. `Pages/Home.razor` and `Pages/NotFound.razor`
stay at the `Pages/` root — they're not "a component's demo page," they're the app's
own entry points. Every moved page pins `@namespace Showcase.Web.Client.Pages` as its
first line regardless of which subfolder it physically lives in, the same
"folder placement is physical only, not a namespace change" rule
[`Blazor/CLAUDE.md`](../Blazor/CLAUDE.md) uses for its own `Enums`/`Models`/
`Components` subfolders — routes (`@page "/fa-button"`) don't depend on file
location either way, so moving a page's file is never a breaking change for a link
to it.

Page files are suffixed `Page` (`FaButtonPage.razor`, not `FaButton.razor`) so the
page's own class name doesn't collide with the `<FaButton>` component tag it renders
— naming a page identically to a component it uses is a real `CS0104` ambiguous-
reference risk in Blazor, since both the page's own namespace and the imported
component namespace are in scope unqualified inside that file.

## Palette gallery

The `Palette` section (`Pages/Palettes/Palette.razor` plus
`Pages/Palettes/Palette<Name>.razor`) is the live-app equivalent of `Blazor/.themes/`
(gitignored, local-only design docs there) — `Palette.razor` mirrors that folder's
`index.html` (a swatch-strip card grid, one card per palette), and each
`Palette<Name>.razor` mirrors its `<slug>.html` (every `--fa-*` token as a labeled
swatch, light mode then dark-mode overrides). Purely a static reference display — it
does not switch the app's own palette; that's what the sidebar's
`<FaPaletteSwitcher>` is for. `Models/PaletteCatalog.cs` holds every palette's
`Light`/`Dark` token dictionaries as literal hex copies of `_palettes.scss` (a live
`var(--fa-*)` read only ever reflects whichever ONE palette is active on `<html>`, so
previewing all of them side by side needs its own copy). Adding a new palette means
adding an entry to `Models/PaletteCatalog.cs` (copy its `--fa-*` values from
`_palettes.scss`) and a matching thin `Pages/Palettes/Palette<Name>.razor` that
renders `<PaletteDetail Slug="..." />`.

## Page templates

`Pages/Templates/` holds one thin page per `FaFa.Templates` component
(`FaDashboardTemplatePage.razor`, etc.) — each is a real, live instance of that
template, not an embedded preview, so each one sets `@layout EmptyLayout`
(`Layout/EmptyLayout.razor`, a bare `@Body` with none of `MainLayout.razor`'s own
`FaSidebarShell` chrome) rather than nesting one full-page template inside another.
`Home.razor`/`NavMenu.razor` link to these with `target="_blank"` for the same
reason — opening one replaces the whole page with that template's own chrome (or, for
`FaAuthTemplate`, no chrome at all), so a new tab keeps the Showcase's own nav from
disappearing under it. See
[`Blazor/docs/page-templates.md`](../Blazor/docs/page-templates.md).
