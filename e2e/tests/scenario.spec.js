// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Playwright E2E scenarij u 10 koraka (rubrika: +3 boda).
 *
 * Pokriva kompletan korisnički tok: početna → globalna pretraga → prijava →
 * pregled piva → check-in → feed → recenzija → profil → AI unos → odjava.
 *
 * Koristi seed korisnika pivo_lover (lozinka Cugger123!) iz demo baze.
 */

const USERNAME = 'pivo_lover';
const PASSWORD = 'Cugger123!';

test('Scenarij u 10 koraka: od posjete do odjave', async ({ page }) => {
  const stamp = Date.now();
  const checkInComment = `Playwright check-in ${stamp}`;
  const reviewComment = `Playwright recenzija ${stamp} — odličan hop profil!`;

  // ── KORAK 1: Otvori početnu stranicu ──
  await test.step('1. Početna stranica se učitava', async () => {
    await page.goto('/');
    await expect(page).toHaveTitle(/Cugger/);
    await expect(page.locator('.navbar-brand')).toContainText('Cugger');
  });

  // ── KORAK 2: Globalna pretraga (Ctrl+K paleta) ──
  await test.step('2. Globalna pretraga pronalazi stranice i podatke', async () => {
    await page.click('#global-search-toggle');
    await expect(page.locator('#global-search-overlay')).toBeVisible();
    await page.fill('#global-search-input', 'IPA');
    const results = page.locator('#global-search-results .gs-item');
    await expect(results.first()).toBeVisible();
    // Grupa "Piva" mora sadržavati barem jedan IPA rezultat
    await expect(page.locator('.gs-group-title', { hasText: 'Piva' })).toBeVisible();
    await page.keyboard.press('Escape');
    await expect(page.locator('#global-search-overlay')).toBeHidden();
  });

  // ── KORAK 3: Prijava seed korisnika ──
  await test.step('3. Prijava kao pivo_lover', async () => {
    await page.goto('/login');
    await page.fill('#UsernameOrEmail', USERNAME);
    await page.fill('#Password', PASSWORD);
    await page.click('button.auth-submit');
    // Nakon prijave navbar prikazuje odjavu (logout gumb)
    await expect(page.locator('.navbar-logout-btn')).toBeVisible();
  });

  // ── KORAK 4: Navigacija na detalje piva kroz globalnu pretragu ──
  await test.step('4. Pretraga vodi na detalje piva Punk IPA', async () => {
    await page.click('#global-search-toggle');
    await page.fill('#global-search-input', 'Punk');
    const punkResult = page.locator('.gs-item', { hasText: 'Punk IPA' }).first();
    await expect(punkResult).toBeVisible();
    await punkResult.click();
    await expect(page).toHaveURL(/\/pivo\/\d+/);
    await expect(page.locator('h1')).toContainText('Punk IPA');
  });

  // ── KORAK 5: Kreiranje check-ina (CRUD: Create) ──
  await test.step('5. Novi check-in za Punk IPA', async () => {
    await page.goto('/CheckIn/Create');

    // Autocomplete odabir piva
    const beerAc = page.locator('.ac-control[data-field-name="BeerId"]');
    await beerAc.locator('.ac-input').fill('Punk');
    await beerAc.locator('.ac-results .ac-result').first().waitFor();
    await beerAc.locator('.ac-results .ac-result').first().dispatchEvent('mousedown');

    // Autocomplete odabir lokala
    const venueAc = page.locator('.ac-control[data-field-name="VenueId"]');
    await venueAc.locator('.ac-input').fill('Dublin');
    await venueAc.locator('.ac-results .ac-result').first().waitFor();
    await venueAc.locator('.ac-results .ac-result').first().dispatchEvent('mousedown');

    // Ocjena preko slidera (range input)
    await page.locator('input[name="Rating"]').evaluate((el) => {
      el.value = '4.5';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });

    await page.fill('textarea[name="Comment"]', checkInComment);
    await page.click('.auth-form button:has-text("Pošalji check-in")');

    // Redirect na detalje check-ina s toast porukom
    await expect(page).toHaveURL(/\/CheckIn\/Details\/\d+/);
    await expect(page.locator('body')).toContainText(checkInComment);
  });

  // ── KORAK 6: Check-in je vidljiv u feedu (CRUD: Read) ──
  await test.step('6. Check-in se pojavljuje u feedu', async () => {
    await page.goto('/feed');
    await expect(page.locator('body')).toContainText(checkInComment);
  });

  // ── KORAK 7: Kreiranje recenzije ──
  await test.step('7. Nova recenzija za pivo', async () => {
    await page.goto('/Review/Create');

    const beerAc = page.locator('.ac-control[data-field-name="BeerId"]');
    await beerAc.locator('.ac-input').fill('Punk');
    await beerAc.locator('.ac-results .ac-result').first().waitFor();
    await beerAc.locator('.ac-results .ac-result').first().dispatchEvent('mousedown');

    await page.locator('input[name="Rating"]').evaluate((el) => {
      el.value = '5';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
    });

    await page.fill('textarea[name="Comment"]', reviewComment);
    await page.click('.auth-form button:has-text("Objavi recenziju")');

    await expect(page.locator('body')).toContainText(reviewComment);
  });

  // ── KORAK 8: Profil korisnika prikazuje aktivnost ──
  await test.step('8. Profil korisnika pivo_lover', async () => {
    await page.goto(`/korisnik/${USERNAME}`);
    await expect(page.locator('body')).toContainText(USERNAME);
  });

  // ── KORAK 9: AI unos stranica je dostupna prijavljenom korisniku ──
  await test.step('9. AI unos stranica se otvara', async () => {
    await page.goto('/ai');
    await expect(page.locator('h1')).toContainText('AI unos');
    await expect(page.locator('#ai-prompt')).toBeVisible();
  });

  // ── KORAK 10: Odjava ──
  await test.step('10. Odjava vraća gosta na javni prikaz', async () => {
    await page.locator('nav .navbar-logout-btn').click();
    await expect(page.locator('nav a[href="/login"]')).toBeVisible();
  });
});

test('Responsive: hamburger meni radi na mobilnom viewportu', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto('/');

  const toggle = page.locator('#navbar-toggle');
  const menu = page.locator('#navbar-menu');

  await expect(toggle).toBeVisible();
  await expect(menu).toBeHidden();

  await toggle.click();
  await expect(menu).toBeVisible();
  await expect(menu.locator('a', { hasText: 'Piva' })).toBeVisible();

  await toggle.click();
  await expect(menu).toBeHidden();
});
