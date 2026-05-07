---
name: entity-framework
description: Use when adding, modifying, or removing EF entities, generating migrations, configuring DbContext relations, or seeding data in the Cugger project.
---

# Entity Framework Skill — Cugger

Pomoć kod EF Core 10 zadataka u Cugger ASP.NET Core MVC projektu.

## Kada koristiti

Aktiviraj kad zadatak uključuje:

- dodavanje novog entiteta (klase) ili svojstva (property)
- modificiranje postojećeg modela (anotacije, relacije)
- konfiguraciju veza (1-N, N-N) u `OnModelCreating`
- generiranje migracije (`dotnet ef migrations add ...`)
- ažuriranje baze (`dotnet ef database update`)
- seed podataka (`HasData`)
- izvedbu LINQ upita s `Include` / `ThenInclude`

## Konvencije u Cugger projektu

- Svi modeli su u `Models/` namespaceu `Cugger.Models`.
- DbContext se zove `CuggerDbContext` i nalazi u `Data/`, namespace `Cugger.Data`.
- Repository klase su u `Repositories/`, namespace `Cugger.Repositories`, registrirane kao `Scoped` u `Program.cs`.
- DB provider se odabire kroz `appsettings.json` ključ `Database:Provider` (`Sqlite` / `SqlServer` / `SqlServerDocker`). Default je `Sqlite`.
- Migracije se drže u `Migrations/` (default folder za EF), ne u zasebnom DAL projektu.

## Pravila pri dodavanju entiteta

1. Klasa mora biti `public` i imati svojstvo `Id` označeno s `[Key]`.
2. Stringovi imaju `[Required]` (kad je nullable=false) i `[StringLength(N)]`.
3. Strani ključevi: posebno svojstvo `XxxId` s `[ForeignKey(nameof(Xxx))]`, plus navigacijsko svojstvo `public virtual Xxx? Xxx { get; set; }`.
4. Kolekcije navigation: `public virtual ICollection<T> Items { get; set; } = new List<T>();`
5. Calc properties (npr. AverageRating): `[NotMapped]`.
6. Datumi: `DateTime` (ne `DateTimeOffset`, osim ako je striktno potrebno).
7. Decimalna polja: `[Column(TypeName = "decimal(P,S)")]`.

## Pravila pri konfiguraciji veza u `OnModelCreating`

- N-1 veze koje stvaraju cycle u SQL Serveru (npr. CheckIn → User, CheckIn → Beer, CheckIn → Venue) postaviti na `OnDelete(DeleteBehavior.Restrict)`.
- Brewery → Beer može ostati `OnDelete(DeleteBehavior.Cascade)` (jednostavna 1-N).
- Unique indexi: `modelBuilder.Entity<X>().HasIndex(x => x.Field).IsUnique();`

## Pravila pri seed podacima

- Koristiti `mb.Entity<T>().HasData(...)` u `OnModelCreating`.
- Svi Id-jevi moraju biti **fiksni** (nikad `0` ili automatski).
- Seed ne smije referencirati navigacijska svojstva — samo FK-id polja.
- Datumi moraju biti deterministički (`new DateTime(2024, 3, 15)`), nikad `DateTime.Now`.

## Migracijske komande

Iz root foldera projekta (gdje je `Cugger.csproj`):

```powershell
# Dodaj novu migraciju
dotnet ef migrations add NazivMigracije

# Primijeni na bazu
dotnet ef database update

# Generiraj SQL skriptu (za production ručno pokretanje)
dotnet ef migrations script

# Vrati zadnju migraciju (development)
dotnet ef migrations remove
```

Ako EF tools nisu instalirani: `dotnet tool install --global dotnet-ef`.

## Repository pattern

Pri dodavanju nove tablice, dodaj i repository:

1. Kreiraj `Repositories/XxxRepository.cs` s konstrukturskom DI `CuggerDbContext`.
2. Standardne metode: `GetAll()`, `GetById(int)`, plus specifične filtere.
3. Eager-load relacije s `Include` / `ThenInclude` na metode koje vraćaju Details (jedan zapis).
4. Index i list metode trebaju biti efikasne — `Include` samo ono što UI prikazuje.
5. Registriraj repository u `Program.cs`:
   ```csharp
   builder.Services.AddScoped<XxxRepository>();
   ```

## Kontrolni popis pri promjeni modela

- [ ] Dodane / izmijenjene anotacije
- [ ] Ažuriran DbContext (DbSet, relacije, seed)
- [ ] Ažuriran odgovarajući Repository
- [ ] Generirana migracija
- [ ] Migracija primijenjena na lokalnu bazu
- [ ] Ažuriran `lab-3/semantic-model.md` ako se model promijenio
- [ ] `dotnet build` prolazi bez warninga
