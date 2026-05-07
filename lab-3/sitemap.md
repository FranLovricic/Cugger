# Cugger — Sitemap & Routing

> Semantički model usmjeravanja: za svaki dostupni URL, koji controller, koja akcija i koji view se koristi.
> Generirano za Lab 3.

## Routing arhitektura

Routing je kombinacija:
- **Klasične default rute** (`{controller}/{action}/{id?}`) iz `Program.cs`
- **Custom imenovanih ruta** (`MapControllerRoute`) koje su PRIJE default rute u `Program.cs`
- **Attribute routinga** (`[Route(...)]`) na pojedinim akcijama

Konfiguracijska datoteka: [`Program.cs`](../Program.cs)

---

## Tablica svih URL-ova

| URL                                | Controller         | Akcija       | View                         | Tip rute                     |
|------------------------------------|--------------------|--------------|------------------------------|------------------------------|
| `/`                                | `HomeController`   | `Index`      | `Views/Home/Index.cshtml`    | default                      |
| `/Home/Privacy`                    | `HomeController`   | `Privacy`    | `Views/Home/Privacy.cshtml`  | default                      |
| `/Home/Error`                      | `HomeController`   | `Error`      | `Views/Shared/Error.cshtml`  | default                      |
| `/Beer`                            | `BeerController`   | `Index`      | `Views/Beer/Index.cshtml`    | default                      |
| `/Beer/Details/{id}`               | `BeerController`   | `Details`    | `Views/Beer/Details.cshtml`  | default                      |
| `/pivo/{id}`                       | `BeerController`   | `Details`    | `Views/Beer/Details.cshtml`  | **custom (`MapControllerRoute`)** |
| `/Beer/Style/{style}`              | `BeerController`   | `Style`      | `Views/Beer/Index.cshtml`    | **attribute `[Route]`**      |
| `/pretraga?q=...`                  | `BeerController`   | `Search`     | `Views/Beer/Search.cshtml`   | **attribute `[Route]` + custom MapControllerRoute** |
| `/Brewery`                         | `BreweryController`| `Index`      | `Views/Brewery/Index.cshtml` | default                      |
| `/Brewery/Details/{id}`            | `BreweryController`| `Details`    | `Views/Brewery/Details.cshtml` | default                    |
| `/pivovara/{id}`                   | `BreweryController`| `Details`    | `Views/Brewery/Details.cshtml` | **custom (`MapControllerRoute`)** |
| `/Brewery/Country/{country}`       | `BreweryController`| `ByCountry`  | `Views/Brewery/Index.cshtml` | **attribute `[Route]`**      |
| `/User`                            | `UserController`   | `Index`      | `Views/User/Index.cshtml`    | default                      |
| `/User/Details/{id}`               | `UserController`   | `Details`    | `Views/User/Details.cshtml`  | default                      |
| `/korisnik/{username}`             | `UserController`   | `ByUsername` | redirect → `Details`         | **custom (`MapControllerRoute`)** |
| `/CheckIn`                         | `CheckInController`| `Index`      | `Views/CheckIn/Index.cshtml` | default                      |
| `/CheckIn/Details/{id}`            | `CheckInController`| `Details`    | `Views/CheckIn/Details.cshtml`| default                     |
| `/feed`                            | `CheckInController`| `Index`      | `Views/CheckIn/Index.cshtml` | **custom (`MapControllerRoute`)** |
| `/Review`                          | `ReviewController` | `Index`      | `Views/Review/Index.cshtml`  | default                      |
| `/Review/Details/{id}`             | `ReviewController` | `Details`    | `Views/Review/Details.cshtml`| default                      |
| `/Review/Top`                      | `ReviewController` | `Top`        | `Views/Review/Index.cshtml`  | **attribute `[Route]`**      |
| `/Venue`                           | `VenueController`  | `Index`      | `Views/Venue/Index.cshtml`   | default                      |
| `/Venue/Details/{id}`              | `VenueController`  | `Details`    | `Views/Venue/Details.cshtml` | default                      |
| `/Venue/City/{city}`               | `VenueController`  | `ByCity`     | `Views/Venue/Index.cshtml`   | **attribute `[Route]`**      |
| `/Friendship`                      | `FriendshipController` | `Index`  | `Views/Friendship/Index.cshtml` | default                   |
| `/Friendship/Details/{id}`         | `FriendshipController` | `Details`| `Views/Friendship/Details.cshtml` | default                 |

---

## Custom routing — detalji

Pri rješavanju zahtjeva, rute se obrađuju **redom** kako su deklarirane.
Custom rute moraju biti registrirane PRIJE default rute, jer bi inače default ruta "uhvatila" zahtjev.

### 1) `/pivo/{id:int}` — friendly URL za pivo

```csharp
app.MapControllerRoute(
    name: "beer-details-friendly",
    pattern: "pivo/{id:int}",
    defaults: new { controller = "Beer", action = "Details" });
```
Pretvara: `https://app/pivo/3` → `BeerController.Details(3)`.

### 2) `/pivovara/{id:int}` — friendly URL za pivovaru

```csharp
app.MapControllerRoute(
    name: "brewery-details-friendly",
    pattern: "pivovara/{id:int}",
    defaults: new { controller = "Brewery", action = "Details" });
```

### 3) `/korisnik/{username}` — pretty URL po username-u

```csharp
app.MapControllerRoute(
    name: "user-by-username",
    pattern: "korisnik/{username}",
    defaults: new { controller = "User", action = "ByUsername" });
```
Akcija `ByUsername` napravi 302 redirect na `Details(id)`.

### 4) `/feed` — kratica za check-in feed

```csharp
app.MapControllerRoute(
    name: "feed-shortcut",
    pattern: "feed",
    defaults: new { controller = "CheckIn", action = "Index" });
```

### 5) `/pretraga` — kratica za pretragu

```csharp
app.MapControllerRoute(
    name: "beer-search",
    pattern: "pretraga",
    defaults: new { controller = "Beer", action = "Search" });
```

### 6) Attribute routes (pojedine akcije)

```csharp
// BeerController
[Route("pretraga")]            public IActionResult Search(string? q) { ... }
[Route("Beer/Style/{style}")]  public IActionResult Style(string style) { ... }

// BreweryController
[Route("Brewery/Country/{country}")]  public IActionResult ByCountry(string country) { ... }

// VenueController
[Route("Venue/City/{city}")]   public IActionResult ByCity(string city) { ... }

// ReviewController
[Route("Review/Top")]          public IActionResult Top() { ... }
```

---

## Default route

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Ako URL ne odgovara nijednoj prethodnoj definiciji, primjenjuje se default ruta.
Fallback vrijednosti: controller = `Home`, action = `Index`, id = nepostavljen.

---

## Tijek obrade jednog zahtjeva (primjer `/pivo/3`)

1. Browser šalje `GET /pivo/3`.
2. ASP.NET Core middleware lanac dolazi do `app.UseRouting()`, koji match-a URL na set pravila.
3. Pravilo `beer-details-friendly` zadovoljava (pattern: `pivo/{id:int}`), tako da se odabere `BeerController.Details(int id)`.
4. Iz DI containera se kreira instanca `BeerController` (sa svim repozitorijima injectanim kroz konstruktor).
5. Akcija `Details(3)` poziva `_beerRepo.GetById(3)`, koji preko EF Core eager-loada `Brewery`, `CheckIns`, `Reviews`.
6. ViewBag se napuni dodatnim podacima (recenzije, prosječna ocjena, breadcrumbs).
7. `return View(beer)` rendera `Views/Beer/Details.cshtml` sa modelom tipa `Beer`.
8. View koristi `@Model.Brewery.Name`, `@ViewBag.CheckIns`, ... za generiranje HTML-a.
9. HTTP odgovor sadrži renderirani HTML.
