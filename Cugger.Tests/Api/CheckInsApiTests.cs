using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>Lab-5: integracijski testovi za /api/checkins (CRUD + pretraga + vlasništvo).</summary>
    public class CheckInsApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public CheckInsApiTests(CuggerApiFactory factory) => _factory = factory;

        private static CheckInInputDto ValidCheckIn() => new()
        {
            BeerId = 4, // Stella Artois — seed pivo bez check-inova
            VenueId = 5,
            Rating = 4.5,
            Comment = "Testni check-in"
        };

        [Fact]
        public async Task GetAll_ReturnsSeededCheckIns_WithNestedDtos()
        {
            var client = _factory.CreateClient();
            var checkIns = await client.GetFromJsonAsync<List<CheckInDto>>("/api/checkins");

            Assert.NotNull(checkIns);
            Assert.True(checkIns!.Count >= 10);
            Assert.All(checkIns, c =>
            {
                Assert.NotNull(c.User);
                Assert.NotNull(c.Beer);
                Assert.NotNull(c.Venue);
            });
        }

        [Fact]
        public async Task GetAll_FilterByUser_ReturnsOnlyTheirCheckIns()
        {
            var client = _factory.CreateClient();
            var checkIns = await client.GetFromJsonAsync<List<CheckInDto>>("/api/checkins?userId=3");

            Assert.NotNull(checkIns);
            Assert.NotEmpty(checkIns!);
            Assert.All(checkIns!, c => Assert.Equal(3, c.User!.Id));
        }

        [Fact]
        public async Task GetAll_WithSearchQuery_FiltersByComment()
        {
            var client = _factory.CreateClient();
            var checkIns = await client.GetFromJsonAsync<List<CheckInDto>>("/api/checkins?q=guinness");

            Assert.NotNull(checkIns);
            Assert.NotEmpty(checkIns!);
        }

        [Fact]
        public async Task GetById_ReturnsCheckIn_WithNestedUserBeerVenue()
        {
            var client = _factory.CreateClient();
            var checkIn = await client.GetFromJsonAsync<CheckInDto>("/api/checkins/5");

            Assert.NotNull(checkIn);
            Assert.Equal("stout_fan", checkIn!.User?.Username);
            Assert.Equal("Guinness Extra Stout", checkIn.Beer?.Name);
            Assert.Equal("Irish Pub Dublin", checkIn.Venue?.Name);
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/checkins/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns201_AndAssignsCurrentUser()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var response = await client.PostAsJsonAsync("/api/checkins", ValidCheckIn());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<CheckInDto>();
            Assert.NotNull(created);
            Assert.Equal(2, created!.User?.Id); // UserId nije zadan → trenutni korisnik
            Assert.Equal("Stella Artois", created.Beer?.Name);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/checkins", ValidCheckIn());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_ForAnotherUser_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var input = ValidCheckIn();
            input.UserId = 3; // tuđi račun

            var response = await client.PostAsJsonAsync("/api/checkins", input);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithUnknownBeer_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember();
            var input = ValidCheckIn();
            input.BeerId = 99999;

            var response = await client.PostAsJsonAsync("/api/checkins", input);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithRatingOutOfRange_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember();
            var input = ValidCheckIn();
            input.Rating = 7.5; // izvan [0, 5]

            var response = await client.PostAsJsonAsync("/api/checkins", input);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_OwnCheckIn_AsMember_Returns204_AndPersists()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var createResponse = await client.PostAsJsonAsync("/api/checkins", ValidCheckIn());
            var created = await createResponse.Content.ReadFromJsonAsync<CheckInDto>();

            var update = ValidCheckIn();
            update.Comment = "Izmijenjeni komentar";
            update.Rating = 3.0;
            var response = await client.PutAsJsonAsync($"/api/checkins/{created!.Id}", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetched = await client.GetFromJsonAsync<CheckInDto>($"/api/checkins/{created.Id}");
            Assert.Equal("Izmijenjeni komentar", fetched!.Comment);
            Assert.Equal(3.0, fetched.Rating);
        }

        [Fact]
        public async Task Update_SomeoneElsesCheckIn_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            // seed check-in 1 pripada korisniku 1
            var response = await client.PutAsJsonAsync("/api/checkins/1", ValidCheckIn());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Update_SomeoneElsesCheckIn_AsAdmin_Returns204()
        {
            var client = _factory.CreateClient().AsAdmin();
            // seed check-in 3 pripada korisniku 2 — admin smije
            var update = ValidCheckIn();
            update.Comment = "Admin izmjena";

            var response = await client.PutAsJsonAsync("/api/checkins/3", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PutAsJsonAsync("/api/checkins/99999", ValidCheckIn());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_OwnCheckIn_AsMember_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var createResponse = await client.PostAsJsonAsync("/api/checkins", ValidCheckIn());
            var created = await createResponse.Content.ReadFromJsonAsync<CheckInDto>();

            var response = await client.DeleteAsync($"/api/checkins/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/checkins/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_SomeoneElsesCheckIn_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            // seed check-in 1 pripada korisniku 1
            var response = await client.DeleteAsync("/api/checkins/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/checkins/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
