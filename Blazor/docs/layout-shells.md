[← Back to index](index.md)

# Layout shells

`AppHeader`, `AppFooter`, and `AppSidebar` are the individual pieces; `StandardShell`
and `SidebarShell` compose them into the two full-page templates. Most consumers only
ever touch the two shells, from `MainLayout.razor`.

## StandardShell — header + content + footer, no sidebar

For pages that don't need app navigation alongside them (landing/marketing pages,
standalone flows).

```razor
@inherits LayoutComponentBase

<StandardShell BrandText="MyApp"
               BrandHref="/"
               IsAuthenticated="@_isAuthenticated"
               UserDisplayName="@_userName"
               UserImageUrl="@_userPhotoUrl"
               OnLogin="HandleLoginAsync"
               OnLogout="HandleLogoutAsync">
    @Body
</StandardShell>

@code {
    private bool _isAuthenticated;
    private string? _userName;
    private string? _userPhotoUrl;

    private Task HandleLoginAsync() { /* redirect to your auth flow */ return Task.CompletedTask; }
    private Task HandleLogoutAsync() { /* sign out */ return Task.CompletedTask; }
}
```

## SidebarShell — header + left sidebar + content + footer

Same idea, plus a `Sidebar` render fragment — your app supplies its own nav menu.

```razor
@inherits LayoutComponentBase

<SidebarShell BrandText="MyApp"
              BrandHref="/"
              IsAuthenticated="@_isAuthenticated"
              UserDisplayName="@_userName"
              OnLogin="HandleLoginAsync"
              OnLogout="HandleLogoutAsync">
    <Sidebar>
        <NavMenu /> @* your own nav links component *@
    </Sidebar>
    @Body
</SidebarShell>
```

## Using the pieces directly

If neither shell fits (a custom page structure), compose `AppHeader`/`AppSidebar`/
`AppFooter` yourself — this is exactly what the two shells do internally.

```razor
<AppHeader BrandText="MyApp" BrandHref="/" IsAuthenticated="@_isAuthenticated"
           UserDisplayName="@_userName" OnLogin="HandleLoginAsync" OnLogout="HandleLogoutAsync" />

<AppSidebar>
    <NavMenu />
</AppSidebar>

<main>@Body</main>

<AppFooter BrandText="MyApp" />
```

## Getting the value

These are pure layout — no bound value. `OnLogin`/`OnLogout` fire on button click;
your app owns actually authenticating and then setting `IsAuthenticated`/
`UserDisplayName`/`UserImageUrl` on the next render. `ThemeSwitcher` is already baked
into `AppHeader` (see [its page](theme-switcher.md) for how theme state itself
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
| `Sidebar` | `RenderFragment?` | **`SidebarShell` only** — your nav content |

[← Back to index](index.md)
