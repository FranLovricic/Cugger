using Cugger.Data;
using Cugger.Models;
using Cugger.Repositories;
using Cugger.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// ========== LOGGING: Serilog (konzola + rolling file u logs/) ==========
// Početni logger — hvata greške i prije nego što je host konfiguriran.
// Namjerno NE koristimo CreateBootstrapLogger(): njegov Freeze() dopušta samo
// jedan host po procesu, a integracijski testovi (WebApplicationFactory) grade više njih.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Puna konfiguracija se čita iz appsettings.json ("Serilog" sekcija)
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// MVC + API
builder.Services.AddControllersWithViews();

// ========== EF CORE - DbContext + Provider ==========
var dbProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";

builder.Services.AddDbContext<CuggerDbContext>(options =>
{
    if (string.Equals(dbProvider, "MySql", StringComparison.OrdinalIgnoreCase))
    {
        var conn = builder.Configuration.GetConnectionString("MySql")
                ?? throw new InvalidOperationException("Missing 'MySql' connection string.");

        options.UseMySql(
            conn,
            ServerVersion.AutoDetect(conn));
    }
    else
    {
        var conn = builder.Configuration.GetConnectionString("Sqlite")
                ?? "Data Source=cugger.db";

        options.UseSqlite(conn);
    }
});

// ========== Repositories (DI) ==========
builder.Services.AddScoped<BeerRepository>();
builder.Services.AddScoped<BreweryRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<VenueRepository>();
builder.Services.AddScoped<CheckInRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<FriendshipRepository>();

// ========== AI integracija: unos podataka prirodnim jezikom (Claude API) ==========
builder.Services.AddScoped<AiEntryService>();

// ========== MCP server (Model Context Protocol) — pristup kroz agentic IDE ==========
// HTTP endpoint /mcp s alatima nad Cugger podacima (search_beers, get_feed, ...)
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<CuggerMcpTools>();

// ========== Auth: ASP.NET Core Identity (lab-5) ==========
builder.Services
    .AddIdentity<AppUser, IdentityRole<int>>(options =>
    {
        // Pravila usklađena s validacijom na Register formi (min 8 znakova)
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<CuggerDbContext>()
    .AddDefaultTokenProviders();

// Dodatni claimovi (ime, prezime, avatar) u auth cookie
builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CuggerClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "cugger.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // API klijenti ne žele redirect na login stranicu nego ispravan statusni kod
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});

// ========== 3rd party autentikacija: Google (lab-5) ==========
// Registrira se samo ako su client id/secret konfigurirani
// (appsettings.json / user-secrets / env varijable).
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        // default CallbackPath = /signin-google
    });
}

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ========== Auto-migrate (or EnsureCreated fallback) + Identity seed ==========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CuggerDbContext>();
    try
    {
        db.Database.Migrate();
        Log.Information("[Cugger] Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database migration failed");
        throw;
    }

    // Osiguraj da seed/demo korisnici imaju valjani Identity hash za "Cugger123!".
    // Pokriva i sentinel ("SEED_NEEDS_HASH") i stari PBKDF2 format iz lab-3
    // (baza migrirana s custom autha na Identity).
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    var seedUsernames = new[] { "pivo_lover", "hop_king", "stout_fan", "craft_explorer" };
    var seedUsers = db.Users.Where(u => seedUsernames.Contains(u.UserName!)).ToList();
    const string defaultPassword = "Cugger123!";
    var rehashed = 0;
    foreach (var u in seedUsers)
    {
        bool needsRehash;
        try
        {
            needsRehash = string.IsNullOrEmpty(u.PasswordHash)
                || u.PasswordHash == "SEED_NEEDS_HASH"
                || hasher.VerifyHashedPassword(u, u.PasswordHash, defaultPassword) == PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            needsRehash = true; // stari hash nije valjani Identity format
        }

        if (needsRehash)
        {
            u.PasswordHash = hasher.HashPassword(u, defaultPassword);
            rehashed++;
        }
    }
    if (rehashed > 0)
    {
        db.SaveChanges();
        Log.Information("[Cugger] Seed passwords initialized for {Count} users (default: '{DefaultPassword}').", rehashed, defaultPassword);
    }
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Serilog request logging — jedan strukturirani zapis po HTTP zahtjevu (metoda, path, status, trajanje)
app.UseSerilogRequestLogging();

// HTTPS redirect — osim za /mcp (MCP klijenti se spajaju na čisti HTTP bez praćenja redirecta)
app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/mcp"),
    branch => branch.UseHttpsRedirection());
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ========== ROUTING ==========
// Custom rute moraju biti PRIJE default rute

app.MapControllerRoute(
    name: "beer-details-friendly",
    pattern: "pivo/{id:int}",
    defaults: new { controller = "Beer", action = "Details" });

app.MapControllerRoute(
    name: "brewery-details-friendly",
    pattern: "pivovara/{id:int}",
    defaults: new { controller = "Brewery", action = "Details" });

app.MapControllerRoute(
    name: "user-by-username",
    pattern: "korisnik/{username}",
    defaults: new { controller = "User", action = "ByUsername" });

app.MapControllerRoute(
    name: "feed-shortcut",
    pattern: "feed",
    defaults: new { controller = "CheckIn", action = "Index" });

app.MapControllerRoute(
    name: "beer-search",
    pattern: "pretraga",
    defaults: new { controller = "Beer", action = "Search" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Attribute-routed API controlleri (lab-5)
app.MapControllers();

// MCP endpoint — agentic IDE-i se spajaju na /mcp (Streamable HTTP transport)
app.MapMcp("/mcp");

try
{
    Log.Information("[Cugger] Starting web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "[Cugger] Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Marker za WebApplicationFactory<Program> u integracijskim testovima (lab-5)
public partial class Program { }
