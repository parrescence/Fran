[← Back to index](index.md) · [← Back to page templates](page-templates.md)

# FaFormTemplate

`FaStandardShell` (no sidebar — a focused form flow shouldn't compete with app nav
for attention) wrapped around one centered `FaCard`. `ChildContent` is your actual
form; this template only owns the framing around it.

## Usage

```razor
@page "/orders/invite"
@using Fran.Templates

<FaFormTemplate BrandText="MyApp" BrandHref="/"
               Title="Invite a teammate"
               Description="They'll get an email with a link to join."
               BackHref="/orders" BackText="Back to orders">
    <EditForm Model="_model" OnValidSubmit="HandleSubmitAsync">
        <FaInput @bind-Value="_model.Email" Label="Email" />
        <FaButton Variant="FaButtonVariant.Primary">Send invite</FaButton>
    </EditForm>
</FaFormTemplate>
```

## Getting the value

Pure layout — no bound value. Whatever form you put in `ChildContent` binds the
normal way.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `BrandText` | `string` | **required** |
| `BrandHref` | `string` | |
| `BrandIconUrl` | `string?` | optional logo shown left of `BrandText` — see [layout shells](layout-shells.md#brand-icon) |
| `IsAuthenticated` | `bool` | |
| `UserDisplayName` / `UserImageUrl` | `string?` | |
| `OnLogin` / `OnLogout` | `EventCallback` | |
| `FooterContent` | `RenderFragment?` | overrides the default `© year BrandText` |
| `HeaderPosition` / `FooterPosition` | `FaNavPosition` | `Standard` (default) \| `Sticky` \| `Floating` — see [layout shells](layout-shells.md#position-standard-sticky-or-floating) |
| `Title` / `Description` | `string?` | shown above `ChildContent` inside the card |
| `MaxWidth` | `string` | any CSS width; defaults to `28rem` |
| `BackHref` / `BackText` | `string?` / `string` | optional link above the card; omit `BackHref` to skip it |
| `ChildContent` | `RenderFragment?` | the form itself |

[← Back to index](index.md) · [← Back to page templates](page-templates.md)
