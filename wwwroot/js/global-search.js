/* ========== CUGGER — Global Search (command palette) ==========
 * Pretraga izbornika/stranica i podataka kroz /api/search/global.
 * Otvaranje: klik na 🔍 u navbaru, Ctrl+K ili "/". Zatvaranje: Esc / klik izvan.
 * Navigacija: strelice gore/dolje + Enter.
 */
(function () {
  'use strict';

  var overlay, input, resultsEl, items = [], activeIndex = -1, debounceTimer = null, lastQuery = null;

  function init() {
    overlay = document.getElementById('global-search-overlay');
    input = document.getElementById('global-search-input');
    resultsEl = document.getElementById('global-search-results');
    if (!overlay || !input || !resultsEl) return;

    var toggle = document.getElementById('global-search-toggle');
    if (toggle) {
      toggle.addEventListener('click', function (e) {
        e.preventDefault();
        open();
      });
    }

    document.addEventListener('keydown', function (e) {
      var isOpen = overlay.classList.contains('open');
      // Ctrl+K / Cmd+K uvijek otvara; "/" samo izvan input polja
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        isOpen ? close() : open();
        return;
      }
      if (!isOpen && e.key === '/' && !isTyping(e.target)) {
        e.preventDefault();
        open();
        return;
      }
      if (!isOpen) return;

      if (e.key === 'Escape') { close(); }
      else if (e.key === 'ArrowDown') { e.preventDefault(); move(1); }
      else if (e.key === 'ArrowUp') { e.preventDefault(); move(-1); }
      else if (e.key === 'Enter') {
        e.preventDefault();
        var target = items[activeIndex] || items[0];
        if (target) window.location.href = target.getAttribute('data-url');
      }
    });

    overlay.addEventListener('mousedown', function (e) {
      if (e.target === overlay) close();
    });

    input.addEventListener('input', function () {
      clearTimeout(debounceTimer);
      debounceTimer = setTimeout(function () { search(input.value); }, 180);
    });
  }

  function isTyping(el) {
    if (!el) return false;
    var tag = (el.tagName || '').toLowerCase();
    return tag === 'input' || tag === 'textarea' || tag === 'select' || el.isContentEditable;
  }

  function open() {
    overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
    input.value = '';
    input.focus();
    search('');
  }

  function close() {
    overlay.classList.remove('open');
    document.body.style.overflow = '';
    activeIndex = -1;
  }

  function move(dir) {
    if (!items.length) return;
    activeIndex = (activeIndex + dir + items.length) % items.length;
    items.forEach(function (el, i) {
      el.classList.toggle('active', i === activeIndex);
    });
    items[activeIndex].scrollIntoView({ block: 'nearest' });
  }

  function search(q) {
    lastQuery = q;
    fetch('/api/search/global?q=' + encodeURIComponent(q))
      .then(function (r) { return r.json(); })
      .then(function (data) {
        if (lastQuery !== q) return; // stigao je stariji odgovor — ignoriraj
        render(data.groups || []);
      })
      .catch(function () {
        resultsEl.innerHTML = '<div class="gs-empty">Greška pri pretrazi. Pokušaj ponovno.</div>';
      });
  }

  function render(groups) {
    items = [];
    activeIndex = -1;
    resultsEl.innerHTML = '';

    if (!groups.length) {
      resultsEl.innerHTML = '<div class="gs-empty">Nema rezultata 🍂</div>';
      return;
    }

    groups.forEach(function (group) {
      var groupEl = document.createElement('div');
      groupEl.className = 'gs-group';

      var title = document.createElement('div');
      title.className = 'gs-group-title';
      title.textContent = group.name;
      groupEl.appendChild(title);

      group.items.forEach(function (item) {
        var a = document.createElement('a');
        a.className = 'gs-item';
        a.href = item.url;
        a.setAttribute('data-url', item.url);

        var icon = document.createElement('span');
        icon.className = 'gs-item-icon';
        icon.textContent = item.icon || '·';

        var body = document.createElement('span');
        body.className = 'gs-item-body';

        var label = document.createElement('span');
        label.className = 'gs-item-label';
        label.textContent = item.label;
        body.appendChild(label);

        if (item.subLabel) {
          var sub = document.createElement('span');
          sub.className = 'gs-item-sub';
          sub.textContent = item.subLabel;
          body.appendChild(sub);
        }

        a.appendChild(icon);
        a.appendChild(body);

        a.addEventListener('mouseenter', function () {
          activeIndex = items.indexOf(a);
          items.forEach(function (el) { el.classList.remove('active'); });
          a.classList.add('active');
        });

        groupEl.appendChild(a);
        items.push(a);
      });

      resultsEl.appendChild(groupEl);
    });
  }

  document.addEventListener('DOMContentLoaded', init);
})();
