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
        if (storedPalette) {
            document.documentElement.setAttribute('data-fa-palette', storedPalette);
        }
        syncPaletteSelects(storedPalette || document.documentElement.getAttribute('data-fa-palette'));
    }

    document.addEventListener('DOMContentLoaded', syncAll);

    // Blazor (WASM or Server) mounts AppHeader's/PaletteSwitcher's actual DOM
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
