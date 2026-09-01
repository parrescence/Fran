// FaCodeBlock's copy-to-clipboard button — plain JS on purpose, not Blazor JS
// interop. navigator.clipboard.writeText has no Blazor-native equivalent, so this
// is one of the few things in this library that genuinely can't be expressed in
// Blazor's own event model (see FaCodeBlock.cs's remarks). The button calls
// window.faCopyCodeBlock(this) directly via a plain onclick attribute, the same
// pattern theme.js's faSetTheme/faSetPalette use.
(function () {
    'use strict';

    // Fallback for non-secure contexts (plain http, where navigator.clipboard is
    // unavailable) — a hidden, offscreen textarea + document.execCommand('copy') is
    // the standard workaround. Deprecated, but still broadly supported, and only
    // reached when the modern API genuinely isn't there.
    function legacyCopy(text) {
        var textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.setAttribute('readonly', '');
        textarea.style.position = 'fixed';
        textarea.style.left = '-9999px';
        document.body.appendChild(textarea);
        textarea.select();
        try {
            document.execCommand('copy');
        } catch (e) {
            // Nothing more to do — the button's "Copied!" feedback simply won't
            // reflect a real success in this edge case.
        }
        document.body.removeChild(textarea);
    }

    function showCopiedFeedback(button) {
        var label = button.querySelector('.fa-codeblock-copy-label');
        var original = label ? label.textContent : null;
        button.classList.add('fa-codeblock-copied');
        if (label) {
            label.textContent = 'Copied!';
        }
        // Reset any in-flight timer from a rapid double-click before starting a new
        // one, so the label doesn't flip back to "Copy" mid-way through a second click.
        clearTimeout(button._faCopyResetTimer);
        button._faCopyResetTimer = setTimeout(function () {
            button.classList.remove('fa-codeblock-copied');
            if (label && original !== null) {
                label.textContent = original;
            }
        }, 1600);
    }

    window.faCopyCodeBlock = function (button) {
        var targetId = button.getAttribute('data-copy-target');
        var codeEl = targetId && document.getElementById(targetId);
        if (!codeEl) {
            return;
        }
        // innerText, not textContent — respects the rendered line breaks in the
        // <pre><code> block rather than collapsing them.
        var text = codeEl.innerText;

        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(
                function () { showCopiedFeedback(button); },
                function () { legacyCopy(text); showCopiedFeedback(button); }
            );
        } else {
            legacyCopy(text);
            showCopiedFeedback(button);
        }
    };
})();
