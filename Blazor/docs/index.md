# FaFa component docs

Prefer one scrollable page instead? Open [`site.html`](site.html) directly in a
browser (double-click it, or via a `file://` link) — same content as this folder,
laid out with a sidebar nav and copy buttons, no server required.

One page per component: a minimal usage example and how to read the value back out.
This assumes you've already done the install/wiring steps in
[**`install.md`**](install.md) (package reference, namespace imports, the
`fa-styles.css`/`theme.js`/`sidebar.js` `<link>`/`<script>` tags in your host page, and
picking a color palette).

Every example is a `.razor` file — the components themselves are authored as plain C#
(see [CLAUDE.md](../CLAUDE.md#component-authoring-c-builder-not-markup)), but you
consume them the normal Blazor way, tags and all.

Two binding shapes show up repeatedly:

- **`InputBase<TValue>`-derived fields** (`FaInput`, `FaSelect`, `FaTextarea`,
  `FaCheckbox`, `FaDate`, `FaCurrency`) only work inside an `<EditForm>`/
  `EditContext`, same as Blazor's own `InputText`/`InputNumber`.
- **Everything else that's bindable** (`FaToggle`, `FaRadioGroup`, `FaSearchSelect`,
  `FaDropdown`, `FaDateRange`) uses a plain `Value`/`ValueChanged` pair, so
  `@bind-Value` works with or without an `EditForm` around it.

## Start here

- [Install & setup](install.md) — GitHub Packages feed, package reference, namespace
  imports, host-page wiring, and picking a color palette

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
- [FaDropdown](fa-dropdown.md) — type-to-filter combobox over a local list, windowed to N visible rows
- [FaTextarea](fa-textarea.md) — multi-line text, optional maxlength counter
- [FaCheckbox](fa-checkbox.md) — checkbox with a clickable label
- [FaRadioGroup](fa-radio-group.md) — a group of radio buttons from a tuple list
- [FaToggle](fa-toggle.md) — segmented N-option switch, with an optional companion input
- [FaDate](fa-date.md) — day/month/year fields + calendar popup
- [FaDateRange](fa-date-range.md) — linked From/To date fields
- [FaCurrency](fa-currency.md) — formatted amount field
- [FaFile](fa-file.md) — file picker, optionally styled as a button

## Data display

- [FaTable](fa-table.md) — plain typed `<table>`, you own paging/filtering
- [FaGrid](fa-grid.md) — a table with its own paging/rows-per-page/sort/filter, in-memory or provider-backed
- [FaCarousel](fa-carousel.md) — one-slide-at-a-time slideshow, in-memory or provider-backed

## Content display

- [FaCodeBlock](fa-code-block.md) — read-only source-code panel, language label, optional copy button, lightly syntax-highlighted

## Self-contained forms

- [FaForm](fa-form.md) — wraps EditForm around your own field markup, returns the populated model on submit; `Mode` (Create/Edit) covers "new" vs "update"
- [FaLoginForm](fa-login-form.md) — username/password + remember-me, emits an `FaLoginRequest`
- [FaLogoutForm](fa-logout-form.md) — "are you sure?" confirm/cancel

## Validation

- [Validation](validation.md) — root/DTO, form, and per-element validation tiers, plus inline error messages on every input

## Sizing & responsive

- [Sizing & responsive](sizing.md) — the shared `FaSize` scale, opt-in `Responsive` full-width-on-mobile, `FaInput`'s floating label, and the app-wide input-style axis

## Progress & loaders

- [FaProgress](fa-progress.md) — filled track, 4 fill directions (Right/Left/Up/Down)
- [FaSpinner](fa-spinner.md) — classic rotating ring
- [FaLoadingDots](fa-loading-dots.md) — "Loading" with a growing/resetting ellipsis
- [FaHelixLoader](fa-helix-loader.md) — twisting, breathing DNA-helix dots
- [FaPongLoader](fa-pong-loader.md) — bouncing-ball Pong loader

## Navigation & structure

- [FaTabs](fa-tabs.md) — role="tablist" tab strip over a (Title, Value) list, you render the panel
- [FaAccordion](fa-accordion.md) — stacked collapsible sections, smooth height animation, no JS
- [FaBreadcrumb](fa-breadcrumb.md) — Home › Section › current page trail
- [FaPagination](fa-pagination.md) — standalone windowed page-number strip
- [FaDivider](fa-divider.md) — plain rule, or a rule split around a short label
- [FaChip](fa-chip.md) — small interactive tag: selectable, removable, or both
- [FaEmptyState](fa-empty-state.md) — "nothing here yet" placeholder with icon/title/description/actions

## Overlays & feedback

- [FaTooltip](fa-tooltip.md) — pure-CSS hover/focus label, no JS positioning
- [FaPopover](fa-popover.md) — click-to-open contextual panel
- [FaToastHost](fa-toast.md) — stacked auto-dismissing notifications, triggered from anywhere via an injected `FaToastService`
- [FaSkeleton](fa-skeleton.md) — shimmering content-shaped loading placeholder

## Page templates

- [Page templates overview](page-templates.md) — how to use one, filling in the bars, sticky vs. not
- [FaDashboardTemplate](fa-dashboard-template.md) — sidebar shell + title/actions row
- [FaFormTemplate](fa-form-template.md) — standard shell + centered form card
- [FaHomeTemplate](fa-home-template.md) — standard shell + hero band
- [FaAuthTemplate](fa-auth-template.md) — chrome-free centered card, for login/register/reset-password

## Layout & chrome

- [Layout shells](layout-shells.md) — `FaStandardShell`/`FaSidebarShell` + `FaHeader`/`FaFooter`/`FaSidebar`
- [FaThemeSwitcher](theme-switcher.md) — Light/Dark/Colorblind-safe buttons
- [FaPaletteSwitcher](palette-switcher.md) — dropdown over all 28 color palettes
- [FaInputStyleSwitcher](input-style-switcher.md) — Standard/Minimal/Maximal buttons for the app-wide input-style axis
- [FaIcon](fa-icon.md) — the hand-drawn SVG icon set
