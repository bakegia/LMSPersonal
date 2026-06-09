// Helpers for preloader and view animations
// Usage: include this script and call StudentLayout.init(options)
(function (global) {
    'use strict';

    const defaults = {
        contentSelector: '#main-content-area .main-content > *',
        linkSelector: '.nav-links a',
        activeClass: 'active',
        defaultAnimation: 'view-fade-in'
    };

    // create preload markup if not present
    function ensurePreloader() {
        if (document.querySelector('.preload-container')) return;

        const container = document.createElement('div');
        container.className = 'preload-container preloading';

        const card = document.createElement('div');
        card.className = 'preload-card';

        const icon = document.createElement('div');
        icon.className = 'preload-icon rotating';
        icon.innerHTML = '<svg width="28" height="28" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M12 2v4" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/><path d="M12 18v4" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/><path d="M4.9 4.9l2.8 2.8" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/><path d="M16.3 16.3l2.8 2.8" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/></svg>';

        const text = document.createElement('div');
        text.className = 'preload-text';
        text.textContent = 'Đang tải...';

        const dots = document.createElement('div');
        dots.className = 'preload-dots';
        dots.innerHTML = '<span></span><span></span><span></span>';

        card.appendChild(icon);
        card.appendChild(text);
        card.appendChild(dots);
        container.appendChild(card);
        document.body.appendChild(container);
    }

    function showPreloader(message) {
        ensurePreloader();
        const container = document.querySelector('.preload-container');
        if (!container) return;
        const txt = container.querySelector('.preload-text');
        if (txt && message) txt.textContent = message;
        container.style.display = 'flex';
        document.documentElement.classList.add('is-preloading');
        document.body.style.overflow = 'hidden';
    }

    function hidePreloader() {
        const container = document.querySelector('.preload-container');
        if (!container) return;
        container.style.display = 'none';
        document.documentElement.classList.remove('is-preloading');
        document.body.style.overflow = '';
    }

    // Persist sidebar scroll position across full page navigations using sessionStorage
    function saveSidebarScroll() {
        try {
            var side = document.querySelector('.sidebar');
            if (!side) return;
            sessionStorage.setItem('student.sidebar.scroll', String(side.scrollTop || 0));
        } catch (e) { /* ignore */ }
    }

    function restoreSidebarScroll() {
        try {
            var side = document.querySelector('.sidebar');
            if (!side) return;
            var pos = sessionStorage.getItem('student.sidebar.scroll');
            if (pos !== null) side.scrollTop = parseInt(pos, 10) || 0;
        } catch (e) { /* ignore */ }
    }

    // Apply animation class to a node and remove it after animation ends
    function applyAnimation(node, animationClass) {
        if (!node) return;
        const classes = ['view-fade-in', 'view-slide-up'];
        classes.forEach(c => node.classList.remove(c));
        // force reflow to restart animation
        void node.offsetWidth;
        node.classList.add(animationClass || defaults.defaultAnimation);
    }

    // Initialize behavior: set active link and optionally fetch remote content
    function init(opt) {
        const config = Object.assign({}, defaults, opt || {});
        const contentEl = document.querySelector(config.contentSelector);

        // restore sidebar scroll position (if user navigated away previously)
        try { restoreSidebarScroll(); } catch (e) { /* ignore */ }

        // save sidebar scroll before leaving the page so it can be restored
        window.addEventListener('beforeunload', function () { try { saveSidebarScroll(); } catch (e) { } });

        // delegate clicks on nav links
        document.addEventListener('click', function (e) {
            const a = e.target.closest && e.target.closest(config.linkSelector);
            if (!a) return;

            // set active class (visual immediate feedback)
            document.querySelectorAll(config.linkSelector).forEach(function (lnk) {
                lnk.classList.remove(config.activeClass);
            });
            a.classList.add(config.activeClass);

            // determine if we should load via AJAX
            const url = a.getAttribute('data-url') || a.getAttribute('href');
            const ajax = a.getAttribute('data-ajax') === 'true';

            // If not an AJAX link, allow normal navigation to proceed
            if (!ajax) {
                return;
            }

            // Intercept navigation for AJAX links
            e.preventDefault();

            if (ajax && url && contentEl) {
                showPreloader('Đang tải...');
                fetch(url, { credentials: 'same-origin' })
                    .then(function (res) {
                        if (!res.ok) throw new Error('Network response was not ok');
                        return res.text();
                    })
                    .then(function (html) {
                        // inject content and animate
                        contentEl.innerHTML = html;
                        applyAnimation(contentEl, config.defaultAnimation);
                    })
                    .catch(function (err) {
                        console.error(err);
                        // fall back: navigate normally
                        window.location.href = url;
                    })
                    .finally(function () { hidePreloader(); });
            } else if (url) {
                // no content area, perform normal navigation
                window.location.href = url;
            }
        }, false);
    }

    global.StudentLayout = {
        init: init,
        showPreloader: showPreloader,
        hidePreloader: hidePreloader,
        applyAnimation: applyAnimation
    };

})(window);

// End of student-layout.js
