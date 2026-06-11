using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>
    /// Lab-5: integracijski testovi za /api/friendships.
    /// Poslovna pravila ne dopuštaju izmjenu prijateljstva pa PUT ne postoji
    /// (testira se da vraća 405 Method Not Allowed).
    /// </summary>
    public class FriendshipsApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public FriendshipsApiTests(CuggerApiFactory factory) => _factory = factory;

        [Fact]
        public async Task GetAll_ReturnsSeededFriendships_WithNestedUsers()
        {
            var client = _factory.CreateClient();
            var friendships = await client.GetFromJsonAsync<List<FriendshipDto>>("/api/friendships");

            Assert.NotNull(friendships);
            Assert.True(friendships!.Count >= 6);
            Assert.All(friendships, f =>
            {
                Assert.NotNull(f.FromUser);
                Assert.NotNull(f.ToUser);
            });
        }

        [Fact]
        public async Task GetAll_FilterByUser_ReturnsOnlyTheirFriendships()
        {
            var client = _factory.CreateClient();
            var friendships = await client.GetFromJsonAsync<List<FriendshipDto>>("/api/friendships?userId=4");

            Assert.NotNull(friendships);
            Assert.NotEmpty(friendships!);
            Assert.All(friendships!, f => Assert.True(f.FromUser!.Id == 4 || f.ToUser!.Id == 4));
        }

        [Fact]
        public async Task GetById_ReturnsFriendship()
        {
            var client = _factory.CreateClient();
            var friendship = await client.GetFromJsonAsync<FriendshipDto>("/api/friendships/1");

            Assert.NotNull(friendship);
            Assert.Equal("pivo_lover", friendship!.FromUser?.Username);
            Assert.Equal("hop_king", friendship.ToUser?.Username);
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/friendships/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns201_AndAssignsCurrentUser()
        {
            var client = _factory.CreateClient().AsMember(userId: 3);
            // korisnik 3 (stout_fan) → korisnik 4 (craft_explorer): ne postoji u seedu
            var response = await client.PostAsJsonAsync("/api/friendships", new FriendshipInputDto { ToUserId = 4 });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<FriendshipDto>();
            Assert.Equal(3, created!.FromUser?.Id);
            Assert.Equal(4, created.ToUser?.Id);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/friendships", new FriendshipInputDto { ToUserId = 4 });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithSelf_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var response = await client.PostAsJsonAsync("/api/friendships", new FriendshipInputDto { ToUserId = 2 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithUnknownTargetUser_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var response = await client.PostAsJsonAsync("/api/friendships", new FriendshipInputDto { ToUserId = 99999 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_Duplicate_Returns409Conflict()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            // seed prijateljstvo 2: korisnik 2 → korisnik 1 već postoji
            var response = await client.PostAsJsonAsync("/api/friendships", new FriendshipInputDto { ToUserId = 1 });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Create_ForAnotherUser_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var input = new FriendshipInputDto { FromUserId = 3, ToUserId = 4 };

            var response = await client.PostAsJsonAsync("/api/friendships", input);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Put_IsNotSupported_Returns405()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PutAsJsonAsync("/api/friendships/1", new FriendshipInputDto { ToUserId = 3 });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task Delete_OwnFriendship_AsMember_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsMember(userId: 4);
            // kreiraj svježe prijateljstvo 4 → 2 pa ga obriši
            var createResponse = await client.PostAsJsonAsync("/api/friendships", new FriendshipInputDto { ToUserId = 2 });
            var created = await createResponse.Content.ReadFromJsonAsync<FriendshipDto>();

            var response = await client.DeleteAsync($"/api/friendships/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/friendships/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_SomeoneElsesFriendship_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 3);
            // seed prijateljstvo 1 pripada korisniku 1
            var response = await client.DeleteAsync("/api/friendships/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/friendships/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
