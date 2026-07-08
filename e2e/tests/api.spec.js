// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Playwright API testovi za SVE API endpointe (rubrika: testovi za sve API endpointe).
 *
 * Dublja pokrivenost (validacije, role, edge-caseovi) živi u xUnit integracijskim
 * testovima (Cugger.Tests). Ovdje Playwright verificira javni ugovor API-ja
 * na živoj aplikaciji: GET čitanja vraćaju podatke, a pisanja bez prijave 401.
 */

// ====== GET (javno čitanje) — svih 8 REST kontrolera ======

const publicListEndpoints = [
  '/api/beers',
  '/api/breweries',
  '/api/venues',
  '/api/users',
  '/api/checkins',
  '/api/reviews',
  '/api/friendships',
  '/api/beers/1/photos',
];

for (const url of publicListEndpoints) {
  test(`GET ${url} vraća 200 i JSON listu`, async ({ request }) => {
    const response = await request.get(url);
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
  });
}

const publicDetailEndpoints = [
  ['/api/beers/1', 'name'],
  ['/api/breweries/1', 'name'],
  ['/api/venues/1', 'name'],
  ['/api/users/1', 'username'],
  ['/api/checkins/1', 'rating'],
  ['/api/reviews/1', 'rating'],
  ['/api/friendships/1', 'id'],
];

for (const [url, field] of publicDetailEndpoints) {
  test(`GET ${url} vraća zapis s poljem "${field}"`, async ({ request }) => {
    const response = await request.get(url);
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toHaveProperty(field);
  });
}

test('GET nepostojeći ID vraća 404 (ProblemDetails)', async ({ request }) => {
  const response = await request.get('/api/beers/99999');
  expect(response.status()).toBe(404);
});

// ====== Pretraga i filtri ======

test('GET /api/beers?q=ipa filtrira po nazivu', async ({ request }) => {
  const response = await request.get('/api/beers?q=ipa');
  const beers = await response.json();
  expect(beers.length).toBeGreaterThan(0);
  for (const b of beers) {
    expect((b.name + (b.brewery?.name ?? '') + b.description).toLowerCase()).toContain('ipa');
  }
});

test('GET /api/search/global?q=ipa vraća grupirane rezultate', async ({ request }) => {
  const response = await request.get('/api/search/global?q=ipa');
  expect(response.status()).toBe(200);
  const body = await response.json();
  expect(body.groups.length).toBeGreaterThan(0);
  const beerGroup = body.groups.find((g) => g.name === 'Piva');
  expect(beerGroup).toBeTruthy();
});

test('GET /api/lookup/beers?q=ipa vraća autocomplete rezultate', async ({ request }) => {
  const response = await request.get('/api/lookup/beers?q=ipa');
  expect(response.status()).toBe(200);
  const items = await response.json();
  expect(items.length).toBeGreaterThan(0);
  expect(items[0]).toHaveProperty('label');
});

// ====== Pisanje bez prijave → 401 (svi POST/PUT/DELETE endpointi) ======

const writeEndpoints = [
  ['post', '/api/beers', { name: 'X', style: 'Lager', abv: 5, breweryId: 1 }],
  ['put', '/api/beers/1', { name: 'X', style: 'Lager', abv: 5, breweryId: 1 }],
  ['delete', '/api/beers/1', null],
  ['post', '/api/breweries', { name: 'X', country: 'HR', city: 'ZG' }],
  ['put', '/api/breweries/1', { name: 'X', country: 'HR', city: 'ZG' }],
  ['delete', '/api/breweries/1', null],
  ['post', '/api/venues', { name: 'X', address: 'Y', city: 'ZG', country: 'HR' }],
  ['put', '/api/venues/1', { name: 'X', address: 'Y', city: 'ZG', country: 'HR' }],
  ['delete', '/api/venues/1', null],
  ['post', '/api/users', { username: 'x', email: 'x@x.hr', password: 'password1', firstName: 'X', lastName: 'Y' }],
  ['put', '/api/users/1', { username: 'x', email: 'x@x.hr', firstName: 'X', lastName: 'Y' }],
  ['delete', '/api/users/1', null],
  ['post', '/api/checkins', { beerId: 1, venueId: 1, rating: 4 }],
  ['put', '/api/checkins/1', { beerId: 1, venueId: 1, rating: 4 }],
  ['delete', '/api/checkins/1', null],
  ['post', '/api/reviews', { beerId: 1, rating: 4, comment: 'x' }],
  ['put', '/api/reviews/1', { beerId: 1, rating: 4, comment: 'x' }],
  ['delete', '/api/reviews/1', null],
  ['post', '/api/friendships', { fromUserId: 1, toUserId: 2 }],
  ['delete', '/api/friendships/1', null],
  ['delete', '/api/photos/1', null],
];

for (const [method, url, data] of writeEndpoints) {
  test(`${method.toUpperCase()} ${url} bez prijave vraća 401`, async ({ request }) => {
    const response = await request[method](url, data ? { data } : undefined);
    expect(response.status()).toBe(401);
  });
}

// ====== MCP endpoint ======

test('MCP endpoint /mcp odgovara na initialize handshake', async ({ request }) => {
  const response = await request.post('/mcp', {
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json, text/event-stream',
    },
    data: {
      jsonrpc: '2.0',
      id: 1,
      method: 'initialize',
      params: {
        protocolVersion: '2025-06-18',
        capabilities: {},
        clientInfo: { name: 'playwright', version: '1.0' },
      },
    },
  });
  expect(response.status()).toBe(200);
  const text = await response.text();
  expect(text).toContain('"serverInfo"');
  expect(text).toContain('Cugger');
});
