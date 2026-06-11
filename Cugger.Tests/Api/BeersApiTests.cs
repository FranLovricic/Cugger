using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>Lab-5: integracijski testovi za /api/beers (CRUD + pretraga + autorizacija).</summary>
    public class BeersApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public BeersApiTests(CuggerApiFactory factory) => _factory = factory;

        private static BeerInputDto ValidBeer(string name = "Test Lager") => new()
        {
            Name = name,
            Style = "Lager",
            Abv = 4.9,
            Ibu = 22,
            Description = "Testno pivo",
            BreweryId = 1
        };

        // ===== GET (svi + pretraga) =====

        [Fact]
        public async Task GetAll_ReturnsSeededBeers_WithNestedBreweryDto()
        {
            var client = _factory.CreateClient();
            var beers = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers");

            Assert.NotNull(beers);
            Assert.True(beers!.Count >= 8);
            Assert.All(beers, b => Assert.NotNull(b.Brewery));
            Assert.DoesNotContain(beers, b => string.IsNullOrEmpty(b.Name));
        }

        [Fact]
        public async Task GetAll_WithSearchQuery_FiltersResults()
        {
            var client = _factory.CreateClient();
            var beers = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers?q=guinness");

            Assert.NotNull(beers);
            Assert.Single(beers!);
            Assert.Equal("Guinness Extra Stout", beers![0].Name);
        }

        [Fact]
        public async Task GetAll_WithStyleFilter_ReturnsOnlyThatStyle()
        {
            var client = _factory.CreateClient();
            var beers = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers?style=IPA");

            Assert.NotNull(beers);
            Assert.NotEmpty(beers!);
            Assert.All(beers!, b => Assert.Equal("IPA", b.Style));
        }

        // ===== GET (jedan po ID-u) =====

        [Fact]
        public async Task GetById_ReturnsBeer_WithRatingStats()
        {
            var client = _factory.CreateClient();
            var beer = await client.GetFromJsonAsync<BeerDto>("/api/beers/2");

            Assert.NotNull(beer);
            Assert.Equal("Stone IPA", beer!.Name);
            Assert.Equal("Stone Brewing", beer.Brewery?.Name);
            Assert.True(beer.RatingCount > 0);
            Assert.InRange(beer.AverageRating, 0, 5);
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/beers/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ===== POST =====

        [Fact]
        public async Task Create_AsAdmin_Returns201_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PostAsJsonAsync("/api/beers", ValidBeer("Novo Pivo 201"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<BeerDto>();
            Assert.NotNull(created);
            Assert.Equal("Novo Pivo 201", created!.Name);
            Assert.NotNull(response.Headers.Location);

            var fetched = await client.GetFromJsonAsync<BeerDto>($"/api/beers/{created.Id}");
            Assert.Equal("Novo Pivo 201", fetched!.Name);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/beers", ValidBeer());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsJsonAsync("/api/beers", ValidBeer());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithMissingName_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidBeer();
            invalid.Name = "";

            var response = await client.PostAsJsonAsync("/api/beers", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithUnknownStyle_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidBeer();
            invalid.Style = "NepostojeciStil";

            var response = await client.PostAsJsonAsync("/api/beers", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithUnknownBrewery_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidBeer();
            invalid.BreweryId = 99999;

            var response = await client.PostAsJsonAsync("/api/beers", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ===== PUT =====

        [Fact]
        public async Task Update_AsAdmin_Returns204_AndPersists()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/beers", ValidBeer("Za Izmjenu"));
            var created = await createResponse.Content.ReadFromJsonAsync<BeerDto>();

            var update = ValidBeer("Izmijenjeno Pivo");
            update.Abv = 6.5;
            var response = await client.PutAsJsonAsync($"/api/beers/{created!.Id}", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetched = await client.GetFromJsonAsync<BeerDto>($"/api/beers/{created.Id}");
            Assert.Equal("Izmijenjeno Pivo", fetched!.Name);
            Assert.Equal(6.5, fetched.Abv);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PutAsJsonAsync("/api/beers/99999", ValidBeer());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithInvalidBody_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsAdmin();
            var invalid = ValidBeer();
            invalid.Abv = 999; // izvan [0, 70]

            var response = await client.PutAsJsonAsync("/api/beers/1", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PutAsJsonAsync("/api/beers/1", ValidBeer());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ===== DELETE =====

        [Fact]
        public async Task Delete_FreshBeer_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsAdmin();
            var createResponse = await client.PostAsJsonAsync("/api/beers", ValidBeer("Za Brisanje"));
            var created = await createResponse.Content.ReadFromJsonAsync<BeerDto>();

            var response = await client.DeleteAsync($"/api/beers/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/beers/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_BeerWithCheckIns_Returns409Conflict()
        {
            var client = _factory.CreateClient().AsAdmin();
            // seed pivo 1 (Karlovačko) ima check-inove
            var response = await client.DeleteAsync("/api/beers/1");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/beers/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.DeleteAsync("/api/beers/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
