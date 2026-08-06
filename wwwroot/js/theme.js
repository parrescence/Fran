// Theme toggle — plain JS on purpose, not Blazor JS interop. Nothing here needs
// C# state: it's three explicit modes (light / dark / colorblind) stamped as
// data-theme on <html> and persisted to localStorage, read back by the inline
// snippet in index.html <head> before first paint so there's no flash of the
// wrong theme. <ThemeSwitcher> calls window.faSetTheme(...) directly via a plain
// onclick attribute — see its .razor file for why that's fine here.
(function () {
    var STORAGE_KEY = 'fa-theme';

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
        localStorage.setItem(STORAGE_KEY, theme);
        markActive(theme);
    };

    document.addEventListener('DOMContentLoaded', function () {
        markActive(localStorage.getItem(STORAGE_KEY));
    });
})();
