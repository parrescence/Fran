[← Back to index](index.md)

# FaCodeBlock

A read-only source-code panel: a language-labeled header, an optional
copy-to-clipboard button, and lightly syntax-highlighted content. Background is
`var(--fa-fir)` — the one token every color palette defines as its deep neutral
"black" anchor — so the block always reads as genuinely darker than the page around
it, in every palette, light or dark mode.

Not a full parser or a third-party highlighting library (no Prism/highlight.js) —
regex-driven token classification (comments, strings, numbers, keywords, or
tags/attributes for markup, or property/literal for JSON) that's good enough for a
snippet to read correctly at a glance.

## Usage

```razor
<FaCodeBlock Language="FaCodeLanguage.CSharp" Code="@_snippet" />

@code {
    private readonly string _snippet = """
        public class Greeter
        {
            public string Greet(string name) => $"Hello, {name}!";
        }
        """;
}
```

### Without the copy button

```razor
<FaCodeBlock Language="FaCodeLanguage.Json" Code="@_json" Copyable="false" />
```

## Getting the value

Nothing to bind — `Code` is display-only input, not a two-way-bound field. There's
no output to read back; the copy button writes straight to the OS clipboard via
`navigator.clipboard.writeText` (falling back to `document.execCommand('copy')` on
non-secure/`http` origins), with no Blazor-side event for a "was copied" moment.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Code` | `string` | **required.** The raw source text — HTML-encoded internally, so it's safe even if it contains `<`/`&`. |
| `Language` | `FaCodeLanguage` | `PlainText` (default) — see below for the full list |
| `Copyable` | `bool` | `true` (default). Hides the copy button when `false`; the language label still shows either way. |
| `CssClass` | `string?` | |

## `FaCodeLanguage` values

`PlainText` (default, no highlighting) · `CSharp` · `Razor` · `JavaScript` ·
`TypeScript` · `Html` · `Xml` · `Css` · `Scss` · `Json` · `Bash` · `PowerShell` ·
`Sql` · `Yaml` · `Markdown` (no highlighting — Markdown's own inline syntax reads
fine as plain monospace text).

`Razor`/`Html`/`Xml` share one tag/attribute-based ruleset; everything else uses a
comment/string/number/keyword ruleset tuned per language (e.g. SQL keywords match
case-insensitively, Bash/PowerShell/YAML use `#` line comments instead of `//`).

## Wiring

Needs `_content/Fran/js/codeblock.js` linked in your host page for the copy button
to actually work — see [Install & setup](install.md#4-wire-the-static-assets-into-your-host-page).
Without it, the button still renders (and `Copyable="false"` still hides it), but
clicking it does nothing.

[← Back to index](index.md)
