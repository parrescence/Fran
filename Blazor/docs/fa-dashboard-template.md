[← Back to index](index.md) · [← Back to page templates](page-templates.md)

# FaDashboardTemplate

`FaSidebarShell` plus the one thing every dashboard page repeats that the bare
shell doesn't provide: a title/actions row above the body. Omit both `Title` and
`Actions` to skip that row entirely and get exactly what `FaSidebarShell` gives you.

## Usage

```razor
@page "/dashboard"
@using Fran.Templates

<FaDashboardTemplate BrandText="MyApp" BrandHref="/"
                    IsAuthenticated="@_isAuthenticated" UserDisplayName="@_userName"
                    OnLogin="HandleLoginAsync" OnLogout="HandleLogoutAsync"
                    Title="Overview">
    <Sidebar>
        <NavMenu />
    </Sidebar>
    <Actions>
        <FaButton Variant="FaButtonVariant.Primary">New report</FaButton>
    </Actions>
    <ChildContent>
        <FaCard>Stat cards, a grid, charts — whatever the page shows.</FaCard>
    </ChildContent>
</FaDashboardTemplate>
```

## Getting the value

Pure layout — no bound value. `OnLogin`/`OnLogout` fire on button click, same as the
raw shells (see [layout shells](layout-shells.md#getting-the-value)).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `BrandText` | `string` | **required** |
| `BrandHref` | `string` | |
| `BrandIconUrl` | `string?` | optional logo shown left of `BrandText` — see [layout shells](layout-shells.md#brand-icon) |
| `IsAuthenticated` | `bool` | |
| `UserDisplayName` / `UserImageUrl` | `string?` | |
| `OnLogin` / `OnLogout` | `EventCallback` | |
| `Sidebar` | `RenderFragment?` | your nav content |
| `FooterContent` | `RenderFragment?` | overrides the default `© year BrandText` |
| `HeaderPosition` / `FooterPosition` / `SidebarPosition` | `FaNavPosition` | `Standard` (default) \| `Sticky` \| `Floating` — see [layout shells](layout-shells.md#position-standard-sticky-or-floating) |
| `SidebarCollapsible` | `bool` | see [layout shells](layout-shells.md#collapsible-sidebar) |
| `ContainScroll` | `bool` | see [layout shells](layout-shells.md#contained-scroll) |
| `Title` | `string?` | page heading; omit with `Actions` to skip the header row |
| `Actions` | `RenderFragment?` | right-aligned content next to `Title`, typically `FaButton`s |
| `ChildContent` | `RenderFragment?` | the dashboard body |

[← Back to index](index.md) · [← Back to page templates](page-templates.md)
