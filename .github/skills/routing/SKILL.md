---
name: routing
description: Use when adding, modifying, or removing URL routes (custom MapControllerRoute, attribute routes, friendly URLs) in the Cugger ASP.NET Core MVC project.
---

# Routing Skill — Cugger

Pomoć pri dodavanju i konfiguraciji URL ruta u ASP.NET Core MVC.

## Kada koristiti

- friendly URL-ovi (`/pivo/3` umjesto `/Beer/Details/3`)
- attribute routing na pojedinim akcijama
- redoslijed ruta (custom prije default)
- route constraints (`{id:int}`, `{slug:minlength(3)}`, ...)
- routing parametri / query stringovi

## Pravila

1. Custom imenovane rute (`MapControllerRoute`) moraju biti **prije** default rute u `Program.cs`. Inače default ruta proguta zahtjev.
2. Attribute routing (`[Route(...)]`) na akciji **nadjačava** convention routing za tu akciju.
3. Naziv kontrolera u definiciji rute se piše BEZ sufiksa "Controller" (npr. `controller = "Beer"`, NE `"BeerController"`).
4. Akcija u `defaults` se piše točno onako kako je metoda nazvana (case-insensitive ali držati se PascalCase).
5. Rute s parametrima koriste constraint kad god je tip nesumnjiv — `{id:int}`, `{slug:alpha}`, `{lang:length(2)}`.
6. Redirect-only akcije (npr. `ByUsername` koji preusmjerava na `Details(id)`) treba vratiti `RedirectToAction` umjesto `View`.

## Postojeće Cugger rute (referentno)

```csharp
// Custom (PRIJE defaulta)
app.MapControllerRoute("beer-details-friendly", "pivo/{id:int}",
    new { controller = "Beer", action = "Details" });

app.MapControllerRoute("brewery-details-friendly", "pivovara/{id:int}",
    new { controller = "Brewery", action = "Details" });

app.MapControllerRoute("user-by-username", "korisnik/{username}",
    new { controller = "User", action = "ByUsername" });

app.MapControllerRoute("feed-shortcut", "feed",
    new { controller = "CheckIn", action = "Index" });

app.MapControllerRoute("beer-search", "pretraga",
    new { controller = "Beer", action = "Search" });

// Default
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Attribute routes (na akcijama)
[Route("pretraga")]                   // Beer.Search
[Route("Beer/Style/{style}")]         // Beer.Style
[Route("Brewery/Country/{country}")]  // Brewery.ByCountry
[Route("Venue/City/{city}")]          // Venue.ByCity
[Route("Review/Top")]                 // Review.Top
```

## Generiranje URL-ova u Razor view-u

Preporučeno (asp tag helpers):
```cshtml
<a asp-controller="Beer" asp-action="Details" asp-route-id="@beer.Id">@beer.Name</a>
```

Ili Url.Action:
```cshtml
<a href="@Url.Action("Details", "Beer", new { id = beer.Id })">@beer.Name</a>
```

Ili friendly URL direktno:
```cshtml
<a href="/pivo/@beer.Id">@beer.Name</a>
```

## Sitemap dokumentacija

Pri svakoj promjeni rute, **uvijek ažurirati** [`lab-3/sitemap.md`](../../../lab-3/sitemap.md) — to je živi sitemap projekta.

## Kontrolni popis

- [ ] Nova ruta deklarirana ili u `Program.cs` (imenovana) ili kao `[Route]` na akciji
- [ ] Custom rute deklarirane PRIJE default rute
- [ ] Akcijska metoda postoji i ima ispravne parametre
- [ ] Constraint-i odgovaraju očekivanom tipu
- [ ] `lab-3/sitemap.md` ažuriran
- [ ] Test u browseru: očekivani URL otvara očekivani view
