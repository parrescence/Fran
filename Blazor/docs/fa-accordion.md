[← Back to index](index.md)

# FaAccordion

Stacked collapsible sections — FAQ list, settings groups. `AllowMultipleOpen`
controls whether opening a section closes the others (the classic accordion
behavior, the default) or each section tracks its own open state independently.
Every panel gets a smooth height animation on open/close with no JavaScript (a CSS
grid-rows trick), and `prefers-reduced-motion` turns it off.

## Usage

```razor
<FaAccordion Items="@(new (string, RenderFragment)[]
{
    ("What's your return policy?", @<p>Returns are accepted within 30 days.</p>),
    ("Do you ship internationally?", @<p>Yes, to most countries.</p>),
})" />
```

### Allow more than one section open at once

```razor
<FaAccordion Items="_sections" AllowMultipleOpen="true" InitialOpenIndex="0" />
```

## Getting the value

No bound value — open/closed state lives entirely inside the component. There's
nothing to read back; if you need to know which sections are open elsewhere in your
app, don't reach for this component (build the open/closed state yourself and
render sections directly instead).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Items` | `IReadOnlyList<(string Title, RenderFragment Content)>` | **required** |
| `AllowMultipleOpen` | `bool` | `false` (default) closes other sections when one opens |
| `InitialOpenIndex` | `int?` | which section starts open, if any |
| `CssClass` | `string?` | |

[← Back to index](index.md)
