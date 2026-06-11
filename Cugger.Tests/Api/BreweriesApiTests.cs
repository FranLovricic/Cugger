using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>Lab-5: integracijski testovi za /api/breweries (CRUD + pretraga + autorizacija).</summary>
    public class BreweriesApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public BreweriesApiTests(CuggerApiFactory factory) => _factory = factory;

        private static BreweryInputDto ValidBrewery(string name = "Test Pivovara") => new()
        {
            Name = name,
            Country = "Hrvatska",
            City = "Split",
            FoundedYear = 2020,
            Description = "Testna pivovara"
        };

        [Fact]
        public async Task GetAll_ReturnsSeededBreweries_WithBeerCount()
        {
            var client = _factory.CreateClient();
            var breweries = await client.GetFromJsonAsync<List<BreweryDto>>("/api/breweries");

            Assert.NotNull(breweries);
            Assert.True(breweries!.Count >= 5);
            var stone = breweries.Single(b => b.Name == "Stone Brewing");
            Assert.Equal(2, stone.BeerCount);
        }

        [Fact]
        public async Task GetAll_WithSearchQuery_FiltersResults()
        {
            var client = _factory.CreateClient();
            var breweries = await client.GetFromJsonAsync<List<BreweryDto>>("/api/breweries?q=zagreb");

            Assert.NotNull(breweries);
            Assert.Contains(breweries!, b => b.Name == "Zmajska Pivovara");
            Assert.All(breweries!, b => Assert.Contains("zagreb", (b.Name + b.City + b.Country + b.Description).ToLower()));
        }

        [Fact]
        public async Task GetById_ReturnsBrewery_WithNestedBeers()
        {
            var client = _factory.CreateClient();
            var brewery = await client.GetFromJsonAsync<BreweryDto>("/api/breweries/2");

            Assert.NotNull(brewery);
            Assert.Equal("Stone Brewing", brewery!.Name);
            Assert.NotNull(brewery.Beers);
            Assert.Equal(2, brewery.Beers!.Count);
            Assert.Contains(brewery.Beers, b => b.Name == "Stone IPA");
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/breweries/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAdmin_Returns201_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PostAsJsonAsync("/api/breweries", ValidBrewery("Nova Pivovara"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<BreweryDto>();
            Assert.Equal("Nova Pivovara", created!.Name);

            var fetched = await client.GetFromJsonAsync<BreweryDto>($"/api/breweries/{created.Id}");
            Assert.Equal("Nova Pivovara", fetched!.Name);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/breweries", ValidBrewery());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsJsonAsync("/api/breweries", ValidBrewery());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithMissingCountry_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidBrewery();
            invalid.Country = "";

            var response = await client.PostAsJsonAsync("/api/breweries", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidFoundedYear_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidBrewery();
            invalid.FoundedYear = 50; // izvan [1000, 2100]

            var response = await client.PostAsJsonAsync("/api/breweries", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_AsAdmin_Returns204_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/breweries", ValidBrewery("Za Izmjenu"));
            var created = await createResponse.Content.ReadFromJsonAsync<BreweryDto>();

            var update = ValidBrewery("Izmijenjena Pivovara");
            update.City = "Rijeka";
            var response = await client.PutAsJsonAsync($"/api/breweries/{created!.Id}", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetched = await client.GetFromJsonAsync<BreweryDto>($"/api/breweries/{created.Id}");
            Assert.Equal("Izmijenjena Pivovara", fetched!.Name);
            Assert.Equal("Rijeka", fetched.City);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PutAsJsonAsync("/api/breweries/99999", ValidBrewery());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_FreshBrewery_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/breweries", ValidBrewery("Za Brisanje"));
            var created = await createResponse.Content.ReadFromJsonAsync<BreweryDto>();

            var response = await client.DeleteAsync($"/api/breweries/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/breweries/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_BreweryWithActiveBeers_Returns409Conflict()
        {
            var client = _factory.CreateClient().AsAdmin();
            // seed pivovara 2 (Stone) ima piva s check-inovima
            var response = await client.DeleteAsync("/api/breweries/2");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/breweries/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
