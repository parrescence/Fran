[← Back to index](index.md) · [← Back to page templates](page-templates.md)

# FaHomeTemplate

`FaStandardShell` plus a hero band above `ChildContent`'s own sections. The hero has
a built-in default (`HeroTitle`/`HeroDescription`/`HeroActions`) for the common
case; `Hero` — a full `RenderFragment` — replaces it completely when the default
band isn't enough. Set at most one of the two; `Hero` wins if both are set.

## Usage

### Built-in hero

```razor
@page "/"
@using FaFa.Templates

<FaHomeTemplate BrandText="MyApp" BrandHref="/"
               HeroTitle="Run your business without the spreadsheet"
               HeroDescription="Invoicing, budgets, and reports in one place.">
    <HeroActions>
        <FaButton Variant="FaButtonVariant.Primary" Href="/signup">Get started</FaButton>
        <FaButton Variant="FaButtonVariant.Secondary" Href="/pricing">See pricing</FaButton>
    </HeroActions>
    <ChildContent>
        <section>@* feature sections, testimonials, pricing table, etc. *@</section>
    </ChildContent>
</FaHomeTemplate>
```

### Fully custom hero

```razor
<FaHomeTemplate BrandText="MyApp" BrandHref="/">
    <Hero>
        <MyOwnHeroComponent />
    </Hero>
    <ChildContent>
        <section>...</section>
    </ChildContent>
</FaHomeTemplate>
```

## Getting the value

Pure layout — no bound value.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `BrandText` | `string` | **required** |
| `BrandHref` | `string` | |
| `IsAuthenticated` | `bool` | |
| `UserDisplayName` / `UserImageUrl` | `string?` | |
| `OnLogin` / `OnLogout` | `EventCallback` | |
| `FooterContent` | `RenderFragment?` | overrides the default `© year BrandText` |
| `HeaderPosition` / `FooterPosition` | `FaNavPosition` | `Standard` (default) \| `Sticky` \| `Floating` — see [layout shells](layout-shells.md#position-standard-sticky-or-floating) |
| `HeroTitle` / `HeroDescription` | `string?` | the built-in hero's text |
| `HeroActions` | `RenderFragment?` | typically one or two `FaButton`s |
| `Hero` | `RenderFragment?` | full replacement for the built-in hero band |
| `ChildContent` | `RenderFragment?` | whatever comes after the hero |

[← Back to index](index.md) · [← Back to page templates](page-templates.md)
