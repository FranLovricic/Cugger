---
name: edit-form
description: Use when generating a Create or Edit form for an entity in the Cugger ASP.NET Core MVC project. Produces both GET and POST actions and the matching Razor view with model binding.
---

# Edit/Create Form Skill — Cugger

Generiranje Create/Edit forme s model bindingom u ASP.NET Core MVC.

## Aktivacija

Kad korisnik traži:
- "napravi formu za dodavanje X"
- "Edit stranica za Y"
- "CRUD forma za Z"

## Što napraviti

1. **Akcije controllera (Create + Edit):**
   ```csharp
   [HttpGet]
   public IActionResult Create()
   {
       return View(new Xxx());
   }

   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Create(Xxx model)
   {
       if (!ModelState.IsValid)
           return View(model);

       _db.Xxxs.Add(model);
       await _db.SaveChangesAsync();
       return RedirectToAction(nameof(Index));
   }

   [HttpGet]
   public IActionResult Edit(int id)
   {
       var item = _xxxRepo.GetById(id);
       if (item == null) return NotFound();
       return View(item);
   }

   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Edit(int id, Xxx model)
   {
       if (id != model.Id) return BadRequest();
       if (!ModelState.IsValid) return View(model);

       _db.Xxxs.Update(model);
       await _db.SaveChangesAsync();
       return RedirectToAction(nameof(Details), new { id = model.Id });
   }
   ```

2. **View `Views/Xxx/Create.cshtml` i `Edit.cshtml`** (može biti partial za zajednički form):

   ```cshtml
   @model Xxx

   @{
       ViewData["Title"] = Model.Id == 0 ? "Novi XXX" : "Uredi XXX";
   }

   <div class="page-header">
       <div class="section-label">@(Model.Id == 0 ? "Novi unos" : "Uređivanje")</div>
       <h1 class="page-title">@ViewData["Title"]</h1>
   </div>

   <form asp-action="@(Model.Id == 0 ? "Create" : "Edit")" method="post" class="form-card">
       @Html.AntiForgeryToken()
       <input type="hidden" asp-for="Id" />

       <div class="form-row">
           <label asp-for="Name" class="form-label">Naziv</label>
           <input asp-for="Name" class="form-input" />
           <span asp-validation-for="Name" class="form-error"></span>
       </div>

       {/* Ostala polja prema modelu */}

       <div class="form-actions">
           <a href="javascript:history.back()" class="btn btn-secondary">Odustani</a>
           <button type="submit" class="btn btn-primary">Spremi</button>
       </div>
   </form>

   @section Scripts {
       <partial name="_ValidationScriptsPartial" />
   }
   ```

3. **CSS (ako još nema form-card):**
   ```css
   .form-card { max-width: 720px; padding: 2rem; background: var(--bg-card); border: 1px solid var(--border); border-radius: var(--radius); }
   .form-row { margin-bottom: 1.5rem; }
   .form-label { display: block; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.1em; }
   .form-input { width: 100%; padding: 0.75rem 1rem; background: var(--bg); border: 2px solid var(--border); border-radius: var(--radius-sm); color: var(--text); }
   .form-input:focus { border-color: var(--primary); outline: none; }
   .form-error { color: var(--danger); font-size: 0.85rem; margin-top: 0.25rem; display: block; }
   .form-actions { display: flex; gap: 1rem; justify-content: flex-end; }
   ```

4. **Validacija:**
   - Anotacije na modelu (`[Required]`, `[StringLength(N)]`, `[Range(...)]`) drive client + server validaciju.
   - `_ValidationScriptsPartial` mora biti uključen za client-side validaciju.

5. **Antiforgery token:**
   - Obavezan na svakom POST.

## Best practices

- Koristiti `asp-for` tag helpere umjesto raw inputa — automatski radi binding + validaciju.
- Ako Create i Edit dijele formu, izvuci u `Views/Xxx/_Form.cshtml` partial.
- Redirect nakon uspješnog POST-a (PRG pattern) — nikad NE renderiraj direktno listu nakon spremanja.
- ModelState.IsValid check uvijek prije DB save-a.

## Anti-pattern (ne raditi)

- ❌ Forma bez `@Html.AntiForgeryToken()`.
- ❌ Direktno `_db.SaveChanges()` bez ModelState provjere.
- ❌ `Edit` POST koji prima `id` ali ga ne validira protiv `model.Id`.
- ❌ Pokazivanje liste nakon POST-a (treba RedirectToAction).
- ❌ Ručno parsiranje `formData["..."]` umjesto model bindinga.
