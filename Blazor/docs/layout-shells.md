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
| `IsAuthenticated` | `bool` | swaps between login button and avatar+name+logout |
| `UserDisplayName` / `UserImageUrl` | `string?` | fed into `FaAvatar` |
| `OnLogin` / `OnLogout` | `EventCallback` | |
| `ChildContent` | `RenderFragment?` | page content (`@Body` in a layout) |
| `FooterContent` | `RenderFragment?` | overrides the default `© year BrandText` footer text |
| `Sidebar` | `RenderFragment?` | **`FaSidebarShell` only** — your nav content |

[← Back to index](index.md)
