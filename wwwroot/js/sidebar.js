// Sidebar collapse toggle — plain JS on purpose, same reasoning as theme.js: this is
// pure client-side UI state (collapsed vs expanded), persisted to localStorage and
// stamped as a class on <html> so CSS alone handles the width/tooltip/icon-rotation
// changes. AppSidebar's toggle button calls window.faToggleSidebar() directly via a
// plain onclick attribute — no Blazor state to keep in sync, nothing to re-render.
(function () {
    var STORAGE_KEY = 'fa-sidebar-collapsed';
    var CLASS_NAME = 'fa-sidebar-collapsed';

    function apply(collapsed) {
        document.documentElement.classList.toggle(CLASS_NAME, collapsed);
        var buttons = document.querySelectorAll('[data-sidebar-toggle]');
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].setAttribute('aria-expanded', collapsed ? 'false' : 'true');
        }
    }

    window.faToggleSidebar = function () {
        var collapsed = document.documentElement.classList.contains(CLASS_NAME);
        var next = !collapsed;
        localStorage.setItem(STORAGE_KEY, next ? '1' : '0');
        apply(next);
    };

    document.addEventListener('DOMContentLoaded', function () {
        apply(localStorage.getItem(STORAGE_KEY) === '1');
    });
})();
