using Cugger.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cugger.Tests.Infrastructure
{
    /// <summary>
    /// WebApplicationFactory za integracijske testove (lab-5):
    ///  - aplikacija se diže in-process s pravim pipelineom (routing, auth, EF...)
    ///  - baza je SQLite in-memory (svježa po factory instanci, sa seed podacima
    ///    iz migracija — isti put kao u produkciji)
    ///  - web root je privremeni direktorij (upload testovi ne diraju pravi wwwroot)
    ///  - autentikacija u testovima ide kroz TestAuth shemu (request headeri)
    /// </summary>
    public class CuggerApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;
        private readonly string _tempWebRoot;

        public CuggerApiFactory()
        {
            _tempWebRoot = Path.Combine(Path.GetTempPath(), "cugger-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempWebRoot);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseWebRoot(_tempWebRoot);

            builder.ConfigureServices(services =>
            {
                // Zamijeni registraciju DbContexta s in-memory SQLite vezom
                // (veza mora ostati otvorena da baza preživi između requestova)
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<CuggerDbContext>)
                             || d.ServiceType == typeof(CuggerDbContext))
                    .ToList();
                foreach (var d in descriptors)
                    services.Remove(d);

                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<CuggerDbContext>(options => options.UseSqlite(_connection));

                // Test auth shema postaje default (Identity cookie sheme ostaju registrirane)
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                }).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Dispose();
            try
            {
                if (Directory.Exists(_tempWebRoot))
                    Directory.Delete(_tempWebRoot, recursive: true);
            }
            catch
            {
                // best-effort čišćenje temp direktorija
            }
        }
    }

    public static class HttpClientAuthExtensions
    {
        /// <summary>Admin (seed korisnik pivo_lover, Id=1, role Admin+Member).</summary>
        public static HttpClient AsAdmin(this HttpClient client, int userId = 1)
        {
            client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
            client.DefaultRequestHeaders.Remove(TestAuthHandler.RolesHeader);
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "Admin,Member");
            return client;
        }

        /// <summary>Običan član (default seed korisnik hop_king, Id=2).</summary>
        public static HttpClient AsMember(this HttpClient client, int userId = 2)
        {
            client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
            client.DefaultRequestHeaders.Remove(TestAuthHandler.RolesHeader);
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "Member");
            return client;
        }
    }
}
