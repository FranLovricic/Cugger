using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>Lab-5: integracijski testovi za /api/users (CRUD + pretraga + autorizacija + DTO bez internih polja).</summary>
    public class UsersApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public UsersApiTests(CuggerApiFactory factory) => _factory = factory;

        private static UserCreateDto ValidUser(string username, string email) => new()
        {
            Username = username,
            Email = email,
            Password = "TestLozinka123",
            FirstName = "Test",
            LastName = "Korisnik"
        };

        [Fact]
        public async Task GetAll_ReturnsSeededUsers()
        {
            var client = _factory.CreateClient();
            var users = await client.GetFromJsonAsync<List<UserDto>>("/api/users");

            Assert.NotNull(users);
            Assert.True(users!.Count >= 4);
            Assert.Contains(users, u => u.Username == "pivo_lover");
        }

        [Fact]
        public async Task GetAll_DoesNotExposeInternalFields()
        {
            var client = _factory.CreateClient();
            var raw = await client.GetStringAsync("/api/users");

            // DTO ne smije izlagati interna polja entiteta
            Assert.DoesNotContain("passwordHash", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("securityStamp", raw, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("email", raw, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAll_WithSearchQuery_FiltersResults()
        {
            var client = _factory.CreateClient();
            var users = await client.GetFromJsonAsync<List<UserDto>>("/api/users?q=hop_king");

            Assert.NotNull(users);
            Assert.Single(users!);
            Assert.Equal("Marko", users![0].FirstName);
        }

        [Fact]
        public async Task GetById_ReturnsUser_WithActivityCounts()
        {
            var client = _factory.CreateClient();
            var user = await client.GetFromJsonAsync<UserDto>("/api/users/1");

            Assert.NotNull(user);
            Assert.Equal("pivo_lover", user!.Username);
            Assert.True(user.CheckInCount > 0);
            Assert.True(user.FriendCount > 0);
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/users/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAdmin_Returns201_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PostAsJsonAsync("/api/users", ValidUser("novi_korisnik", "novi@example.com"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.Equal("novi_korisnik", created!.Username);

            var fetched = await client.GetFromJsonAsync<UserDto>($"/api/users/{created.Id}");
            Assert.Equal("novi_korisnik", fetched!.Username);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/users", ValidUser("anon_user", "anon@example.com"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsJsonAsync("/api/users", ValidUser("member_user", "member@example.com"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidEmail_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidUser("los_email", "nije-email");

            var response = await client.PostAsJsonAsync("/api/users", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithTooShortPassword_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidUser("kratka_lozinka", "kratka@example.com");
            invalid.Password = "kratko";

            var response = await client.PostAsJsonAsync("/api/users", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithDuplicateUsername_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var duplicate = ValidUser("pivo_lover", "duplikat@example.com"); // seed username

            var response = await client.PostAsJsonAsync("/api/users", duplicate);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_OwnProfile_AsMember_Returns204_AndPersists()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var update = new UserUpdateDto { FirstName = "Marko", LastName = "Horvat", Bio = "Novi bio iz testa" };

            var response = await client.PutAsJsonAsync("/api/users/2", update);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetched = await client.GetFromJsonAsync<UserDto>("/api/users/2");
            Assert.Equal("Novi bio iz testa", fetched!.Bio);
        }

        [Fact]
        public async Task Update_SomeoneElsesProfile_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var update = new UserUpdateDto { FirstName = "Hak", LastName = "Er" };

            var response = await client.PutAsJsonAsync("/api/users/3", update);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Update_SomeoneElsesProfile_AsAdmin_Returns204()
        {
            var client = _factory.CreateClient().AsAdmin();
            var update = new UserUpdateDto { FirstName = "Ana", LastName = "Novak", Bio = "Admin je uredio bio" };

            var response = await client.PutAsJsonAsync("/api/users/3", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var update = new UserUpdateDto { FirstName = "Nitko", LastName = "Nigdje" };

            var response = await client.PutAsJsonAsync("/api/users/99999", update);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithMissingFirstName_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = new UserUpdateDto { FirstName = "", LastName = "Prezime" };

            var response = await client.PutAsJsonAsync("/api/users/1", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Delete_FreshUser_AsAdmin_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/users", ValidUser("za_brisanje", "brisanje@example.com"));
            var created = await createResponse.Content.ReadFromJsonAsync<UserDto>();

            var response = await client.DeleteAsync($"/api/users/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/users/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_UserWithActivity_Returns409Conflict()
        {
            var client = _factory.CreateClient().AsAdmin();
            // seed korisnik 1 ima check-inove, recenzije i prijateljstva
            var response = await client.DeleteAsync("/api/users/1");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/users/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.DeleteAsync("/api/users/3");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
