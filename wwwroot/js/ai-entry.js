/* ========== CUGGER — AI unos (Views/Ai/Index.cshtml) ==========
 * Tok: prompt → POST /ai/parse (prijedlog) → potvrda → POST /ai/create.
 */
(function () {
  'use strict';

  var FIELD_LABELS = {
    entityType: 'Vrsta',
    name: 'Naziv',
    style: 'Stil',
    abv: 'ABV (%)',
    ibu: 'IBU',
    description: 'Opis',
    breweryName: 'Pivovara',
    country: 'Država',
    city: 'Grad',
    address: 'Adresa',
    foundedYear: 'Godina osnutka',
    websiteUrl: 'Web',
    latitude: 'Geo širina',
    longitude: 'Geo dužina',
    beerName: 'Pivo',
    venueName: 'Lokal',
    rating: 'Ocjena',
    comment: 'Komentar'
  };

  var TYPE_LABELS = {
    beer: '🍺 Novo pivo',
    brewery: '🏭 Nova pivovara',
    venue: '📍 Novi lokal',
    checkin: '✅ Check-in',
    review: '⭐ Recenzija',
    unknown: '❓ Nepoznato'
  };

  var lastParsed = null;

  function $(id) { return document.getElementById(id); }

  function setStatus(text, isError) {
    var el = $('ai-status');
    el.hidden = !text;
    el.textContent = text || '';
    el.classList.toggle('error', !!isError);
  }

  function post(url, body) {
    return fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    }).then(function (r) {
      return r.json().then(function (data) {
        if (!r.ok) throw new Error(data.error || ('Greška (' + r.status + ')'));
        return data;
      });
    });
  }

  function showPreview(data) {
    lastParsed = data.parsed;
    var table = $('ai-preview-table');
    table.innerHTML = '';

    $('ai-preview-message').textContent = data.parsed.message || '';

    Object.keys(FIELD_LABELS).forEach(function (key) {
      var value = data.parsed[key];
      if (value === null || value === undefined || value === '') return;
      var row = table.insertRow();
      var th = document.createElement('th');
      th.textContent = FIELD_LABELS[key];
      var td = row.insertCell();
      td.textContent = key === 'entityType' ? (TYPE_LABELS[value] || value) : value;
      row.insertBefore(th, td);
    });

    var problemsEl = $('ai-preview-problems');
    problemsEl.innerHTML = '';
    (data.problems || []).forEach(function (p) {
      var div = document.createElement('div');
      div.className = 'ai-problem';
      div.textContent = '⚠️ ' + p;
      problemsEl.appendChild(div);
    });

    $('ai-confirm').disabled = !data.canCreate;
    $('ai-preview').hidden = false;
  }

  function hidePreview() {
    $('ai-preview').hidden = true;
    lastParsed = null;
  }

  function parse() {
    var prompt = $('ai-prompt').value.trim();
    if (!prompt) return;

    hidePreview();
    setStatus('🧠 AI razmišlja...');
    $('ai-submit').disabled = true;

    post('/ai/parse', { prompt: prompt })
      .then(function (data) {
        setStatus('');
        showPreview(data);
      })
      .catch(function (err) {
        setStatus(err.message, true);
      })
      .finally(function () {
        $('ai-submit').disabled = false;
      });
  }

  function confirmCreate() {
    if (!lastParsed) return;
    setStatus('💾 Spremam...');
    $('ai-confirm').disabled = true;

    post('/ai/create', lastParsed)
      .then(function (data) {
        setStatus('');
        hidePreview();
        var status = $('ai-status');
        status.hidden = false;
        status.classList.remove('error');
        status.innerHTML = '✅ ' + data.label + ' <a href="' + data.url + '">Pogledaj →</a>';
        $('ai-prompt').value = '';
      })
      .catch(function (err) {
        setStatus(err.message, true);
        $('ai-confirm').disabled = false;
      });
  }

  document.addEventListener('DOMContentLoaded', function () {
    var submit = $('ai-submit');
    if (!submit) return;

    submit.addEventListener('click', parse);
    $('ai-prompt').addEventListener('keydown', function (e) {
      if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) parse();
    });
    $('ai-confirm').addEventListener('click', confirmCreate);
    $('ai-cancel').addEventListener('click', hidePreview);

    document.querySelectorAll('.ai-example').forEach(function (btn) {
      btn.addEventListener('click', function () {
        $('ai-prompt').value = btn.textContent;
        $('ai-prompt').focus();
      });
    });
  });
})();
