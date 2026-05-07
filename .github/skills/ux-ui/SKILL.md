---
name: ux-ui
description: Use when generating, updating, or restyling Razor views, HTML markup, or CSS in the Cugger ASP.NET Core MVC project. Always invoke this skill when producing UI code so the result respects the brutalist dark theme.
---

# UX/UI Sub-Agent — Cugger

Cugger je clone Untappd-a — **brutalistic dark theme** sa amber akcentom inspiriran je frog.co stilom.
Kad god generiraš UI kod (Razor view, partial, HTML, CSS), drži se ovih pravila.

## Visual identity

- **Boja pozadine:** `#0a0a0a` (var --bg)
- **Card pozadina:** `#1a1a1a` (var --bg-card), border `rgba(255,255,255,0.08)`
- **Akcent:** amber `#F59E0B` (var --primary), tamniji `#D97706`, svijetliji `#FCD34D`
- **Tekst:** primarni `#f0f0f0`, sekundarni `#888`, muted `#555`
- **Font:** `Inter`, weight 400/500/600/700/800/900
- **Radius:** `1rem` (kartice), `0.5rem` (manje)
- **Ease:** `cubic-bezier(0.16, 1, 0.3, 1)` za sve tranzicije

## Layout

- Container max-width `~1200px`, padding 0 1.5rem.
- Grid sistem: `.grid-2`, `.grid-3`, `.grid-4`.
- Sve glavne sekcije zovu se `.section`.
- Header / hero / page-header imaju **veliki spacing** (6rem 0).
- `.divider` između cjelina (1px line, full width).

## Konvencije

1. **Eyebrow + h2 + subtitle** ritam. Svaka sekcija počinje:
   ```cshtml
   <div class="section-label">Eyebrow</div>
   <h2 class="section-title">Glavni naslov</h2>
   <p class="section-subtitle">Kratki opis sekcije.</p>
   ```
2. **Linkovi imaju "→"** — koristi `link-arrow` klasu i `<span>→</span>` element.
3. **Reveal animacije** — sve glavne sekcije imaju klasu `.reveal` i `.stagger` na grid containerima.
4. **Kartice koje prikazuju entitet** trebaju biti hoverable — `transform: translateY(-3px)` na hover.
5. **Brojke (stats)** koriste `data-counter="N"` i animiraju se kroz `site.js`.
6. **Marquee** za stilove piva — neprekidno scrollanje kao traka.

## Pravila pri generiranju view-a

- Koristiti **friendly URL-ove** kad postoje: `/pivo/{id}`, `/pivovara/{id}`, `/korisnik/{username}`, `/feed`, `/pretraga`. NE `/Beer/Details/{id}` u UI linkovima (to je interno).
- Eager-loaded podatke koristiti direktno: `@checkIn.User?.FirstName`, `@checkIn.Beer?.Name`, `@checkIn.Venue?.Name`. NIKADA "User #1", "Beer #2".
- ViewBag treba biti tipiziran kroz `as`: `var beers = ViewBag.Beers as List<Beer>;`.
- Empty state: koristi `.section-empty` klasu sa ikonom i porukom.

## Komponente koje već postoje (NE duplicirati)

- `.btn`, `.btn-primary`, `.btn-secondary`
- `.card`, `.card-header`, `.card-body`, `.card-footer`, `.card-title`
- `.beer-card` (s rotation hover effect)
- `.checkin-card` (specifičan za feed)
- `.avatar` `.avatar-sm` `.avatar-md` `.avatar-lg` `.avatar-xl`
- `.tag` `.tag-style`
- `.search-bar`, `.search-input`
- `.filter-chip`, `.filter-chip.active`
- `.stat-blocks`, `.stat-block`, `.stat-bar`, `.stat-bar-fill`
- `.marquee-wrap`, `.marquee`, `.marquee-item`
- `.quote-block`, `.quote-text`, `.quote-attr`, `.accent`
- `.feature-block`, `.feature-text`, `.feature-visual`
- `.breadcrumb`, `.breadcrumb-item`
- `.list`, `.list-item`, `.list-item-content`, `.list-item-title`, `.list-item-subtitle`, `.list-item-meta`
- `.hero`, `.hero-eyebrow`, `.hero-title`, `.hero-description`, `.hero-cta`, `.hero-search`
- `.divider`

## Komponente koje generiraš (kad su potrebne nove)

Kad treba nešto novo, **prvo provjeri** postoji li slično u CSS-u, pa proširi ako ne. Drži se postojećih custom property var-ova.

## Anti-pattern (ne raditi)

- ❌ Bootstrap default classes (osim ako je već u layoutu — nije).
- ❌ Inline width/height u px za responsive komponente.
- ❌ Hardcoded boje umjesto var-ova.
- ❌ "User #1", "Beer #3" tip placeholderi — uvijek dohvati ime kroz EF eager-load.
- ❌ Stranica bez breadcrumbsa (osim Home Indexa).
- ❌ Linkovi bez `→` ili druge vizualne afirmacije.

## Kontrolni popis prije završetka

- [ ] Sve linkove koristim friendly URL kad postoji.
- [ ] Eager-loaded entiteti se koriste umjesto ID-jeva.
- [ ] Sekcije imaju eyebrow + title + subtitle.
- [ ] Empty state je obrađen.
- [ ] Reveal/stagger klase dodane gdje ima smisla.
- [ ] Mobilna verzija nije pokvarena (grid se kolapsa u 1 kolonu).
- [ ] Breadcrumbs ažurirani.
