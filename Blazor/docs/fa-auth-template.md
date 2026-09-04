[← Back to index](index.md) · [← Back to page templates](page-templates.md)

# FaAuthTemplate

Chrome-free — no `FaHeader`/`FaSidebar`/`FaFooter` at all. A login/register/reset-
password page wants the visitor's full attention on the one card, not app
navigation they can't use yet anyway. Just a brand link, a centered `FaCard`, and
optional small content below it. Reach for [FaFormTemplate](fa-form-template.md)
instead when the page should keep the site's normal header/footer around the form.

## Usage

```razor
@page "/login"
@using Fran.Templates

<FaAuthTemplate BrandText="MyApp" BrandHref="/" Title="Log in">
    <FaLoginForm OnSubmit="HandleLoginAsync" />
    <FooterContent>
        <span>Don't have an account? <a href="/signup">Sign up</a></span>
    </FooterContent>
</FaAuthTemplate>
```

## Getting the value

Pure layout — no bound value. Whatever you put in `ChildContent` (typically
`FaLoginForm`/`FaLogoutForm` or your own `EditForm`) binds the normal way.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `BrandText` | `string` | **required** — shown above the card, not inside a header bar |
| `BrandHref` | `string` | |
| `BrandIconUrl` | `string?` | optional logo shown left of `BrandText` — see [layout shells](layout-shells.md#brand-icon) |
| `Title` / `Description` | `string?` | shown above `ChildContent` inside the card |
| `MaxWidth` | `string` | any CSS width; defaults to `24rem` |
| `ChildContent` | `RenderFragment?` | the form itself |
| `FooterContent` | `RenderFragment?` | small content below the card — a "sign up instead" link, terms text |

[← Back to index](index.md) · [← Back to page templates](page-templates.md)
