[← Back to index](index.md)

# Layout shells

`FaHeader`, `FaFooter`, and `FaSidebar` are the individual pieces; `FaStandardShell`
and `FaSidebarShell` compose them into the two full-page templates. Most consumers only
ever touch the two shells, from `MainLayout.razor`.

## FaStandardShell — header + content + footer, no sidebar

For pages that don't need app navigation alongside them (landing/marketing pages,
standalone flows).

```razor
@inherits LayoutComponentBase

<FaStandardShell BrandText="MyApp"
               BrandHref="/"
               IsAuthenticated="@_isAuthenticated"
               UserDisplayName="@_userName"
               UserImageUrl="@_userPhotoUrl"
               OnLogin="HandleLoginAsync"
               OnLogout="HandleLogoutAsync">
    @Body
</FaStandardShell>

@code {
    private bool _isAuthenticated;
    private string? _userName;
    private string? _userPhotoUrl;

    private Task HandleLoginAsync() { /* redirect to your auth flow */ return Task.CompletedTask; }
    private Task HandleLogoutAsync() { /* sign out */ return Task.CompletedTask; }
}
```

## FaSidebarShell — header + left sidebar + content + footer

Same idea, plus a `Sidebar` render fragment — your app supplies its own nav menu.

```razor
@inherits LayoutComponentBase

<FaSidebarShell BrandText="MyApp"
              BrandHref="/"
              IsAuthenticated="@_isAuthenticated"
              UserDisplayName="@_userName"
              OnLogin="HandleLoginAsync"
              OnLogout="HandleLogoutAsync">
    <Sidebar>
        <NavMenu /> @* your own nav links component *@
    </Sidebar>
    <ChildContent>
        @Body
    </ChildContent>
</FaSidebarShell>
```

`<ChildContent>` has to be explicit here, not bare `@Body` — Razor only auto-maps
unwrapped content to a component's `ChildContent` when that's the *only*
`RenderFragment` parameter being used at that call site. `FaSidebarShell` also has
`Sidebar` (and optionally `FooterContent`), so once `Sidebar` is in use, `ChildContent`
needs its own tag too or the compiler rejects it (`RZ9996`). `FaStandardShell` (below)
doesn't have this problem — it only has `ChildContent`, so bare content works fine.

## Using the pieces directly

If neither shell fits (a custom page structure), compose `FaHeader`/`FaSidebar`/
`FaFooter` yourself — this is exactly what the two shells do internally.

```razor
<FaHeader BrandText="MyApp" BrandHref="/" IsAuthenticated="@_isAuthenticated"
           UserDisplayName="@_userName" OnLogin="HandleLoginAsync" OnLogout="HandleLogoutAsync" />

<FaSidebar>
    <NavMenu />
</FaSidebar>

<main>@Body</main>

<FaFooter BrandText="MyApp" />
```

## Brand icon

Set `BrandIconUrl` on either shell (or `FaHeader` directly) to show a logo next to
`BrandText`:

```razor
<FaSidebarShell BrandText="MyApp" BrandHref="/" BrandIconUrl="/logo.svg">
    <Sidebar><NavMenu /></Sidebar>
    <ChildContent>@Body</ChildContent>
</FaSidebarShell>
```

Omit it for text-only branding (the default). This is the header/sidebar chrome's
own logo, not the browser tab's favicon — see
[Install & setup, step 7](install.md#7-set-a-favicon--app-icon-optional) for that.

## Position: Standard, Sticky, or Floating

`FaHeader`, `FaFooter`, and `FaSidebar` each take a `Position="FaNavPosition.___"`
parameter — `Standard` (the default, scrolls away with the page), `Sticky` (pinned
to its edge of the viewport once scrolled to), or `Floating` (same pinning, inset
with margin/rounded corners so it reads as a detached bar over the content). The two
shells expose the same choice per-bar as `HeaderPosition`/`FooterPosition`/
`SidebarPosition`, passed straight through to the piece they wrap:

```razor
<FaSidebarShell BrandText="MyApp" BrandHref="/"
              HeaderPosition="FaNavPosition.Sticky"
              SidebarPosition="FaNavPosition.Sticky"
              FooterPosition="FaNavPosition.Standard">
    <Sidebar><NavMenu /></Sidebar>
    <ChildContent>@Body</ChildContent>
</FaSidebarShell>
```

Pinning the sidebar is different from pinning the header/footer: once it's stuck
full-height, its own content has to scroll independently instead of scrolling away
with the page, so `Sticky`/`Floating` also switch it to `overflow-y: auto`. It also
settles in right below the header rather than sliding underneath it — its `top`/
`height` are offset by `--fa-header-height` (a fixed `4rem`, also enforced as
`FaHeader`'s own `min-height` so the two stay in sync; see `_layout.scss`) instead
of `0`/`100vh`. That offset assumes a standard-height header sits above it, which is
how both shells pair them — as in the example above. Using the pieces directly, set
`Position` on `FaHeader`/`FaFooter`/`FaSidebar` themselves the same way.

## Contained scroll

By default (`ContainScroll="false"`), a shell's `.fa-shell` only sets `min-height:
100vh` — header, sidebar, main content, and footer all just grow the page past one
viewport, and the whole document scrolls together. That's the right default for most
pages, but wrong for a dashboard-style page that wants the header/sidebar/footer
pinned to the viewport edges with only the page's own content scrolling inside
`<main>`. Set `ContainScroll="true"` on either shell to switch to that instead:

```razor
<FaSidebarShell BrandText="MyApp" BrandHref="/" ContainScroll="true">
    <Sidebar><NavMenu /></Sidebar>
    <ChildContent>@Body</ChildContent>
</FaSidebarShell>
```

This is a separate opt-in class (`.fa-shell-contained` in `_layout.scss`) rather than
a change to `.fa-shell` itself, so existing whole-page-scroll consumers are
unaffected. A `Standard`-position `Sidebar` taller than the viewport scrolls
independently under `ContainScroll` too, the same way `Sticky`/`Floating` already do
(see above) — nothing extra to configure for that.

## Collapsible sidebar

`FaSidebar` also takes `Collapsible` — `false` by default, meaning no toggle button
renders and the sidebar always shows expanded on desktop widths (small screens ignore
this entirely — see "Sidebar on small screens" below, unrelated to this parameter and
can't be turned off). `FaSidebarShell` exposes the same choice as `SidebarCollapsible`:

```razor
<FaSidebarShell BrandText="MyApp" BrandHref="/" SidebarCollapsible="true">
    <Sidebar><NavMenu /></Sidebar>
    <ChildContent>@Body</ChildContent>
</FaSidebarShell>
```

Set `true` only once every link in your `Sidebar` content is set up to collapse to
an icon — each one needs its own `FaIcon` + `<span class="fa-sidebar-link-text">`
wrapper (see `FaSidebar`'s Overview/Palette links in the
[Fran Showcase](https://github.com/parrescence/fran-showcase) app's own
`NavMenu.razor` for the shape). A collapsed sidebar is an icon-only rail: any link
without an icon of its own has nothing to show at 64px wide, so it's hidden while
collapsed rather than left to wrap into an illegible sliver — which means
`Collapsible="true"` with an icon-less nav effectively hides most of your navigation
once a user collapses it. Fran Showcase leaves `SidebarCollapsible` at its `false`
default for exactly that reason — its `NavMenu.razor` groups links under `<details>`
disclosure sections that don't have individual icons.

## Sidebar on small screens

Below `_responsive.scss`'s breakpoint (720px), `FaSidebar` goes off-canvas — hidden
until opened — regardless of `Collapsible`, which is a separate, desktop-only
icon-rail affordance. `FaSidebarShell` automatically shows a hamburger button left of
the brand in the header at that width (`FaHeader.ShowSidebarToggle`, which the shell
sets for you — nothing to configure); tapping it reveals the sidebar as a full-width
row below the header, tapping again hides it. This is also independent of `Position` —
`Sticky`/`Floating`'s pinned-full-height styling is reset at this breakpoint too, so
every `Position` collapses into the same off-canvas row rather than only `Standard`
behaving this way. State isn't persisted across page loads, unlike the desktop
collapse above. Using `FaHeader`/`FaSidebar` directly instead of `FaSidebarShell`, set
`ShowSidebarToggle="true"` on `FaHeader` yourself to get the same behavior.

## Getting the value

These are pure layout — no bound value. `OnLogin`/`OnLogout` fire on button click;
your app owns actually authenticating and then setting `IsAuthenticated`/
`UserDisplayName`/`UserImageUrl` on the next render. `FaThemeSwitcher` is already baked
into `FaHeader` (see [its page](theme-switcher.md) for how theme state itself
works) — nothing to wire up for it.

## Parameters (shared by both shells)

| Parameter | Type | Notes |
|---|---|---|
| `BrandText` | `string` | **required** (no hardcoded default — see [CLAUDE.md](../CLAUDE.md)) |
| `BrandHref` | `string` | link target for the brand |
| `BrandIconUrl` | `string?` | optional logo/icon shown left of `BrandText` — any `<img>` src (static asset path, `data:` URI, CDN URL); omit for text-only branding (default) |
| `IsAuthenticated` | `bool` | swaps between login button and avatar+name+logout |
| `UserDisplayName` / `UserImageUrl` | `string?` | fed into `FaAvatar` |
| `OnLogin` / `OnLogout` | `EventCallback` | |
| `ChildContent` | `RenderFragment?` | page content (`@Body` in a layout) |
| `FooterContent` | `RenderFragment?` | overrides the default `© year BrandText` footer text |
| `Sidebar` | `RenderFragment?` | **`FaSidebarShell` only** — your nav content |
| `HeaderPosition` | `FaNavPosition` | `Standard` (default) \| `Sticky` \| `Floating` |
| `FooterPosition` | `FaNavPosition` | `Standard` (default) \| `Sticky` \| `Floating` |
| `SidebarPosition` | `FaNavPosition` | **`FaSidebarShell` only** — `Standard` (default) \| `Sticky` \| `Floating` |
| `SidebarCollapsible` | `bool` | **`FaSidebarShell` only** — icon-only collapse toggle, defaults to `false` |
| `ContainScroll` | `bool` | pins header/sidebar/footer to the viewport, only `<main>` scrolls internally, defaults to `false` |

[← Back to index](index.md)
