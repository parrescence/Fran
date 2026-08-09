# FactoryAspects component docs

Prefer one scrollable page instead? Open [`site.html`](site.html) directly in a
browser (double-click it, or via a `file://` link) — same content as this folder,
laid out with a sidebar nav and copy buttons, no server required.

One page per component: a minimal usage example and how to read the value back out.
This assumes you've already done the install steps in the root
[README](../README.md#install) (package reference + the `theme.css`/`theme.js`/
`sidebar.js` `<link>`/`<script>` tags in your host page).

Every example is a `.razor` file — the components themselves are authored as plain C#
(see [CLAUDE.md](../CLAUDE.md#component-authoring-c-builder-not-markup)), but you
consume them the normal Blazor way, tags and all.

Two binding shapes show up repeatedly:

- **`InputBase<TValue>`-derived fields** (`FaInput`, `FaSelect`, `FaTextarea`,
  `FaCheckbox`, `FaDatePicker`, `FaCurrency`) only work inside an `<EditForm>`/
  `EditContext`, same as Blazor's own `InputText`/`InputNumber`.
- **Everything else that's bindable** (`FaToggle`, `FaRadioGroup`, `FaSearchSelect`,
  `FaDateRange`) uses a plain `Value`/`ValueChanged` pair, so `@bind-Value` works with
  or without an `EditForm` around it.

## Buttons & feedback

- [FaButton](fa-button.md) — the standard button/link, 7 color variants
- [FaCard](fa-card.md) — bordered, optionally clickable tile
- [FaAlert](fa-alert.md) — info/success/danger banner
- [FaBadge](fa-badge.md) — small pill label
- [FaAvatar](fa-avatar.md) — photo or initials circle
- [FaModal](fa-modal.md) — backdrop dialog, caller owns open/closed state

## Form fields

- [FaInput](fa-input.md) — generic text/number field
- [FaSelect](fa-select.md) — plain `<select>`, options as child content
- [FaSearchSelect](fa-search-select.md) — type-to-search combobox over your own query function
- [FaTextarea](fa-textarea.md) — multi-line text, optional maxlength counter
- [FaCheckbox](fa-checkbox.md) — checkbox with a clickable label
- [FaRadioGroup](fa-radio-group.md) — a group of radio buttons from a tuple list
- [FaToggle](fa-toggle.md) — segmented N-option switch, with an optional companion input
- [FaDatePicker](fa-date-picker.md) — day/month/year fields + calendar popup
- [FaDateRange](fa-date-range.md) — linked From/To date fields
- [FaCurrency](fa-currency.md) — formatted amount field
- [FaFile](fa-file.md) — file picker, optionally styled as a button

## Data display

- [FaTable](fa-table.md) — plain typed `<table>`, you own paging/filtering
- [FaGrid](fa-grid.md) — a table with its own paging/rows-per-page/sort/filter, in-memory or provider-backed

## Layout & chrome

- [Layout shells](layout-shells.md) — `StandardShell`/`SidebarShell` + `AppHeader`/`AppFooter`/`AppSidebar`
- [ThemeSwitcher](theme-switcher.md) — Light/Dark/Colorblind-safe buttons
- [FaIcon](fa-icon.md) — the hand-drawn SVG icon set
