// ============================================================================
// Cugger custom controls: AjaxSearch, AutocompleteSelect, DateTimePicker
// Pure vanilla JS, no jQuery (validation script uses jQuery, ovo je samostalno).
// ============================================================================

(function () {
    'use strict';

    // ---------------- helpers ----------------
    function debounce(fn, ms) {
        let t;
        return function () {
            const args = arguments, ctx = this;
            clearTimeout(t);
            t = setTimeout(() => fn.apply(ctx, args), ms);
        };
    }

    function escapeHtml(s) {
        if (s == null) return '';
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // navigator.language → "hr" or "en" (everything else falls back to en)
    function getLocale() {
        const lang = (navigator.language || 'hr').toLowerCase();
        return lang.startsWith('hr') ? 'hr' : 'en';
    }

    // =================================================================
    // 1) AutocompleteSelect — type, fetch via AJAX, click to pick
    // =================================================================
    function initAutocomplete(root) {
        const endpoint = root.dataset.endpoint;
        const minChars = parseInt(root.dataset.minChars || '1', 10);
        const hidden   = root.querySelector('.ac-value');
        const input    = root.querySelector('.ac-input');
        const list     = root.querySelector('.ac-results');
        const clearBtn = root.querySelector('.ac-clear');
        const spinner  = root.querySelector('.ac-spinner');

        let activeIndex = -1;
        let lastResults = [];
        let aborter = null;

        function open()  { list.classList.add('ac-open'); }
        function close() { list.classList.remove('ac-open'); activeIndex = -1; }

        function render(items, query) {
            list.innerHTML = '';
            lastResults = items;
            if (!items || !items.length) {
                list.innerHTML = '<li class="ac-empty">Nema rezultata.</li>';
                open();
                return;
            }
            items.forEach((item, idx) => {
                const li = document.createElement('li');
                li.className = 'ac-result';
                li.setAttribute('role', 'option');
                li.dataset.id = item.id;
                li.dataset.label = item.label;

                const main = document.createElement('div');
                main.className = 'ac-result-main';
                main.innerHTML = highlight(escapeHtml(item.label), query);

                const sub = document.createElement('div');
                sub.className = 'ac-result-sub';
                sub.textContent = item.subLabel || '';

                li.appendChild(main);
                if (item.subLabel) li.appendChild(sub);

                li.addEventListener('mousedown', (e) => {
                    e.preventDefault(); // prevent input blur before click
                    pick(idx);
                });
                list.appendChild(li);
            });
            open();
        }

        function highlight(text, q) {
            if (!q) return text;
            try {
                const re = new RegExp('(' + q.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'ig');
                return text.replace(re, '<mark>$1</mark>');
            } catch (_) { return text; }
        }

        function pick(idx) {
            const it = lastResults[idx];
            if (!it) return;
            hidden.value = it.id;
            input.value = it.label;
            close();
            // Trigger change event so any form validation hooks fire
            hidden.dispatchEvent(new Event('change', { bubbles: true }));
            input.dispatchEvent(new Event('blur', { bubbles: true }));
        }

        async function fetchResults(q) {
            if (aborter) aborter.abort();
            aborter = new AbortController();
            spinner.classList.add('ac-spinner-active');
            try {
                const url = endpoint + '?q=' + encodeURIComponent(q || '');
                const res = await fetch(url, { signal: aborter.signal, headers: { 'Accept': 'application/json' } });
                if (!res.ok) throw new Error('HTTP ' + res.status);
                const data = await res.json();
                render(data, q);
            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('Autocomplete error', err);
                    list.innerHTML = '<li class="ac-empty">Greška kod dohvaćanja.</li>';
                    open();
                }
            } finally {
                spinner.classList.remove('ac-spinner-active');
            }
        }

        const onInput = debounce(function (e) {
            const q = e.target.value.trim();
            // Clearing the text invalidates the selected ID
            if (!q) {
                hidden.value = '';
                close();
                return;
            }
            if (q.length < minChars) {
                close();
                return;
            }
            fetchResults(q);
        }, 220);

        input.addEventListener('input', onInput);

        input.addEventListener('focus', function () {
            if (lastResults.length) open();
            else if (input.value.trim().length >= minChars) fetchResults(input.value.trim());
            else fetchResults(''); // empty query — show top results
        });

        input.addEventListener('blur', function () {
            // Delay so click on result still fires
            setTimeout(() => close(), 150);
        });

        input.addEventListener('keydown', function (e) {
            const items = list.querySelectorAll('.ac-result');
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                activeIndex = Math.min(items.length - 1, activeIndex + 1);
                updateActive(items);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                activeIndex = Math.max(0, activeIndex - 1);
                updateActive(items);
            } else if (e.key === 'Enter') {
                if (activeIndex >= 0 && lastResults[activeIndex]) {
                    e.preventDefault();
                    pick(activeIndex);
                }
            } else if (e.key === 'Escape') {
                close();
            }
        });

        function updateActive(items) {
            items.forEach((el, idx) => el.classList.toggle('ac-active', idx === activeIndex));
            const el = items[activeIndex];
            if (el) el.scrollIntoView({ block: 'nearest' });
        }

        clearBtn.addEventListener('click', function () {
            hidden.value = '';
            input.value = '';
            close();
            input.focus();
            hidden.dispatchEvent(new Event('change', { bubbles: true }));
        });
    }

    // =================================================================
    // 2) DateTimePicker — custom popup calendar, locale-aware
    // =================================================================
    const HR = {
        months: ['Siječanj','Veljača','Ožujak','Travanj','Svibanj','Lipanj','Srpanj','Kolovoz','Rujan','Listopad','Studeni','Prosinac'],
        weekdays: ['P','U','S','Č','P','S','N']
    };
    const EN = {
        months: ['January','February','March','April','May','June','July','August','September','October','November','December'],
        weekdays: ['M','T','W','T','F','S','S']
    };

    function initDateTimePicker(root) {
        const locale     = getLocale();
        const L          = locale === 'hr' ? HR : EN;
        const fmt        = locale === 'hr' ? 'dd.mm.yyyy' : 'mm/dd/yyyy';
        const includeTime = root.dataset.includeTime === '1';
        const minStr     = root.dataset.min || '';
        const maxStr     = root.dataset.max || '';
        const minDate    = minStr ? new Date(minStr) : null;
        const maxDate    = maxStr ? new Date(maxStr) : null;

        const hidden  = root.querySelector('.dt-value');
        const display = root.querySelector('.dt-input');
        const icon    = root.querySelector('.dt-icon');
        const popup   = root.querySelector('.dt-popup');
        const monthSel = root.querySelector('.dt-month');
        const yearSel  = root.querySelector('.dt-year');
        const grid    = root.querySelector('.dt-grid');
        const weekdays = root.querySelector('.dt-weekdays');
        const prevBtn = root.querySelector('.dt-prev');
        const nextBtn = root.querySelector('.dt-next');
        const todayBtn = root.querySelector('.dt-today');
        const clearBtn = root.querySelector('.dt-clear');
        const applyBtn = root.querySelector('.dt-apply');
        const hourInp  = root.querySelector('.dt-hour');
        const minInp   = root.querySelector('.dt-minute');

        // Init state — current date
        let current = hidden.value ? new Date(hidden.value) : new Date();
        if (isNaN(current.getTime())) current = new Date();
        let viewYear = current.getFullYear();
        let viewMonth = current.getMonth();
        let selectedDate = hidden.value ? new Date(hidden.value) : null;

        // Weekday header
        L.weekdays.forEach(w => {
            const span = document.createElement('span');
            span.className = 'dt-weekday';
            span.textContent = w;
            weekdays.appendChild(span);
        });

        // Month/year selects
        L.months.forEach((name, idx) => {
            const opt = document.createElement('option');
            opt.value = idx;
            opt.textContent = name;
            monthSel.appendChild(opt);
        });
        const thisYear = new Date().getFullYear();
        for (let y = thisYear - 10; y <= thisYear + 2; y++) {
            const opt = document.createElement('option');
            opt.value = y;
            opt.textContent = y;
            yearSel.appendChild(opt);
        }
        monthSel.value = viewMonth;
        yearSel.value = viewYear;

        function pad(n) { return n.toString().padStart(2, '0'); }
        function formatDisplay(d) {
            if (!d) return '';
            const day = pad(d.getDate()), mon = pad(d.getMonth() + 1), yr = d.getFullYear();
            const datePart = locale === 'hr' ? `${day}.${mon}.${yr}` : `${mon}/${day}/${yr}`;
            return includeTime ? `${datePart} ${pad(d.getHours())}:${pad(d.getMinutes())}` : datePart;
        }
        function formatIso(d) {
            if (!d) return '';
            return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:00`;
        }
        function isSameDay(a, b) {
            return a && b && a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
        }
        function isOutOfRange(d) {
            const stripped = new Date(d.getFullYear(), d.getMonth(), d.getDate());
            if (minDate && stripped < new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate())) return true;
            if (maxDate && stripped > new Date(maxDate.getFullYear(), maxDate.getMonth(), maxDate.getDate())) return true;
            return false;
        }

        function renderGrid() {
            grid.innerHTML = '';
            const firstDay = new Date(viewYear, viewMonth, 1);
            // Monday-first ordering
            const offset = (firstDay.getDay() + 6) % 7;
            const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();
            const daysInPrev  = new Date(viewYear, viewMonth, 0).getDate();
            const today = new Date();

            // Leading days from prev month
            for (let i = offset - 1; i >= 0; i--) {
                const d = new Date(viewYear, viewMonth - 1, daysInPrev - i);
                grid.appendChild(makeCell(d, true, today));
            }
            // Current month
            for (let d = 1; d <= daysInMonth; d++) {
                const dt = new Date(viewYear, viewMonth, d);
                grid.appendChild(makeCell(dt, false, today));
            }
            // Trailing days to fill 6 weeks (42 cells)
            const cells = grid.children.length;
            for (let i = 1; i <= 42 - cells; i++) {
                const dt = new Date(viewYear, viewMonth + 1, i);
                grid.appendChild(makeCell(dt, true, today));
            }

            monthSel.value = viewMonth;
            yearSel.value = viewYear;
        }

        function makeCell(d, isOtherMonth, today) {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'dt-day';
            btn.textContent = d.getDate();
            if (isOtherMonth) btn.classList.add('dt-day-other');
            if (isSameDay(d, today)) btn.classList.add('dt-day-today');
            if (selectedDate && isSameDay(d, selectedDate)) btn.classList.add('dt-day-selected');
            if (isOutOfRange(d)) {
                btn.disabled = true;
                btn.classList.add('dt-day-disabled');
            } else {
                btn.addEventListener('click', () => {
                    selectedDate = new Date(d);
                    renderGrid();
                });
            }
            return btn;
        }

        function open() {
            popup.classList.add('dt-open');
            popup.setAttribute('aria-hidden', 'false');
            renderGrid();
        }
        function close() {
            popup.classList.remove('dt-open');
            popup.setAttribute('aria-hidden', 'true');
        }

        display.addEventListener('click', open);
        icon.addEventListener('click', () => popup.classList.contains('dt-open') ? close() : open());

        document.addEventListener('mousedown', function (e) {
            if (!root.contains(e.target)) close();
        });

        prevBtn.addEventListener('click', () => {
            if (--viewMonth < 0) { viewMonth = 11; viewYear--; }
            renderGrid();
        });
        nextBtn.addEventListener('click', () => {
            if (++viewMonth > 11) { viewMonth = 0; viewYear++; }
            renderGrid();
        });
        monthSel.addEventListener('change', () => { viewMonth = parseInt(monthSel.value, 10); renderGrid(); });
        yearSel.addEventListener('change',  () => { viewYear  = parseInt(yearSel.value,  10); renderGrid(); });

        todayBtn.addEventListener('click', () => {
            const t = new Date();
            selectedDate = t;
            viewYear = t.getFullYear();
            viewMonth = t.getMonth();
            renderGrid();
        });

        clearBtn.addEventListener('click', () => {
            selectedDate = null;
            hidden.value = '';
            display.value = '';
            close();
            hidden.dispatchEvent(new Event('change', { bubbles: true }));
        });

        applyBtn.addEventListener('click', () => {
            if (!selectedDate) { close(); return; }
            const final = new Date(selectedDate);
            if (includeTime) {
                final.setHours(parseInt(hourInp.value || '0', 10));
                final.setMinutes(parseInt(minInp.value || '0', 10));
            }
            hidden.value = formatIso(final);
            display.value = formatDisplay(final);
            close();
            hidden.dispatchEvent(new Event('change', { bubbles: true }));
            display.dispatchEvent(new Event('blur', { bubbles: true }));
        });
    }

    // =================================================================
    // 3) AJAX Search — type-as-you-go grid refresh
    //    Markup: <form data-ajax-search="endpoint" data-target="#results">
    // =================================================================
    function initAjaxSearch(form) {
        const endpoint = form.dataset.ajaxSearch;
        const targetSelector = form.dataset.target;
        const target = document.querySelector(targetSelector);
        if (!target) {
            console.warn('AJAX search target not found:', targetSelector);
            return;
        }

        const inputs = form.querySelectorAll('input[name], select[name]');
        let lastQS = '';
        let aborter = null;

        async function refresh() {
            const params = new URLSearchParams();
            inputs.forEach(inp => {
                if (inp.value !== '' && inp.value != null) {
                    params.append(inp.name, inp.value);
                }
            });
            const qs = params.toString();
            if (qs === lastQS) return;
            lastQS = qs;

            if (aborter) aborter.abort();
            aborter = new AbortController();

            target.classList.add('ajax-loading');
            try {
                const url = endpoint + (qs ? '?' + qs : '');
                const res = await fetch(url, { signal: aborter.signal, headers: { 'Accept': 'text/html' } });
                if (!res.ok) throw new Error('HTTP ' + res.status);
                const html = await res.text();
                target.innerHTML = html;

                // re-arm reveal/stagger animations on freshly inserted nodes
                target.querySelectorAll('.stagger, .reveal').forEach(el => el.classList.add('visible'));
            } catch (err) {
                if (err.name !== 'AbortError') {
                    console.error('AJAX search error', err);
                }
            } finally {
                target.classList.remove('ajax-loading');
            }
        }

        const refreshDebounced = debounce(refresh, 220);

        inputs.forEach(inp => {
            inp.addEventListener('input', refreshDebounced);
            inp.addEventListener('change', refreshDebounced);
        });

        // Prevent default form submit (since results update live)
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            refresh();
        });
    }

    // =================================================================
    // Wire up on DOM ready (and re-wire when AJAX swaps content)
    // =================================================================
    function wireAll(scope) {
        const root = scope || document;
        root.querySelectorAll('.ac-control:not(.ac-wired)').forEach(el => {
            el.classList.add('ac-wired');
            initAutocomplete(el);
        });
        root.querySelectorAll('.dt-control:not(.dt-wired)').forEach(el => {
            el.classList.add('dt-wired');
            initDateTimePicker(el);
        });
        root.querySelectorAll('form[data-ajax-search]:not(.ajax-wired)').forEach(el => {
            el.classList.add('ajax-wired');
            initAjaxSearch(el);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => wireAll(document));
    } else {
        wireAll(document);
    }

    // Expose for re-wiring after dynamic content injection
    window.CuggerControls = { wireAll };
})();
