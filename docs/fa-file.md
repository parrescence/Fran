[← Back to index](index.md)

# FaFile

File picker. Set `AsButton` to show a styled button instead of the browser's default
file-input chrome.

## Usage

```razor
<FaFile AsButton="true" ButtonLabel="Upload receipt" OnChange="HandleFileSelectedAsync" />

@code {
    private async Task HandleFileSelectedAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
        // upload/read stream...
    }
}
```

## Getting the value

There's no bound `Value` — `OnChange` hands you the raw
`InputFileChangeEventArgs` (same as Blazor's own `InputFile.OnChange`), from which
you read `.File` (or `.GetMultipleFiles()` when `Multiple="true"`).

## Parameters

| Parameter | Type | Notes |
|---|---|---|
| `OnChange` | `EventCallback<InputFileChangeEventArgs>` | |
| `AsButton` | `bool` | styled button instead of default chrome |
| `ButtonLabel` | `string` | default `"Choose file"` |
| `Multiple` | `bool` | |
| `CssClass` | `string?` | |

[← Back to index](index.md)
