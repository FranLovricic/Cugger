# Lab 3 — AI Agent Log

> Granularni log korištenja AI agenta tijekom Lab 3.

## Korišteni alati

- **Claude Code** (Opus 4.7, 1M context) za glavne refaktore i generiranje koda.
- **Custom skills** definirani u `.github/skills/` — agent ih učitava automatski kad zadatak odgovara opisu skill-a.

---

## Kronološki tijek (Lab 3)

### 1) Inicijalna analiza projekta
Agent: glavni Claude
Zadatak: pročitati postojeće Modele, Controllere, Views, Program.cs i CuggerDataService kako bi razumio postojeću strukturu prije EF refaktora.

### 2) Dodavanje EF Core paketa
Skill: `entity-framework`
Zadatak: Dodati `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` u `Cugger.csproj`.

### 3) Anotacije modela
Skill: `entity-framework`
Zadatak: Dodati `[Key]`, `[Required]`, `[StringLength]`, `[ForeignKey]`, `[Range]`, `[Column]`, `[NotMapped]` na sve modele (Beer, Brewery, User, CheckIn, Review, Venue, Friendship). Pretvoriti `List<T>` navigacijska svojstva u `ICollection<T>`.

### 4) Kreiranje DbContexta
Skill: `entity-framework`
Zadatak: Generirati `Cugger.Data.CuggerDbContext` s 7 `DbSet`-ova, `OnModelCreating` konfiguracijom relacija (Restrict cascade za izbjegavanje SQL Server cycle problema), unique indexima za `User.Username` i `User.Email`, i seed podacima za sve entitete.

### 5) Repozitoriji
Skill: glavni agent (bez specifičnog skill-a)
Zadatak: Kreirati 7 EF-baziranih repozitorija u `Repositories/` folderu sa `Include`/`ThenInclude` za eager-loading relacija.

### 6) Custom routing
Skill: `routing`
Zadatak: Dodati 5 custom imenovanih ruta u `Program.cs` (`/pivo/{id}`, `/pivovara/{id}`, `/korisnik/{username}`, `/feed`, `/pretraga`) + 4 attribute rute na pojedinim akcijama (`Beer.Search`, `Beer.Style`, `Brewery.ByCountry`, `Venue.ByCity`, `Review.Top`).

### 7) UI/UX redizajn (sub-agent)
Skill: `ux-ui` (UX sub-agent)
Zadatak:
- Pretvoriti reference s `User #1`, `Beer #2` na stvarne imene (zahvaljujući EF eager-loadu).
- Dodati search bar u hero sekciju i Beer/Index, te novu `Search` stranicu.
- Dodati style filter chips (filter po `BeerStyle` enumu).
- Brutalist `stat-blocks` na Beer/Details (ABV, IBU, Rating bar diagrams).
- Friendlier URL-ovi u hyperlinkovima (`/pivo/`, `/pivovara/`, `/korisnik/`).

Log poziva sub-agenta vidi u: [`agent-log/ux-ui-calls.md`](agent-log/ux-ui-calls.md)

### 8) Dokumentacija
Skill: glavni agent
Zadatak: Generirati `semantic-model.md` (DB shema), `sitemap.md` (routing pregled), i ažurirati `README.md` s uputama za migracije i odabir DB providera.

---

## Validacija

- Build projekta: ✅ (`dotnet build`)
- Inicijalna migracija generirana: ✅ (`dotnet ef migrations add InitialCreate`)
- Database update: ✅ (auto-migrate na `Program.Main` startup)

## Što agent NIJE radio sam

- Konkretne dizajn odluke (npr. izbor SQLite kao default providera kako student ne bi morao instalirati MSSQL) **donesene su sa studentom**.
- Brutalist stil i font (Inter weight 800/900) zadržani su kao u Lab 2 — agent ih nije mijenjao.
- Migracijska skripta — `dotnet ef migrations add` se mora pokrenuti ručno (vidi README); agent ne pokreće dotnet CLI komande u korisničkom okruženju.
