// Theme toggle — plain JS on purpose, not Blazor JS interop. Nothing here needs
// C# state: it's three independent axes stamped as data attributes on <html> —
// mode (light/dark/colorblind, data-theme), palette (seasonal/regional/custom,
// data-fa-palette), and input style (standard/minimal/maximal, data-fa-input-style,
// see _inputs.scss) — all persisted to localStorage, read back by an inline snippet
// in index.html <head> before first paint so there's no flash of the wrong
// theme/palette/input-style. <FaThemeSwitcher> calls window.faSetTheme(...) directly
// via a plain onclick attribute, <FaPaletteSwitcher> calls window.faSetPalette(...)
// via a plain onchange attribute, and <FaInputStyleSwitcher> calls
// window.faSetInputStyle(...) via onclick — see each .cs file for why that's fine here.
(function () {
    var THEME_STORAGE_KEY = 'fa-theme';
    var PALETTE_STORAGE_KEY = 'fa-palette';
    var CUSTOM_PALETTE_STORAGE_KEY = 'fa-custom-palette';
    var INPUT_STYLE_STORAGE_KEY = 'fa-input-style';

    // The same token set FaPaletteColors (Blazor/Models/FaPalette.cs) requires, in
    // camelCase to match its System.Text.Json-serialized JSON. --fa-<kebab-case> is the
    // custom property each one maps to.
    var CUSTOM_PALETTE_TOKENS = ['primary', 'primaryDark', 'primaryLight', 'footer', 'glow',
        'gold', 'accent', 'accentDark', 'ember', 'wine', 'fir', 'cream', 'surface', 'text',
        'textMuted', 'textOnPrimary', 'border', 'borderFocus', 'alertDangerBg',
        'alertSuccessBg', 'alertInfoBg'];

    function customPaletteCssVar(token) {
        return '--fa-' + token.replace(/([A-Z])/g, '-$1').toLowerCase();
    }

    function isDarkModeActive() {
        var explicit = document.documentElement.getAttribute('data-theme');
        if (explicit) {
            return explicit === 'dark';
        }
        return !!(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);
    }

    // Applies a FaPalette's colors/darkOverrides (Blazor/Models/FaPalette.cs) as inline
    // --fa-* custom properties on <html> — a custom palette has no compiled CSS block to
    // select via data-fa-palette, so this is its only path to actually taking effect.
    // Re-run on mode changes too (see faSetTheme below) since darkOverrides only apply
    // while dark mode is active.
    function applyCustomPaletteColors(colors, darkOverrides) {
        if (!colors) {
            return;
        }
        var dark = isDarkModeActive();
        var style = document.documentElement.style;
        for (var i = 0; i < CUSTOM_PALETTE_TOKENS.length; i++) {
            var token = CUSTOM_PALETTE_TOKENS[i];
            var value = (dark && darkOverrides && darkOverrides[token]) || colors[token];
            if (value) {
                style.setProperty(customPaletteCssVar(token), value);
            }
        }
    }

    function clearCustomPaletteColors() {
        var style = document.documentElement.style;
        for (var i = 0; i < CUSTOM_PALETTE_TOKENS.length; i++) {
            style.removeProperty(customPaletteCssVar(CUSTOM_PALETTE_TOKENS[i]));
        }
    }

    function reapplyStoredCustomPalette() {
        var stored = localStorage.getItem(CUSTOM_PALETTE_STORAGE_KEY);
        if (!stored) {
            return;
        }
        try {
            var parsed = JSON.parse(stored);
            applyCustomPaletteColors(parsed.colors, parsed.darkOverrides);
        } catch (e) {
            // Malformed stored JSON (e.g. hand-edited) — ignore rather than throw.
        }
    }

    function markActive(theme) {
        // No stored/attribute theme means "following the OS", not "Light"
        // specifically — leave every button unhighlighted rather than falsely
        // claiming Light was chosen. (Called with the already-resolved value —
        // see the DOMContentLoaded handler below for where "resolved" comes from.)
        var buttons = document.querySelectorAll('[data-theme-btn]');
        for (var i = 0; i < buttons.length; i++) {
            var btn = buttons[i];
            var isActive = !!theme && btn.getAttribute('data-theme-btn') === theme;
            btn.classList.toggle('fa-theme-btn-active', isActive);
            btn.setAttribute('aria-pressed', isActive ? 'true' : 'false');
        }
    }

    function markActiveInputStyle(inputStyle) {
        var resolved = inputStyle || 'standard';
        var buttons = document.querySelectorAll('[data-input-style-btn]');
        for (var i = 0; i < buttons.length; i++) {
            var btn = buttons[i];
            var isActive = btn.getAttribute('data-input-style-btn') === resolved;
            btn.classList.toggle('fa-input-style-btn-active', isActive);
            btn.setAttribute('aria-pressed', isActive ? 'true' : 'false');
        }
    }

    // Unlike palette, input style has a real "unset" default (standard) that always
    // resolves to something concrete — no OS-level equivalent to follow, so this
    // always stamps/removes the attribute the same way faSetPalette does.
    window.faSetInputStyle = function (inputStyle) {
        if (!inputStyle || inputStyle === 'standard') {
            document.documentElement.removeAttribute('data-fa-input-style');
            localStorage.removeItem(INPUT_STYLE_STORAGE_KEY);
        } else {
            document.documentElement.setAttribute('data-fa-input-style', inputStyle);
            localStorage.setItem(INPUT_STYLE_STORAGE_KEY, inputStyle);
        }
        markActiveInputStyle(inputStyle);
    };

    window.faSetTheme = function (theme) {
        if (theme === 'light') {
            // "light" is the explicit choice, not just "no attribute" — otherwise
            // clicking Light on a dark-OS machine would silently fall back to the
            // prefers-color-scheme media query and look like nothing happened.
            document.documentElement.setAttribute('data-theme', 'light');
        } else {
            document.documentElement.setAttribute('data-theme', theme);
        }
        localStorage.setItem(THEME_STORAGE_KEY, theme);
        markActive(theme);
        // A custom palette's darkOverrides only take effect in dark mode, so switching
        // mode while one's active needs its colors recomputed, not just the built-in
        // palette's own precompiled dark CSS block (which handles itself).
        reapplyStoredCustomPalette();
    };

    // Keeps every <select data-palette-select> (FaPaletteSwitcher.cs) showing the
    // palette actually in effect — needed both on first paint and after faSetPalette
    // runs, since a plain onchange attribute doesn't update the <select> for you when
    // the value was set programmatically (only user interaction does that natively).
    function syncPaletteSelects(palette) {
        var selects = document.querySelectorAll('[data-palette-select]');
        for (var i = 0; i < selects.length; i++) {
            selects[i].value = palette || '';
        }
    }

    // Palette has no "unset means follow the OS" concept the way mode does — there's
    // no OS-level signal for "Southwest Summer" — so unlike faSetTheme this always
    // just stamps (or removes, for the "northwest-fall" default) the attribute.
    //
    // selectEl is the <select> the change came from (FaPaletteSwitcher passes `this`
    // via onchange="faSetPalette(this.value, this)") — only actually needed for a
    // "custom:{Value}" palette, to read that option's data-colors/data-dark-overrides
    // JSON (see FaPalette in Blazor/Models/FaPalette.cs). Built-ins ignore it; calling
    // window.faSetPalette('some-builtin') by hand (docs/palette-switcher.md) still works
    // with no second argument at all.
    window.faSetPalette = function (palette, selectEl) {
        if (palette && palette.indexOf('custom:') === 0) {
            var optionEl = selectEl && selectEl.selectedOptions && selectEl.selectedOptions[0];
            var colors = optionEl && optionEl.dataset.colors ? JSON.parse(optionEl.dataset.colors) : null;
            if (!colors) {
                return; // Nothing to apply — bail rather than half-apply a palette with no colors.
            }
            var darkOverrides = optionEl.dataset.darkOverrides ? JSON.parse(optionEl.dataset.darkOverrides) : null;

            document.documentElement.removeAttribute('data-fa-palette');
            applyCustomPaletteColors(colors, darkOverrides);
            localStorage.setItem(PALETTE_STORAGE_KEY, palette);
            localStorage.setItem(CUSTOM_PALETTE_STORAGE_KEY, JSON.stringify({ colors: colors, darkOverrides: darkOverrides }));
            syncPaletteSelects(palette);
            return;
        }

        clearCustomPaletteColors();
        localStorage.removeItem(CUSTOM_PALETTE_STORAGE_KEY);
        if (!palette || palette === 'northwest-fall') {
            document.documentElement.removeAttribute('data-fa-palette');
            localStorage.removeItem(PALETTE_STORAGE_KEY);
        } else {
            document.documentElement.setAttribute('data-fa-palette', palette);
            localStorage.setItem(PALETTE_STORAGE_KEY, palette);
        }
        syncPaletteSelects(palette);
    };

    function syncAll() {
        // Same fallback shape both times: a stored choice wins; failing that, an
        // explicit attribute already on <html> (a consumer hardcoding
        // "light"/"dark"/"colorblind", or a palette, at build time — install.md's
        // palette "Option A") is real information and should be reflected too.
        // Only genuinely absent-both — nothing stored, nothing on the attribute,
        // CSS quietly following prefers-color-scheme on its own — stays ambiguous/
        // unhighlighted for theme (palette has no such "follow the OS" concept).
        markActive(localStorage.getItem(THEME_STORAGE_KEY) || document.documentElement.getAttribute('data-theme'));

        var storedPalette = localStorage.getItem(PALETTE_STORAGE_KEY);
        if (storedPalette && storedPalette.indexOf('custom:') === 0) {
            // Custom palette colors travel in their own storage key (see faSetPalette),
            // not the compiled CSS a data-fa-palette attribute would select — reapply
            // them directly instead of stamping the attribute.
            reapplyStoredCustomPalette();
        } else if (storedPalette) {
            document.documentElement.setAttribute('data-fa-palette', storedPalette);
        }
        syncPaletteSelects(storedPalette || document.documentElement.getAttribute('data-fa-palette'));

        var storedInputStyle = localStorage.getItem(INPUT_STYLE_STORAGE_KEY);
        if (storedInputStyle) {
            document.documentElement.setAttribute('data-fa-input-style', storedInputStyle);
        }
        markActiveInputStyle(storedInputStyle || document.documentElement.getAttribute('data-fa-input-style'));
    }

    document.addEventListener('DOMContentLoaded', syncAll);

    // Blazor (WASM or Server) mounts FaHeader's/FaPaletteSwitcher's actual DOM
    // elements asynchronously — after the .NET runtime finishes booting and the
    // component tree first renders, which is well after DOMContentLoaded already
    // fired. The call above typically finds zero [data-theme-btn]/
    // [data-palette-select] elements yet, since nothing's rendered them into the
    // page at that point. This observer re-runs the same sync as Blazor's initial
    // render actually lands, and stops itself after 10s regardless — plenty for
    // even a slow WASM boot, and bounds the cost for a page that (unusually) never
    // renders either component at all.
    var initialMountObserver = new MutationObserver(syncAll);
    initialMountObserver.observe(document.body, { childList: true, subtree: true });
    setTimeout(function () { initialMountObserver.disconnect(); }, 10000);
})();
