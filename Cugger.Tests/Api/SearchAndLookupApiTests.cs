using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>
    /// Integracijski testovi za MVC API endpointe koji nisu dio REST API-ja:
    /// autocomplete lookup (/api/lookup/*), AJAX pretraga (/api/search/*),
    /// globalna pretraga (/api/search/global) te filter/paging varijante REST API-ja.
    /// </summary>
    public class SearchAndLookupApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public SearchAndLookupApiTests(CuggerApiFactory factory) => _factory = factory;

        private record LookupItem(int Id, string Label, string? SubLabel);
        private record GlobalSearchItem(string Label, string? SubLabel, string Url, string Icon);
        private record GlobalSearchGroup(string Name, List<GlobalSearchItem> Items);
        private record GlobalSearchResponse(string? Query, List<GlobalSearchGroup> Groups);

        // ====== /api/lookup/* (autocomplete) ======

        [Fact]
        public async Task LookupBeers_FiltersByName()
        {
            var client = _factory.CreateClient();
            var items = await client.GetFromJsonAsync<List<LookupItem>>("/api/lookup/beers?q=ipa");

            Assert.NotNull(items);
            Assert.NotEmpty(items!);
            Assert.All(items!, i => Assert.Contains("ipa", (i.Label + i.SubLabel).ToLower()));
        }

        [Fact]
        public async Task LookupBreweries_FiltersByCity()
        {
            var client = _factory.CreateClient();
            var items = await client.GetFromJsonAsync<List<LookupItem>>("/api/lookup/breweries?q=zagreb");

            Assert.NotNull(items);
            Assert.NotEmpty(items!);
        }

        [Fact]
        public async Task LookupVenues_FiltersByName()
        {
            var client = _factory.CreateClient();
            var items = await client.GetFromJsonAsync<List<LookupItem>>("/api/lookup/venues?q=dublin");

            Assert.NotNull(items);
            Assert.Single(items!);
        }

        [Fact]
        public async Task LookupUsers_FiltersByUsername()
        {
            // Regresija: upit je koristio [NotMapped] alias Username i pucao u runtimeu
            var client = _factory.CreateClient();
            var items = await client.GetFromJsonAsync<List<LookupItem>>("/api/lookup/users?q=hop_king");

            Assert.NotNull(items);
            Assert.Single(items!);
            Assert.Equal("@hop_king", items![0].SubLabel);
        }

        // ====== /api/search/global (globalna pretraga) ======

        [Fact]
        public async Task GlobalSearch_FindsBeersAndPages()
        {
            var client = _factory.CreateClient();
            var result = await client.GetFromJsonAsync<GlobalSearchResponse>("/api/search/global?q=ipa");

            Assert.NotNull(result);
            var beersGroup = result!.Groups.FirstOrDefault(g => g.Name == "Piva");
            Assert.NotNull(beersGroup);
            Assert.Contains(beersGroup!.Items, i => i.Label.Contains("IPA"));
        }

        [Fact]
        public async Task GlobalSearch_FindsMenuPages()
        {
            var client = _factory.CreateClient();
            var result = await client.GetFromJsonAsync<GlobalSearchResponse>("/api/search/global?q=pivovare");

            Assert.NotNull(result);
            var pagesGroup = result!.Groups.FirstOrDefault(g => g.Name == "Stranice");
            Assert.NotNull(pagesGroup);
            Assert.Contains(pagesGroup!.Items, i => i.Url == "/Brewery");
        }

        [Fact]
        public async Task GlobalSearch_EmptyQuery_ReturnsQuickLinks()
        {
            var client = _factory.CreateClient();
            var result = await client.GetFromJsonAsync<GlobalSearchResponse>("/api/search/global");

            Assert.NotNull(result);
            var pagesGroup = result!.Groups.FirstOrDefault(g => g.Name == "Stranice");
            Assert.NotNull(pagesGroup);
            Assert.True(pagesGroup!.Items.Count >= 5);
        }

        [Fact]
        public async Task GlobalSearch_FindsUsers()
        {
            // Regresija za [NotMapped] Username fix — pretraga korisnika u globalnom searchu
            var client = _factory.CreateClient();
            var result = await client.GetFromJsonAsync<GlobalSearchResponse>("/api/search/global?q=hop_king");

            Assert.NotNull(result);
            var usersGroup = result!.Groups.FirstOrDefault(g => g.Name == "Korisnici");
            Assert.NotNull(usersGroup);
            Assert.Contains(usersGroup!.Items, i => i.SubLabel == "@hop_king");
        }

        // ====== /api/search/* (AJAX partial HTML) ======

        [Theory]
        [InlineData("/api/search/beers?q=ipa", "IPA")]
        [InlineData("/api/search/breweries?q=zagreb", "Zagreb")]
        [InlineData("/api/search/venues?q=dublin", "Dublin")]
        [InlineData("/api/search/users?q=hop_king", "hop_king")]
        public async Task SearchPartials_ReturnHtmlWithMatch(string url, string expectedContent)
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedContent, html, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("/api/search/checkins?q=guinness")]
        [InlineData("/api/search/reviews?q=stout")]
        public async Task SearchPartials_CheckInsAndReviews_Return200(string url)
        {
            // Regresija: pretraga po korisniku unutar check-inova/recenzija koristila
            // [NotMapped] Username — sada UserName (prevodivo u SQL)
            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ====== REST API filter/paging varijante (nepokrivene u postojećim testovima) ======

        [Fact]
        public async Task Beers_FilterByBreweryId_ReturnsOnlyThatBrewery()
        {
            var client = _factory.CreateClient();
            var beers = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers?breweryId=1");

            Assert.NotNull(beers);
            Assert.NotEmpty(beers!);
            Assert.All(beers!, b => Assert.Equal(1, b.Brewery!.Id));
        }

        [Fact]
        public async Task Beers_SortByAbv_ReturnsDescending()
        {
            var client = _factory.CreateClient();
            var beers = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers?sort=abv");

            Assert.NotNull(beers);
            var abvs = beers!.Select(b => b.Abv).ToList();
            Assert.Equal(abvs.OrderByDescending(a => a).ToList(), abvs);
        }

        [Fact]
        public async Task Beers_Paging_RespectsPageSize()
        {
            var client = _factory.CreateClient();
            var page1 = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers?page=1&pageSize=3");
            var page2 = await client.GetFromJsonAsync<List<BeerDto>>("/api/beers?page=2&pageSize=3");

            Assert.NotNull(page1);
            Assert.Equal(3, page1!.Count);
            Assert.NotNull(page2);
            Assert.NotEmpty(page2!);
            // Stranice se ne smiju preklapati
            Assert.Empty(page1.Select(b => b.Id).Intersect(page2!.Select(b => b.Id)));
        }

        [Fact]
        public async Task CheckIns_FilterByBeerId_ReturnsOnlyThatBeer()
        {
            var client = _factory.CreateClient();
            var checkIns = await client.GetFromJsonAsync<List<CheckInDto>>("/api/checkins?beerId=1");

            Assert.NotNull(checkIns);
            Assert.NotEmpty(checkIns!);
            Assert.All(checkIns!, c => Assert.Equal(1, c.Beer!.Id));
        }

        [Fact]
        public async Task CheckIns_FilterByVenueId_ReturnsOnlyThatVenue()
        {
            var client = _factory.CreateClient();
            var checkIns = await client.GetFromJsonAsync<List<CheckInDto>>("/api/checkins?venueId=1");

            Assert.NotNull(checkIns);
            Assert.NotEmpty(checkIns!);
            Assert.All(checkIns!, c => Assert.Equal(1, c.Venue!.Id));
        }
    }
}
