---
name: list-page
description: Use when generating a new "Index/list" page for an entity in the Cugger ASP.NET Core MVC project (table or card grid showing all records).
---

# List Page Skill — Cugger

Generiranje "Index" / lista stranice za entitet.

## Aktivacija

Kad korisnik traži:
- "napravi listu X-eva"
- "Index stranica za novi entitet"
- "prikaz svih kvizova / piva / lokacija ..."

## Što napraviti

1. **Controller akcija:**
   ```csharp
   public IActionResult Index()
   {
       var items = _xxxRepo.GetAll();
       ViewBag.Breadcrumbs = new[]
       {
           new BreadcrumbItem("Dashboard", "/", false),
           new BreadcrumbItem("XXX", "/Xxx", true)
       };
       return View(items);
   }
   ```

2. **View `Views/Xxx/Index.cshtml`:**
   ```cshtml
   @model List<Xxx>

   @{
       ViewData["Title"] = "XXX - Cugger";
   }

   <div class="page-header">
       <div class="section-label">{Eyebrow}</div>
       <h1 class="page-title">XXX</h1>
       <p class="page-subtitle">{Opis sekcije.}</p>
   </div>

   @if (Model?.Any() == true)
   {
       <div class="grid grid-3 stagger">
           @foreach (var item in Model)
           {
               <a href="/Xxx/Details/@item.Id" style="text-decoration: none; color: inherit;">
                   <div class="card">
                       <div class="card-header"><h3 class="card-title">@item.Name</h3></div>
                       <div class="card-body">
                           {/* Sažeti pregled kartice */}
                       </div>
                       <div class="card-footer">
                           <span class="link-arrow" style="font-size: 0.8rem;">Pogledaj <span>→</span></span>
                       </div>
                   </div>
               </a>
           }
       </div>
   }
   else
   {
       <div class="section-empty"><div class="section-empty-icon">📭</div><p>Nema XXX-eva.</p></div>
   }
   ```

3. **Repository:**
   - Provjeri da postoji `XxxRepository.GetAll()` koji vraća `List<Xxx>`.
   - Ako kartica prikazuje neke property-je iz povezanih entiteta, doradi `Include()` u `GetAll()`.

4. **Routing:**
   - Default ruta `/Xxx` već radi za standardni Index.
   - Razmotri friendly URL ako je entitet često linkan.

5. **Navbar:**
   - Dodaj link u `Views/Shared/_Layout.cshtml` ako želiš da entitet bude dostupan iz glavne navigacije.

## Best practices

- Grid `.grid-3` ili `.grid-4` za kartice; `.grid-2` za bogatije kartice (npr. checkin feed).
- Empty state je obavezan.
- Breadcrumbs uvijek.
- Linkovi koriste friendly URL gdje postoji.
- Stagger animacija na grid containeru.

## Test

Otvori `/Xxx` u browseru i provjeri:
- prikazuje sve zapise iz baze
- linkovi vode na Details stranicu
- mobilna verzija (1 kolona) izgleda OK
