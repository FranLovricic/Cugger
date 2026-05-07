using Cugger.Data;
using Cugger.Repositories;
using Cugger.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// ========== EF CORE - DbContext + Provider ==========
var dbProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";

builder.Services.AddDbContext<CuggerDbContext>(options =>
{
    if (string.Equals(dbProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var conn = builder.Configuration.GetConnectionString("SqlServer")
                   ?? throw new InvalidOperationException("Missing 'SqlServer' connection string.");
        options.UseSqlServer(conn);
    }
    else if (string.Equals(dbProvider, "SqlServerDocker", StringComparison.OrdinalIgnoreCase))
    {
        var conn = builder.Configuration.GetConnectionString("SqlServerDocker")
                   ?? throw new InvalidOperationException("Missing 'SqlServerDocker' connection string.");
        options.UseSqlServer(conn);
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

// ========== Auth ==========
builder.Services.AddSingleton<PasswordService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
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
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ========== Auto-migrate (or EnsureCreated fallback) ==========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CuggerDbContext>();
    var passwords = scope.ServiceProvider.GetRequiredService<PasswordService>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("[Cugger] Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Cugger] Migrate() failed ({ex.Message}). Falling back to EnsureCreated().");
        db.Database.EnsureCreated();
    }

    // Zamijeni sentinel vrijednosti seed korisnika s pravim PBKDF2 hash-em za "Cugger123!"
    var seedUsers = db.Users.Where(u => u.PasswordHash == "SEED_NEEDS_HASH").ToList();
    if (seedUsers.Count > 0)
    {
        const string defaultPassword = "Cugger123!";
        foreach (var u in seedUsers)
        {
            var (hash, salt) = passwords.HashPassword(defaultPassword);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
        }
        db.SaveChanges();
        Console.WriteLine($"[Cugger] Seed passwords initialized for {seedUsers.Count} users (default: '{defaultPassword}').");
    }
}

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
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

app.Run();
