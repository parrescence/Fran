// FaDate's compact wheel picker (.fa-date-wheel, see fa-date.md) — plain JS, same
// reasoning as theme.js/sidebar.js, and the one exception to FaDate's own "no JS"
// design (see the class remarks on FaDate.cs). Scrolling a wheel to a new position is
// pure browser behavior (overflow-y + scroll-snap, no JS needed), but knowing which
// item ended up centered once scrolling settles needs real element-position math a
// Blazor scroll event's args don't carry. This only figures out *which* item that is
// and calls .click() on it — the actual selection still runs through that button's own
// Blazor onclick handler (FaDate.SelectWheelValue) exactly as a manual click would, so
// there's no separate JS-to-Blazor interop call here, no state kept in this file at all.
//
// Every position calculation below deliberately uses offsetTop/scrollTop/clientHeight
// (pure layout properties, computed before any transform is applied) rather than
// getBoundingClientRect() (which reports the element's actual *painted* position,
// transform included). Item elements carry a cosmetic transform of their own — see
// updateWheelCurvature — so reading their painted position back would mean each
// frame's correction compounds on top of the last frame's already-shifted position,
// drifting further off with every recompute instead of converging on the right place.
(function () {
    if (window.faDateWheelInit) {
        return;
    }
    window.faDateWheelInit = true;

    // An item's untransformed vertical center, in px from the wheel's own visible
    // top edge — i.e. where it would sit with no cosmetic transform and no scroll
    // offset applied. offsetTop is relative to the nearest positioned ancestor,
    // which is the wheel itself (.fa-date-wheel has position: relative).
    function flowCenter(wheel, item) {
        return (item.offsetTop + (item.offsetHeight / 2)) - wheel.scrollTop;
    }

    function wheelCenter(wheel) {
        return wheel.clientHeight / 2;
    }

    // --- Cylinder curvature: the "spinning wheel" look ---------------------
    // Purely cosmetic, no bearing on which value is selected (settle() below
    // picks that from flow position alone, never from how a transform painted
    // an item). Each item gets tilted/pushed back/faded by how many "steps" it
    // sits from the center row, so the list reads as a barrel curving away
    // above and below the highlighted band instead of a flat scrolling list —
    // the same illusion a native iOS/Android picker wheel uses. Recomputed
    // continuously while scrolling (rAF-throttled below, not on every raw
    // scroll event) so the curve visibly animates as the wheel spins, not just
    // once it settles.
    var WHEEL_STEP_ANGLE_DEG = 26;
    // Radius of the imaginary cylinder each item's center rides along —
    // translateY below repositions every item onto that curve (R*sin(angle))
    // instead of leaving it at its native, evenly-spaced flow position.
    var WHEEL_RADIUS_PX = 62;
    var WHEEL_MIN_SCALE = 0.72;
    // Capped at 2, not 3 — the wheel's own height only comfortably shows about
    // two rows above/below the highlighted band before overflow clips them
    // anyway; tilting a 3rd row just crowded/overlapped text that was already
    // sitting right at that clipped edge.
    var WHEEL_MAX_STEPS = 2;
    var WHEEL_MIN_OPACITY = 0.1;
    var DEG_TO_RAD = Math.PI / 180;

    function updateWheelCurvature(wheel) {
        var items = wheel.querySelectorAll('.fa-date-wheel-item');
        if (!items.length) {
            return;
        }

        var center = wheelCenter(wheel);
        var itemHeight = items[0].offsetHeight || 1;

        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            var flowOffset = flowCenter(wheel, item) - center;
            var steps = flowOffset / itemHeight;
            var clampedSteps = Math.max(-WHEEL_MAX_STEPS, Math.min(WHEEL_MAX_STEPS, steps));
            var angleRad = clampedSteps * WHEEL_STEP_ANGLE_DEG * DEG_TO_RAD;

            // Where the item should *appear* to sit on the cylinder, and the
            // translateY needed to move it there from its actual flow position.
            var curvedOffset = WHEEL_RADIUS_PX * Math.sin(angleRad);
            var translateY = curvedOffset - flowOffset;
            var scale = Math.max(WHEEL_MIN_SCALE, Math.cos(angleRad));
            var opacity = Math.max(WHEEL_MIN_OPACITY, 1 - (Math.abs(steps) * 0.4));

            item.style.transform = 'translateY(' + translateY + 'px) rotateX(' + (-clampedSteps * WHEEL_STEP_ANGLE_DEG) + 'deg) scale(' + scale + ')';
            item.style.opacity = String(opacity);
        }
    }

    // rAF-throttled per wheel — 'scroll' can fire dozens of times a frame on
    // some browsers/input methods, and re-measuring every item that often is
    // wasted work the eye can't tell apart from once-per-frame anyway.
    var curvatureFramePending = new WeakMap();

    function scheduleCurvatureUpdate(wheel) {
        if (curvatureFramePending.get(wheel)) {
            return;
        }

        curvatureFramePending.set(wheel, true);
        requestAnimationFrame(function () {
            curvatureFramePending.set(wheel, false);
            updateWheelCurvature(wheel);
        });
    }

    // --- Infinite wrap for Day/Month: 31 -> 1 and back, no matter the spin
    // direction ------------------------------------------------------------
    // FaDate.cs renders a cyclic wheel's item list three times end-to-end
    // (see RenderWheelColumn's remarks) instead of once. All three copies are
    // pixel-identical, so whenever scrolling drifts deep into the first or
    // last copy, silently jumping scrollTop by exactly one copy's height lands
    // on the same visual item in the middle copy — invisible to the eye, but
    // it buys another full copy's worth of scroll room before the *real* DOM
    // edge is ever reached. Runs on every scroll event (native wheel-scroll,
    // drag, or momentum below all fire one), not just at rest, so a long fast
    // flick gets rewound mid-flight instead of only once it settles.
    var WHEEL_CYCLE_COUNT = 3;
    var WHEEL_CYCLIC_CLASS = 'fa-date-wheel-cyclic';

    // One copy's height for a cyclic wheel (null for a non-cyclic one, or one
    // that hasn't laid out any items yet) — shared by checkWrap() and
    // centerSelected()'s nearest-copy math below.
    function cyclicCopyHeight(wheel) {
        if (!wheel.classList.contains(WHEEL_CYCLIC_CLASS)) {
            return null;
        }

        var items = wheel.querySelectorAll('.fa-date-wheel-item');
        if (items.length < WHEEL_CYCLE_COUNT) {
            return null;
        }

        var itemHeight = items[0].offsetHeight || 1;
        var perCycle = items.length / WHEEL_CYCLE_COUNT;
        var cycleHeight = perCycle * itemHeight;
        return cycleHeight > 0 ? cycleHeight : null;
    }

    function checkWrap(wheel) {
        var cycleHeight = cyclicCopyHeight(wheel);
        if (cycleHeight === null) {
            return;
        }

        // Valid range spans 3 copies: [0, 3*cycleHeight]. Recentering whenever
        // scrollTop strays past the half-copy mark on either side of the
        // middle copy keeps at least half a copy's buffer in both directions
        // at all times.
        if (wheel.scrollTop < cycleHeight * 0.5) {
            wheel.scrollTop += cycleHeight;
        } else if (wheel.scrollTop > cycleHeight * 1.5) {
            wheel.scrollTop -= cycleHeight;
        }
    }

    // --- Bounded wheels: stop at the real choices, don't scroll into the
    // disabled buffer around them --------------------------------------------
    // FaDate.cs marks a wheel .fa-date-wheel-bounded whenever Min/Max leave
    // more than one selectable item but still fewer than the wheel renders —
    // Year almost always (it pads 15 disabled years on either side of Min/Max
    // just so a single valid year can still reach dead center, see FaDate.cs's
    // RenderWheels remarks), or Month/Day narrowed to a handful of real
    // choices. Without this, letting go mid-scroll would still land back on a
    // real value (settle() already skips disabled items), but getting there
    // meant scrolling through however much disabled padding sat past it first
    // — "the only two years reachable end up being the two valid ones, but
    // only after scrolling through a decade of years that were never going to
    // stick" reads as broken, not just decorative dimming. Clamping scrollTop
    // to wherever the nearest in-range item sits stops the wheel exactly at
    // the real choices instead, the same idea as a native OS picker that
    // refuses to spin past its own end stops.
    var WHEEL_BOUNDED_CLASS = 'fa-date-wheel-bounded';

    function clampToEnabledRange(wheel) {
        var allItems = wheel.querySelectorAll('.fa-date-wheel-item');
        if (!allItems.length) {
            return;
        }

        var cycleHeight = cyclicCopyHeight(wheel);
        var perCycle = cycleHeight !== null ? (allItems.length / WHEEL_CYCLE_COUNT) : allItems.length;

        // A cyclic (Month/Day) wheel only cares about the *middle* copy's own
        // enabled items — FaDate.cs only ever marks that one copy's matching
        // item fa-date-wheel-item-selected (see RenderWheelColumn's remarks),
        // so clamping against any other copy's boundary could leave the wheel
        // resting on a copy that never reads as selected at all. A non-cyclic
        // wheel (Year) has just the one copy, so this is simply every item.
        var rangeStart = cycleHeight !== null ? perCycle : 0;
        var rangeEnd = rangeStart + perCycle;

        var first = null;
        var last = null;
        for (var i = rangeStart; i < rangeEnd; i++) {
            if (!allItems[i].disabled) {
                if (!first) {
                    first = allItems[i];
                }
                last = allItems[i];
            }
        }

        // Nothing enabled in range at all (shouldn't happen — RenderWheels
        // always keeps at least one item selectable — but bail rather than
        // clamp to a nonsensical empty range).
        if (!first || !last) {
            return;
        }

        var center = wheelCenter(wheel);
        var minScrollTop = (first.offsetTop + (first.offsetHeight / 2)) - center;
        var maxScrollTop = (last.offsetTop + (last.offsetHeight / 2)) - center;

        if (wheel.scrollTop < minScrollTop) {
            wheel.scrollTop = minScrollTop;
        } else if (wheel.scrollTop > maxScrollTop) {
            wheel.scrollTop = maxScrollTop;
        }
    }

    // --- Mouse/touch drag-to-spin -------------------------------------------
    // Native touch-panning already moves a wheel's scrollTop for free, but it
    // never fires for a mouse, and it can't be steered mid-scroll the way
    // checkWrap() above needs — a plain OS-driven touch fling finishes
    // wherever its own momentum happens to land, with no chance to rewrite
    // scrollTop along the way. Routing every pointer type (mouse, touch, pen)
    // through Pointer Events instead gives one code path for both: a mouse can
    // now spin the wheel by dragging, and touch gets the same capped-speed
    // momentum below instead of whatever fling curve the OS would otherwise
    // apply (see _date.scss's touch-action:none, which hands touch panning
    // over to this code entirely).
    var activeDrag = null;
    var DRAG_CLICK_THRESHOLD_PX = 6;
    // Caps how fast a released flick starts decelerating from — keeps even a
    // very fast real-world swipe reading as one trackable spin instead of a
    // blur, per the "reasonable human visual cognition" ask this exists for.
    var MAX_FLING_VELOCITY_PX_MS = 1.1;
    var FLING_FRICTION = 0.94;
    var FLING_STOP_THRESHOLD_PX = 0.4;

    function wheelFromEvent(e) {
        var target = e.target;
        return target && target.closest ? target.closest('.fa-date-wheel') : null;
    }

    function stopMomentum(wheel) {
        if (wheel._faMomentumFrame) {
            cancelAnimationFrame(wheel._faMomentumFrame);
            wheel._faMomentumFrame = null;
        }
    }

    function runMomentum(wheel, pxPerFrame) {
        wheel.scrollTop -= pxPerFrame;
        pxPerFrame *= FLING_FRICTION;

        if (Math.abs(pxPerFrame) > FLING_STOP_THRESHOLD_PX) {
            wheel._faMomentumFrame = requestAnimationFrame(function () {
                runMomentum(wheel, pxPerFrame);
            });
        } else {
            wheel._faMomentumFrame = null;
        }
    }

    document.addEventListener('pointerdown', function (e) {
        // Left button only for a mouse — a right/middle click shouldn't start
        // a drag. Touch/pen have no meaningful e.button to gate on.
        if (e.pointerType === 'mouse' && e.button !== 0) {
            return;
        }

        var wheel = wheelFromEvent(e);
        if (!wheel) {
            return;
        }

        stopMomentum(wheel);
        activeDrag = {
            wheel: wheel,
            pointerId: e.pointerId,
            startY: e.clientY,
            startScrollTop: wheel.scrollTop,
            lastY: e.clientY,
            lastTime: e.timeStamp,
            velocity: 0,
            moved: false
        };

        try {
            wheel.setPointerCapture(e.pointerId);
        } catch (err) {
            // Some pointer types/browsers can reject capture; the drag still
            // works via document-level listeners either way.
        }
    }, true);

    document.addEventListener('pointermove', function (e) {
        if (!activeDrag || activeDrag.pointerId !== e.pointerId) {
            return;
        }

        // Content follows the finger/cursor: dragging down (positive deltaY)
        // moves scrollTop backward, revealing earlier items — the same feel
        // as a native touch scroll.
        var deltaY = e.clientY - activeDrag.startY;
        if (!activeDrag.moved && Math.abs(deltaY) > DRAG_CLICK_THRESHOLD_PX) {
            activeDrag.moved = true;
        }

        activeDrag.wheel.scrollTop = activeDrag.startScrollTop - deltaY;

        var dt = e.timeStamp - activeDrag.lastTime;
        if (dt > 0) {
            activeDrag.velocity = (e.clientY - activeDrag.lastY) / dt;
        }
        activeDrag.lastY = e.clientY;
        activeDrag.lastTime = e.timeStamp;
    }, true);

    function endDrag(e) {
        if (!activeDrag || activeDrag.pointerId !== e.pointerId) {
            return;
        }

        var drag = activeDrag;
        activeDrag = null;

        try {
            drag.wheel.releasePointerCapture(drag.pointerId);
        } catch (err) {
            // Already released/never captured — nothing to clean up.
        }

        if (!drag.moved) {
            return;
        }

        // Suppress the click the browser synthesizes on whichever item the
        // pointer happens to be sitting over once the drag ends — without
        // this, ending a drag mid-item commits it as if it had been
        // deliberately tapped instead of just passed over mid-spin.
        drag.wheel._faSuppressClick = true;

        var velocity = Math.max(-MAX_FLING_VELOCITY_PX_MS, Math.min(MAX_FLING_VELOCITY_PX_MS, drag.velocity));
        if (Math.abs(velocity) > 0.03) {
            // Seed momentum at roughly one frame's worth of the capped
            // release velocity; runMomentum's own friction decays it from there.
            runMomentum(drag.wheel, velocity * 16);
        }
    }

    document.addEventListener('pointerup', endDrag, true);
    document.addEventListener('pointercancel', endDrag, true);

    document.addEventListener('click', function (e) {
        var wheel = wheelFromEvent(e);
        if (wheel && wheel._faSuppressClick) {
            wheel._faSuppressClick = false;
            e.preventDefault();
            e.stopPropagation();
        }
    }, true);

    var settleTimers = new WeakMap();
    var SETTLE_DELAY_MS = 130;

    function settle(wheel) {
        // Skip while the wheel has no layout box — the popup starts out
        // `display: none` (see _date.scss) but its wheels are still mounted
        // in the DOM (FaDate.cs never conditionally renders them), so the
        // very first "mark today's value selected" render already fires the
        // mount-time centering path below, on a hidden element whose
        // offsetTop/clientHeight all read 0. That 0-based math briefly (and
        // wrongly) leaves scrollTop at 0, which still fires a real 'scroll'
        // event — and without this guard, this function would go on to find
        // whatever's "closest" to that bogus position and .click() it,
        // permanently overwriting the correct default (today, or the bound
        // Value) with an unrelated nearby value before the popup was ever
        // opened. offsetParent is null for any display:none ancestor, a
        // reliable cheap visibility check with no layout thrash of its own.
        if (wheel.offsetParent === null) {
            return;
        }

        // Skip while the user is actively typing into one of the number inputs
        // above the wheels — a layout shift from that typing (an on-screen
        // keyboard opening, the popup repositioning) can fire an incidental
        // 'scroll' event on a wheel that never asked to be scrolled, and
        // clicking whatever ends up nearest-to-center from that would commit a
        // value straight past the "wait for a complete value" guard
        // TryCommitValue applies to the input itself (see FaDate.cs) — a wheel
        // click is a deliberate selection with no such guard, by design. Only
        // suppress it while a wheel-input actually has focus, not just because
        // the popup is open, so a genuine hand-scroll still settles normally.
        var group = wheel.closest('.fa-date-wheels-group');
        var active = document.activeElement;
        if (group && active && active.classList && active.classList.contains('fa-date-wheel-input') && group.contains(active)) {
            return;
        }

        var items = wheel.querySelectorAll('.fa-date-wheel-item');
        if (!items.length) {
            return;
        }

        var center = wheelCenter(wheel);
        var closest = null;
        var closestDistance = Infinity;

        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            // A Day wheel always renders all 31 slots and disables whichever
            // tail the current month doesn't have (see RenderWheelColumn's
            // remarks) — skip those entirely so a spin can never come to rest
            // on, say, Feb 30. The browser already refuses to click a disabled
            // button on its own, but resting there and never settling reads as
            // broken rather than just non-interactive.
            if (item.disabled) {
                continue;
            }

            var distance = Math.abs(flowCenter(wheel, item) - center);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = item;
            }
        }

        // Already selected: skip the click. Re-clicking a value that's already
        // committed is harmless either way, but this avoids the extra Blazor
        // render every time a wheel merely comes to rest back where it started.
        if (closest && !closest.classList.contains('fa-date-wheel-item-selected')) {
            closest.click();
        }
    }

    document.addEventListener('scroll', function (event) {
        var wheel = event.target;
        if (!wheel || !wheel.classList || !wheel.classList.contains('fa-date-wheel')) {
            return;
        }

        // Bounded (Year almost always, or a narrowed Month/Day) stops at its
        // own real choices instead of wrapping/scrolling into the disabled
        // buffer around them — see clampToEnabledRange's own remarks. A
        // wheel is never both bounded and cyclic-wrap-eligible at once: a
        // bounded cyclic wheel (Month/Day genuinely narrowed by Min/Max) has
        // nowhere sensible left to wrap *to*.
        if (wheel.classList.contains(WHEEL_BOUNDED_CLASS)) {
            clampToEnabledRange(wheel);
        } else {
            checkWrap(wheel);
        }
        scheduleCurvatureUpdate(wheel);

        var pending = settleTimers.get(wheel);
        if (pending) {
            clearTimeout(pending);
        }

        settleTimers.set(wheel, setTimeout(function () {
            settleTimers.delete(wheel);
            settle(wheel);
        }, SETTLE_DELAY_MS));
    }, true);

    // The other direction: whenever some *other* change makes a different item the
    // selected one — typing a value into the number input above a wheel, opening
    // the compact popup, switching into compact mode — that item needs to scroll
    // into the highlighted middle band too, the same way landing there by hand-
    // scrolling already does above. FaDate used to do this from C# via
    // ElementReference.FocusAsync() (scrolling into view is a side effect of
    // focusing), but FocusAsync also moves real keyboard focus — firing it after
    // every keystroke yanked focus off the input the user was actively typing
    // into and onto the wheel button instead, so a second digit had nowhere to
    // land. A MutationObserver watching for the "selected" class sidesteps that
    // entirely, and reacts to *any* path that can change which item is selected —
    // typed value, wheel click/scroll-settle above, or initial popup open — with
    // one listener instead of each caller having to remember to ask for a
    // re-center. Scrolling is done via a direct scrollTop assignment (flow-based
    // math, same as everywhere else in this file) rather than
    // item.scrollIntoView() — scrollIntoView reads the element's actual painted
    // position, which the curvature transform above deliberately moves away from
    // its true flow position, so it would center the cosmetically-shifted box
    // rather than the item's real slot.
    var SELECTED_CLASS = 'fa-date-wheel-item-selected';

    function centerSelected(item) {
        var wheel = item && item.closest ? item.closest('.fa-date-wheel') : null;
        if (!wheel) {
            return;
        }

        // Nothing to compute correctly yet on a hidden wheel — offsetTop/
        // clientHeight all read 0 behind `display: none` (see settle()'s own
        // remarks for why this matters beyond just "don't bother"), and the
        // OPEN_CLASS branch below already re-runs this same centering once
        // the popup is actually visible, so skipping now loses nothing.
        if (wheel.offsetParent === null) {
            return;
        }

        var target = (item.offsetTop + (item.offsetHeight / 2)) - wheelCenter(wheel);

        // FaDate.cs only ever puts the SELECTED_CLASS on one specific copy of
        // a cyclic (Day/Month) wheel's item list — the middle one (see
        // RenderWheelColumn's remarks) — regardless of which copy the user was
        // actually looking at when the value committed (settle()/a wheel click
        // can select an item from the first or last copy just as easily). Used
        // as-is, `target` would always be that one fixed copy's position, so a
        // selection made while resting in a different copy would jump the
        // wheel a whole copy-height away instead of leaving it where the user
        // left it. Every copy holds the same item at the same offset mod one
        // copy's height, so nudging `target` by whichever whole-copy multiple
        // lands closest to the wheel's *current* scrollTop picks the copy the
        // user was already looking at — a same-value target, just the nearby
        // copy of it instead of always the fixed middle one.
        var cycleHeight = cyclicCopyHeight(wheel);
        if (cycleHeight !== null) {
            var laps = Math.round((wheel.scrollTop - target) / cycleHeight);

            // The naive nearest-lap pick above can land outside the wheel's
            // actual scrollable range — most commonly at mount, before the
            // user has ever scrolled anything, when scrollTop is still 0 and
            // rounding "nearest to 0" for a target a cycle-and-a-half deep
            // rounds down a whole cycle too far into negative territory.
            // Nothing stops that assignment from "succeeding" — the browser
            // just silently clamps scrollTop to 0 for you — but a *clamped*
            // scrollTop no longer centers the copy this function just picked;
            // it centers whatever else happens to sit at the very top of the
            // list instead. Left uncorrected, settle()'s own 130ms-later
            // pass then finds that unrelated item as "closest", sees it
            // isn't the one already marked selected, and clicks it —
            // permanently overwriting the correct value (today's date, or
            // the bound Value) with whatever was physically nearest to a
            // scroll position nobody actually chose. Walking `laps` back
            // into range keeps the *intended* item's copy centered instead.
            var maxScrollTop = wheel.scrollHeight - wheel.clientHeight;
            while (target + laps * cycleHeight < 0) {
                laps += 1;
            }
            while (target + laps * cycleHeight > maxScrollTop) {
                laps -= 1;
            }

            target += laps * cycleHeight;
        }

        // CSS scroll-snap-type (see _date.scss) still owns the resting point
        // for a plain native scroll (mouse-wheel/trackpad over the wheel), so
        // it stays on outside of this. But the drag/settle path just ended a
        // *real* browser scroll gesture (whatever the user's finger/momentum
        // did) before this correction ever runs — the browser can still
        // re-snap back toward that gesture's own resting point on its own
        // schedule, silently undoing the scrollTop this function just set.
        // Suspending snap for one frame around the write removes that race:
        // nothing native is left mid-gesture for the browser to re-snap
        // toward once it's back on.
        var previousSnapType = wheel.style.scrollSnapType;
        wheel.style.scrollSnapType = 'none';
        wheel.scrollTop = target;
        checkWrap(wheel);
        scheduleCurvatureUpdate(wheel);

        requestAnimationFrame(function () {
            wheel.style.scrollSnapType = previousSnapType;
        });
    }

    function centerAllSelectedWithin(container) {
        if (typeof container.querySelectorAll !== 'function') {
            return;
        }

        var items = container.querySelectorAll('.' + SELECTED_CLASS);
        for (var i = 0; i < items.length; i++) {
            centerSelected(items[i]);
        }
    }

    // The popup's own wheels stay mounted in the DOM even while closed (CSS
    // hides them, FaDate doesn't conditionally render them) — so opening the
    // popup, or switching the header's mode toggle into compact, only flips a
    // class on the calendar container rather than creating any new
    // fa-date-wheel-item-selected node for the observer's other branches to
    // catch. Treat that class landing as its own trigger to re-center
    // everything inside, the one case nothing else here already covers.
    var OPEN_CLASS = 'fa-date-calendar-open';

    var wheelObserver = new MutationObserver(function (mutations) {
        for (var i = 0; i < mutations.length; i++) {
            var mutation = mutations[i];
            if (mutation.type === 'attributes') {
                var target = mutation.target;
                if (!target.classList) {
                    continue;
                }

                if (target.classList.contains(SELECTED_CLASS)) {
                    centerSelected(target);
                }

                if (target.classList.contains(OPEN_CLASS)) {
                    // One frame later: the popup goes display:none -> block in
                    // this same class change, with a pop-in animation attached
                    // (see .fa-date-calendar-open in _date.scss) — offsetTop/
                    // clientHeight read real layout geometry, which a
                    // transform/opacity animation doesn't affect, but waiting a
                    // frame costs nothing and guarantees the browser's laid the
                    // newly-visible content out at all before measuring it.
                    requestAnimationFrame(function () {
                        centerAllSelectedWithin(target);
                        var wheels = target.querySelectorAll('.fa-date-wheel');
                        for (var w = 0; w < wheels.length; w++) {
                            updateWheelCurvature(wheels[w]);
                        }
                    });
                }
            } else if (mutation.type === 'childList') {
                for (var n = 0; n < mutation.addedNodes.length; n++) {
                    var node = mutation.addedNodes[n];
                    if (node.nodeType !== 1) {
                        continue;
                    }

                    if (node.classList && node.classList.contains(SELECTED_CLASS)) {
                        centerSelected(node);
                    } else if (typeof node.querySelector === 'function') {
                        centerSelected(node.querySelector('.' + SELECTED_CLASS));
                    }

                    // Switching the header's mode toggle into compact inserts
                    // each wheel's items for the first time — give them their
                    // curve immediately instead of leaving them flat until the
                    // first hand-scroll.
                    if (node.classList && node.classList.contains('fa-date-wheel')) {
                        updateWheelCurvature(node);
                    } else if (typeof node.querySelectorAll === 'function') {
                        var newWheels = node.querySelectorAll('.fa-date-wheel');
                        for (var w2 = 0; w2 < newWheels.length; w2++) {
                            updateWheelCurvature(newWheels[w2]);
                        }
                    }
                }
            }
        }
    });

    wheelObserver.observe(document.body, {
        attributes: true,
        attributeFilter: ['class'],
        childList: true,
        subtree: true
    });
})();
