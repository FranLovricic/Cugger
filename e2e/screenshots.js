// Pomoćna skripta: snimi screenshotove novih UI elemenata (nije test)
const { chromium } = require('@playwright/test');

(async () => {
  const outDir = process.argv[2] || '.';
  const browser = await chromium.launch();

  // 1. Global search paleta (desktop)
  const desktop = await browser.newPage({ viewport: { width: 1280, height: 800 } });
  await desktop.goto('http://localhost:5017/');
  await desktop.click('#global-search-toggle');
  await desktop.fill('#global-search-input', 'ipa');
  await desktop.waitForSelector('.gs-item');
  await desktop.screenshot({ path: `${outDir}/search-palette.png` });

  // 2. AI stranica (prijavljen korisnik)
  await desktop.keyboard.press('Escape');
  await desktop.goto('http://localhost:5017/login');
  await desktop.fill('#UsernameOrEmail', 'pivo_lover');
  await desktop.fill('#Password', 'Cugger123!');
  await desktop.click('button.auth-submit');
  await desktop.waitForSelector('.navbar-logout-btn');
  await desktop.goto('http://localhost:5017/ai');
  await desktop.screenshot({ path: `${outDir}/ai-page.png`, fullPage: false });

  // 3. Mobilni prikaz s otvorenim hamburger menijem
  const mobile = await browser.newPage({ viewport: { width: 375, height: 812 } });
  await mobile.goto('http://localhost:5017/');
  await mobile.click('#navbar-toggle');
  await mobile.waitForSelector('#navbar-menu.open');
  await mobile.screenshot({ path: `${outDir}/mobile-menu.png` });

  await browser.close();
  console.log('screenshots saved');
})();
