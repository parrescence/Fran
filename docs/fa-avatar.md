[← Back to index](index.md)

# FaAvatar

Circular avatar — shows `ImageUrl` when set, otherwise falls back to initials
computed from `DisplayName`.

## Usage

```razor
<FaAvatar DisplayName="@user.Name" ImageUrl="@user.PhotoUrl" />
```

## Getting the value

No bound value — purely presentational. It derives initials from `DisplayName`
itself (first + last initial, or a single initial, or `"?"` if empty) — nothing to
read back.

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `DisplayName` | `string?` | used for `title`/`alt` and the initials fallback |
| `ImageUrl` | `string?` | when set, renders `<img>` instead of initials |
| `CssClass` | `string?` | |

[← Back to index](index.md)
