using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>Lab-5: integracijski testovi za /api/venues (CRUD + pretraga + autorizacija).</summary>
    public class VenuesApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public VenuesApiTests(CuggerApiFactory factory) => _factory = factory;

        private static VenueInputDto ValidVenue(string name = "Test Pub") => new()
        {
            Name = name,
            Address = "Testna ulica 1",
            City = "Osijek",
            Country = "Hrvatska",
            Latitude = 45.55,
            Longitude = 18.69
        };

        [Fact]
        public async Task GetAll_ReturnsSeededVenues()
        {
            var client = _factory.CreateClient();
            var venues = await client.GetFromJsonAsync<List<VenueDto>>("/api/venues");

            Assert.NotNull(venues);
            Assert.True(venues!.Count >= 5);
        }

        [Fact]
        public async Task GetAll_WithSearchQuery_FiltersResults()
        {
            var client = _factory.CreateClient();
            var venues = await client.GetFromJsonAsync<List<VenueDto>>("/api/venues?q=dublin");

            Assert.NotNull(venues);
            Assert.Single(venues!);
            Assert.Equal("Irish Pub Dublin", venues![0].Name);
        }

        [Fact]
        public async Task GetById_ReturnsVenue_WithCheckInCount()
        {
            var client = _factory.CreateClient();
            var venue = await client.GetFromJsonAsync<VenueDto>("/api/venues/1");

            Assert.NotNull(venue);
            Assert.Equal("The Beer Garden", venue!.Name);
            Assert.True(venue.CheckInCount > 0);
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/venues/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAdmin_Returns201_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PostAsJsonAsync("/api/venues", ValidVenue("Novi Pub"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<VenueDto>();
            Assert.Equal("Novi Pub", created!.Name);

            var fetched = await client.GetFromJsonAsync<VenueDto>($"/api/venues/{created.Id}");
            Assert.Equal("Novi Pub", fetched!.Name);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/venues", ValidVenue());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsJsonAsync("/api/venues", ValidVenue());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithMissingAddress_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidVenue();
            invalid.Address = "";

            var response = await client.PostAsJsonAsync("/api/venues", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidLatitude_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidVenue();
            invalid.Latitude = 123.45; // izvan [-90, 90]

            var response = await client.PostAsJsonAsync("/api/venues", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_AsAdmin_Returns204_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/venues", ValidVenue("Za Izmjenu"));
            var created = await createResponse.Content.ReadFromJsonAsync<VenueDto>();

            var update = ValidVenue("Izmijenjeni Pub");
            update.City = "Varaždin";
            var response = await client.PutAsJsonAsync($"/api/venues/{created!.Id}", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetched = await client.GetFromJsonAsync<VenueDto>($"/api/venues/{created.Id}");
            Assert.Equal("Izmijenjeni Pub", fetched!.Name);
            Assert.Equal("Varaždin", fetched.City);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PutAsJsonAsync("/api/venues/99999", ValidVenue());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_FreshVenue_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/venues", ValidVenue("Za Brisanje"));
            var created = await createResponse.Content.ReadFromJsonAsync<VenueDto>();

            var response = await client.DeleteAsync($"/api/venues/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/venues/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_VenueWithCheckIns_Returns409Conflict()
        {
            var client = _factory.CreateClient().AsAdmin();
            // seed lokal 1 ima check-inove
            var response = await client.DeleteAsync("/api/venues/1");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/venues/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
