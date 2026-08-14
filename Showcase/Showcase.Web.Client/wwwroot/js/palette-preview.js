// Fills each [data-swatch] element's ".swatch-hex" child with the current computed
// value of the CSS custom property named in its data-swatch attribute — e.g.
// <div data-swatch="--fa-primary"><span class="swatch-hex"></span></div> shows
// whatever --fa-primary currently resolves to. Same "plain vanilla-JS IIFE, no
// Blazor interop" style as theme.js/sidebar.js (see Blazor/CLAUDE.md) — this is
// showcase-only, not part of the library, but follows the same convention since
// it's the same kind of pure client-side visual state.
//
// Self-updating: a MutationObserver on <html>'s data-theme/data-fa-palette
// attributes re-fills the swatches whenever the palette or mode changes, so the
// Palette page updates live if you switch palettes while already on it — no
// Blazor round trip, no re-render needed.
(function () {
    function fillSwatches() {
        document.querySelectorAll('[data-swatch]').forEach(function (el) {
            var varName = el.getAttribute('data-swatch');
            var hexEl = el.querySelector('.swatch-hex');
            if (!hexEl) {
                return;
            }
            var value = getComputedStyle(document.documentElement).getPropertyValue(varName).trim();
            hexEl.textContent = value;
        });
    }

    window.showcasePalettePreview = { fillSwatches: fillSwatches };

    var observer = new MutationObserver(fillSwatches);
    observer.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme', 'data-fa-palette'] });
})();
