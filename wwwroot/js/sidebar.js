// Sidebar collapse toggle — plain JS on purpose, same reasoning as theme.js: this is
// pure client-side UI state (collapsed vs expanded), persisted to localStorage and
// stamped as a class on <html> so CSS alone handles the width/tooltip/icon-rotation
// changes. AppSidebar's toggle button calls window.faToggleSidebar() directly via a
// plain onclick attribute — no Blazor state to keep in sync, nothing to re-render.
(function () {
    var STORAGE_KEY = 'fa-sidebar-collapsed';
    var CLASS_NAME = 'fa-sidebar-collapsed';
    var SETTLED_CLASS_NAME = 'fa-sidebar-collapsed-settled';
    // Matches .fa-sidebar-link-text's max-width transition duration in theme.css —
    // justify-content isn't an animatable CSS property, so centering the icon has to
    // be a discrete class added once the label has actually finished collapsing,
    // not the instant collapse starts (that read as the icon jumping to center
    // before the text had visually finished closing over).
    var SETTLE_DELAY_MS = 200;
    var settleTimer = null;

    function apply(collapsed, immediate) {
        document.documentElement.classList.toggle(CLASS_NAME, collapsed);
        var buttons = document.querySelectorAll('[data-sidebar-toggle]');
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].setAttribute('aria-expanded', collapsed ? 'false' : 'true');
        }

        if (settleTimer) {
            clearTimeout(settleTimer);
            settleTimer = null;
        }

        if (!collapsed) {
            // Expanding: un-center immediately so the icon moves back to the left as
            // the label grows back in, instead of staying centered mid-animation.
            document.documentElement.classList.remove(SETTLED_CLASS_NAME);
            return;
        }

        if (immediate) {
            document.documentElement.classList.add(SETTLED_CLASS_NAME);
        } else {
            settleTimer = setTimeout(function () {
                document.documentElement.classList.add(SETTLED_CLASS_NAME);
                settleTimer = null;
            }, SETTLE_DELAY_MS);
        }
    }

    window.faToggleSidebar = function () {
        var collapsed = document.documentElement.classList.contains(CLASS_NAME);
        var next = !collapsed;
        localStorage.setItem(STORAGE_KEY, next ? '1' : '0');
        apply(next, false);
    };

    document.addEventListener('DOMContentLoaded', function () {
        // Applying the saved state on page load should never animate — only the
        // interactive toggle click gets the delayed settle.
        apply(localStorage.getItem(STORAGE_KEY) === '1', true);
    });
})();
