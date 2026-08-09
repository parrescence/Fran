[← Back to index](index.md)

# FaIcon

Small hand-drawn SVG icon set — no icon font/external dependency. Every path fills
with `currentColor`, so `Color` is pure CSS.

## Usage

```razor
<FaIcon Name="FaIconName.Calendar" Color="FaIconColor.Black" Size="18" Title="Calendar" />
```

## Getting the value

No bound value — it's a static icon. `Name` picks which SVG renders.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `Name` | `FaIconName` | **required** — see the full list below |
| `Color` | `FaIconColor` | `White` (default) \| `Black` |
| `Size` | `int` | pixel width/height, default `20` |
| `Title` | `string?` | sets `role="img"` + `<title>` for a11y; omit for a purely decorative icon (`aria-hidden`) |
| `CssClass` | `string?` | |

## Available `FaIconName` values

`Home`, `Plus`, `Ledger`, `PiggyBank`, `Dashboard`, `People`, `Sun`, `Moon`, `Eye`,
`ChevronLeft`, `ChevronRight`, `Tag`, `Person`, `Receipt`, `Calendar`

Action suite — CRUD, editing, and general UI actions:

`Edit`, `Save`, `Delete`, `Cancel`, `Confirm`, `Search`, `Filter`, `Sort`, `Download`,
`Upload`, `Copy`, `Print`, `Refresh`, `Settings`, `Menu`, `MoreHorizontal`,
`MoreVertical`, `Warning`, `Info`, `Success`, `Error`, `EyeOff`, `Star`, `Bell`,
`Lock`, `Unlock`, `ArrowUp`, `ArrowDown`, `ArrowLeft`, `ArrowRight`, `ChevronUp`,
`ChevronDown`, `Undo`, `Redo`, `Login`, `Logout`, `Share`, `ExternalLink`

[← Back to index](index.md)
