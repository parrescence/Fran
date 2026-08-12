// Theme toggle — plain JS on purpose, not Blazor JS interop. Nothing here needs
// C# state: it's three explicit modes (light / dark / colorblind) stamped as
// data-theme on <html>, and a separate seasonal/regional palette stamped as
// data-fa-palette on <html> (see fa-styles.css's palette partial — the two
// attributes are independent axes) — both persisted to localStorage, read back by
// an inline snippet in index.html <head> before first paint so there's no flash of
// the wrong theme/palette. <ThemeSwitcher> calls window.faSetTheme(...) directly
// via a plain onclick attribute, and <PaletteSwitcher> calls window.faSetPalette(...)
// via a plain onchange attribute — see each .cs file for why that's fine here.
(function () {
    var THEME_STORAGE_KEY = 'fa-theme';
    var PALETTE_STORAGE_KEY = 'fa-palette';

    function markActive(theme) {
        // No stored theme means "following the OS", not "Light" specifically — leave
        // every button unhighlighted rather than falsely claiming Light was chosen.
        var buttons = document.querySelectorAll('[data-theme-btn]');
        for (var i = 0; i < buttons.length; i++) {
            var btn = buttons[i];
            var isActive = !!theme && btn.getAttribute('data-theme-btn') === theme;
            btn.classList.toggle('fa-theme-btn-active', isActive);
            btn.setAttribute('aria-pressed', isActive ? 'true' : 'false');
        }
    }

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
    };

    // Keeps every <select data-palette-select> (PaletteSwitcher.cs) showing the
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
    window.faSetPalette = function (palette) {
        if (!palette || palette === 'northwest-fall') {
            document.documentElement.removeAttribute('data-fa-palette');
            localStorage.removeItem(PALETTE_STORAGE_KEY);
        } else {
            document.documentElement.setAttribute('data-fa-palette', palette);
            localStorage.setItem(PALETTE_STORAGE_KEY, palette);
        }
        syncPaletteSelects(palette);
    };

    document.addEventListener('DOMContentLoaded', function () {
        markActive(localStorage.getItem(THEME_STORAGE_KEY));

        var storedPalette = localStorage.getItem(PALETTE_STORAGE_KEY);
        if (storedPalette) {
            document.documentElement.setAttribute('data-fa-palette', storedPalette);
        }
        syncPaletteSelects(storedPalette);
    });
})();
