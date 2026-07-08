# 🍺 Cugger

Cugger je clone aplikacije **Untappd** za rate-anje piva, check-iniranje na lokalima, dijeljenje recenzija i druženje sa zajednicom ljubitelja piva.

Projekt iz kolegija ASP.NET MVC.

---

## Stack

- **.NET 10** + ASP.NET Core MVC
- **Entity Framework Core 10** (SQLite default; opcionalno SQL Server / Docker)
- **Custom CSS** (brutalist dark theme inspiriran frog.co)
- Inter font, čisti vanilla JS
- **Serilog** (file + console logging)
- **Anthropic Claude API** (AI unos podataka)
- **ModelContextProtocol** (MCP server za agentic IDE-e)
- **Playwright** + **xUnit** (E2E i integracijski testovi)

---

## Brzo pokretanje

### 1. Klonirati repo

```powershell
git clone https://github.com/<TVOJ-USER>/Cugger.git
cd Cugger
```

### 2. Pokreni aplikaciju

```powershell
dotnet run
```

Aplikacija sama provede `Database.Migrate()` na startup-u i napuni bazu seed podacima.

Otvori: https://localhost:5001 (ili gledaj output `dotnet run` za točan port).

> Default DB provider je **SQLite** — datoteka `cugger.db` se kreira automatski u root folderu projekta. **Ništa ne moraš instalirati.**

---

## Database setup — opcije

Projekt podržava 3 providera, biraš ih kroz `appsettings.json` ključ `Database:Provider`.

```json
{
  "Database": { "Provider": "Sqlite" },
  "ConnectionStrings": {
    "Sqlite": "Data Source=cugger.db",
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=CuggerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "SqlServerDocker": "Server=127.0.0.1,1433;Database=CuggerDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

### A) SQLite (zadano — radi out-of-the-box)

```json
"Database": { "Provider": "Sqlite" }
```

Pokreni `dotnet run` i to je to. SQLite datoteka `cugger.db` se kreira u root folderu.
Kad želiš resetirati bazu — obriši `cugger.db` i pokreni opet `dotnet run`.

### B) SQL Server LocalDB (instaliran s Visual Studio-m)

```json
"Database": { "Provider": "SqlServer" }
```

Provjeri da imaš LocalDB:
```powershell
sqllocaldb info
```

Ako vidiš `MSSQLLocalDB`, sve je spremno. Pokreni:
```powershell
dotnet run
```

Aplikacija će automatski kreirati bazu `CuggerDb` u LocalDB instanci i primijeniti migraciju.

### C) SQL Server u Dockeru (svi OS-ovi)

```json
"Database": { "Provider": "SqlServerDocker" }
```

Pokreni Docker container:

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" `
  -p 1433:1433 --name cugger-mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

Za zaustaviti / pokrenuti container:
```powershell
docker stop cugger-mssql
docker start cugger-mssql
```

Onda:
```powershell
dotnet run
```

---

## EF Core migracije

Projekt koristi EF Migrations za održavanje sheme baze.

### Pokreni postojeće migracije (ručno — ako ne želiš auto-migrate)

Ako iz nekog razloga isključiš auto-migrate u `Program.cs`, ručno primijeni:

```powershell
dotnet ef database update
```

### Generiraj novu migraciju kad promijeniš model

```powershell
dotnet ef migrations add NazivIzmjene
dotnet ef database update
```

### Vrati zadnju migraciju (development only!)

```powershell
dotnet ef migrations remove
```

### Generiraj SQL skriptu (za production)

```powershell
dotnet ef migrations script -o migration.sql
```

Ako `dotnet-ef` alat nije instaliran:
```powershell
dotnet tool install --global dotnet-ef
```

### "Hardcore reset" — kreni iz nule

```powershell
# SQLite — samo obriši fajl
Remove-Item cugger.db

# SQL Server — kroz EF
dotnet ef database drop --force
dotnet ef database update
```

---

## Routing — friendly URL-ovi

Cugger koristi i klasične `{controller}/{action}/{id}` rute, i nekoliko **lijepih (friendly) URL-ova**:

| URL                      | Vodi na                        |
|--------------------------|--------------------------------|
| `/pivo/3`                | Detalji piva s ID 3            |
| `/pivovara/2`            | Detalji pivovare               |
| `/korisnik/pivo_lover`   | Profil korisnika po username-u |
| `/feed`                  | Feed nedavnih check-inova      |
| `/pretraga?q=ipa`        | Pretraga piva                  |
| `/Beer/Style/IPA`        | Sva IPA piva                   |
| `/Brewery/Country/Hrvatska` | Hrvatske pivovare           |
| `/Venue/City/Zagreb`     | Lokali u Zagrebu               |
| `/Review/Top`            | Top liked recenzije            |

Pun pregled svih URL-ova: [`lab-3/sitemap.md`](lab-3/sitemap.md).

---

## Struktura projekta

```
Cugger/
├── Controllers/         # MVC controlleri (Beer, Brewery, User, Ai, ...)
│   └── Api/             # REST API controlleri (lab-5)
├── Cugger.Tests/        # xUnit integracijski testovi (svi API endpointi)
├── Data/                # CuggerDbContext + EF konfiguracija
├── e2e/                 # Playwright testovi (API + 10-koračni E2E scenarij)
├── Migrations/          # EF migracijske skripte
├── Models/              # Domain modeli (anotirani za EF)
├── Repositories/        # EF-bazirani repozitoriji (DI scoped)
├── Services/            # AiEntryService (Claude), CuggerMcpTools (MCP), ...
├── Views/               # Razor view-ovi
├── wwwroot/             # CSS, JS, assets
├── logs/                # Serilog rolling file logovi (git-ignored)
├── lab-1/               # Lab 1 dokumenti i AI agent log
├── lab-2/               # Lab 2 dokumenti
├── lab-3/               # Lab 3 dokumenti (semantic-model, sitemap, agent-log)
├── .github/skills/      # Custom Copilot skills (entity-framework, routing, ux-ui, ...)
├── .mcp.json            # MCP registracija za agentic IDE-e
├── appsettings.json     # Konfiguracija (DB, Serilog, Anthropic)
└── Program.cs           # App startup + DI + routing + MCP endpoint
```

---

## Lab 3 — što je dodano

Lab 3 zadaci ([Lab3.pdf](lab-3/Lab3.pdf)):

- ✅ EF Core konfiguriran (`Microsoft.EntityFrameworkCore.SqlServer` + `.Sqlite`)
- ✅ Anotacije na sve modele (`[Key]`, `[ForeignKey]`, `[Required]`, `[StringLength]`, `[Range]`, `[NotMapped]`, `[Column]`)
- ✅ `virtual ICollection<>` na navigation kolekcijama
- ✅ `CuggerDbContext` s `DbSet<>` za 7 entiteta i seed-om
- ✅ Auto-migrate na startup-u (`Database.Migrate()`)
- ✅ Mock repository → EF repository (refaktor `CuggerDataService` u 7 EF-baziranih repozitorija)
- ✅ Inicijalna migracija generirana (`Migrations/20260507071729_InitialCreate.cs`)
- ✅ 5 custom imenovanih ruta + 5 attribute ruta (više od 4 zahtijevana)
- ✅ [`lab-3/semantic-model.md`](lab-3/semantic-model.md) — semantički DB model
- ✅ [`lab-3/sitemap.md`](lab-3/sitemap.md) — sitemap svih URL-ova
- ✅ Skills: [`entity-framework`](.github/skills/entity-framework/SKILL.md), [`routing`](.github/skills/routing/SKILL.md), [`ux-ui`](.github/skills/ux-ui/SKILL.md), [`list-page`](.github/skills/list-page/SKILL.md), [`edit-form`](.github/skills/edit-form/SKILL.md)

---

## Završni projekt — što je dodano

### 📝 Logging (Serilog)

Svi HTTP zahtjevi i domenski događaji (prijave, CRUD operacije, AI upiti, MCP pozivi)
logiraju se u konzolu i **rolling file** `logs/cugger-YYYYMMDD.log` (čuva se 14 dana).
Konfiguracija je u `appsettings.json` pod ključem `Serilog`.

### 🔍 Globalna pretraga

Klik na 🔍 u navbaru, **Ctrl+K** ili **/** otvara command-palette pretragu koja istovremeno
pretražuje **stranice/izbornike** i **podatke** (piva, pivovare, lokale, korisnike,
check-inove, recenzije). Navigacija strelicama + Enter.
Backend: `GET /api/search/global?q=...` (grupirani JSON).

### 📱 Responsive (mobile UI)

Navbar se na širinama ≤900px pretvara u hamburger meni (animirani toggle, zatvaranje na
odabir stavke). Layout ima media query breakpointove na 480/768/900/1024px.
Pokriveno Playwright testom na 375px viewportu.

### 🤖 AI unos podataka (Claude API)

Na `/ai` (prijavljeni korisnici) upit na prirodnom jeziku — npr.
*"Popio sam Guinness u Pivnici Medvedgrad, ocjena 4.5"* — Claude pretvara u strukturirani
prijedlog (pivo / pivovara / lokal / check-in / recenzija) koji korisnik potvrđuje prije
spremanja. Poštuju se role (piva/pivovare/lokali samo admin).

Za rad postavi API ključ (jedno od):
```powershell
# opcija A: environment varijabla
$env:ANTHROPIC_API_KEY = "sk-ant-..."

# opcija B: appsettings.json
# "Anthropic": { "ApiKey": "sk-ant-...", "Model": "claude-opus-4-8" }
```

Implementacija: `Services/AiEntryService.cs` (Anthropic C# SDK + structured outputs),
`Controllers/AiController.cs`, `Views/Ai/Index.cshtml`.

### 🔌 MCP server (agentic IDE pristup)

Aplikacija izlaže **MCP (Model Context Protocol)** endpoint na `POST /mcp`
(Streamable HTTP). Alati: `search_beers`, `get_beer`, `search_breweries`, `search_venues`,
`get_feed`, `get_top_beers`, `get_stats`.

Projekt sadrži [`.mcp.json`](.mcp.json), pa Claude Code / VS Code / Cursor automatski vide
server — dovoljno je pokrenuti aplikaciju (`dotnet run`) i otvoriti projekt u IDE-u.
Ručna registracija u Claude Code:
```powershell
claude mcp add --transport http cugger http://localhost:5017/mcp
```

### ✅ Testovi

**xUnit integracijski testovi** (143 testa — svi API endpointi: CRUD, validacije, role,
pretraga, filtri, paging, lookup i globalna pretraga):
```powershell
dotnet test
```

**Playwright** (43 testa — API smoke testovi svih endpointa, MCP handshake, responsive
test i **E2E scenarij u 10 koraka**: početna → globalna pretraga → prijava → detalji piva
→ check-in → feed → recenzija → profil → AI stranica → odjava):
```powershell
cd e2e
npm install
npx playwright install chromium
npx playwright test          # sam pokreće aplikaciju ako ne radi
npx playwright show-report   # HTML izvještaj
```

---

## Razvoj

### Build
```powershell
dotnet build
```

### Run (HTTPS)
```powershell
dotnet run
```

### Run (watch mode — auto-reload na promjenu koda)
```powershell
dotnet watch run
```

---

## Poznata pitanja

**P: `dotnet ef` ne radi.**
A: `dotnet tool install --global dotnet-ef`.

**P: SQL Server ne želi spojiti se.**
A: Provjeri provider u `appsettings.json`. Ili default-aj na `Sqlite` (radi bez instalacije).

**P: Baza je "stuck" — treba mi clean reset.**
A: Obriši `cugger.db` (ako koristiš SQLite) ili `dotnet ef database drop --force`, pa `dotnet run`.

**P: Migracija failuje s cycle/cascade greškom.**
A: Sve N-1 veze koje rade cycle (CheckIn → User/Beer/Venue, Review → User/Beer, Friendship → User/User) već su konfigurirane s `OnDelete(DeleteBehavior.Restrict)` u `OnModelCreating`. Ako dodaješ novu vezu i dobiješ ovu grešku — postavi `Restrict` ili `NoAction`.
