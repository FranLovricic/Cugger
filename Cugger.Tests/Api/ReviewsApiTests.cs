using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>Lab-5: integracijski testovi za /api/reviews (CRUD + pretraga + vlasništvo).</summary>
    public class ReviewsApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public ReviewsApiTests(CuggerApiFactory factory) => _factory = factory;

        private static ReviewInputDto ValidReview() => new()
        {
            BeerId = 4, // Stella Artois
            Rating = 4.0,
            Comment = "Testna recenzija"
        };

        [Fact]
        public async Task GetAll_ReturnsSeededReviews_WithNestedDtos()
        {
            var client = _factory.CreateClient();
            var reviews = await client.GetFromJsonAsync<List<ReviewDto>>("/api/reviews");

            Assert.NotNull(reviews);
            Assert.True(reviews!.Count >= 4);
            Assert.All(reviews, r =>
            {
                Assert.NotNull(r.User);
                Assert.NotNull(r.Beer);
            });
        }

        [Fact]
        public async Task GetAll_FilterByBeer_ReturnsOnlyThatBeersReviews()
        {
            var client = _factory.CreateClient();
            var reviews = await client.GetFromJsonAsync<List<ReviewDto>>("/api/reviews?beerId=2");

            Assert.NotNull(reviews);
            Assert.Equal(2, reviews!.Count);
            Assert.All(reviews, r => Assert.Equal("Stone IPA", r.Beer?.Name));
        }

        [Fact]
        public async Task GetAll_WithMinRatingFilter_FiltersResults()
        {
            var client = _factory.CreateClient();
            var reviews = await client.GetFromJsonAsync<List<ReviewDto>>("/api/reviews?minRating=5");

            Assert.NotNull(reviews);
            Assert.NotEmpty(reviews!);
            Assert.All(reviews!, r => Assert.True(r.Rating >= 5));
        }

        [Fact]
        public async Task GetById_ReturnsReview()
        {
            var client = _factory.CreateClient();
            var review = await client.GetFromJsonAsync<ReviewDto>("/api/reviews/3");

            Assert.NotNull(review);
            Assert.Equal("stout_fan", review!.User?.Username);
            Assert.Equal("Guinness Extra Stout", review.Beer?.Name);
        }

        [Fact]
        public async Task GetById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/reviews/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsMember_Returns201_AndAssignsCurrentUser()
        {
            var client = _factory.CreateClient().AsMember(userId: 3);
            var response = await client.PostAsJsonAsync("/api/reviews", ValidReview());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ReviewDto>();
            Assert.Equal(3, created!.User?.Id);
            Assert.Equal("Stella Artois", created.Beer?.Name);
        }

        [Fact]
        public async Task Create_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/reviews", ValidReview());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithEmptyComment_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember();
            var invalid = ValidReview();
            invalid.Comment = "";

            var response = await client.PostAsJsonAsync("/api/reviews", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithUnknownBeer_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember();
            var invalid = ValidReview();
            invalid.BeerId = 99999;

            var response = await client.PostAsJsonAsync("/api/reviews", invalid);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_OwnReview_AsMember_Returns204_AndPersists()
        {
            var client = _factory.CreateClient().AsMember(userId: 3);
            var createResponse = await client.PostAsJsonAsync("/api/reviews", ValidReview());
            var created = await createResponse.Content.ReadFromJsonAsync<ReviewDto>();

            var update = ValidReview();
            update.Comment = "Izmijenjena recenzija";
            update.Rating = 2.5;
            var response = await client.PutAsJsonAsync($"/api/reviews/{created!.Id}", update);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetched = await client.GetFromJsonAsync<ReviewDto>($"/api/reviews/{created.Id}");
            Assert.Equal("Izmijenjena recenzija", fetched!.Comment);
            Assert.Equal(2.5, fetched.Rating);
        }

        [Fact]
        public async Task Update_SomeoneElsesReview_AsMember_Returns403()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            // seed recenzija 1 pripada korisniku 1
            var response = await client.PutAsJsonAsync("/api/reviews/1", ValidReview());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Update_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.PutAsJsonAsync("/api/reviews/99999", ValidReview());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_OwnReview_AsMember_Returns204_AndRemoves()
        {
            var client = _factory.CreateClient().AsMember(userId: 3);
            var createResponse = await client.PostAsJsonAsync("/api/reviews", ValidReview());
            var created = await createResponse.Content.ReadFromJsonAsync<ReviewDto>();

            var response = await client.DeleteAsync($"/api/reviews/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var fetchAfter = await client.GetAsync($"/api/reviews/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, fetchAfter.StatusCode);
        }

        [Fact]
        public async Task Delete_SomeoneElsesReview_AsAdmin_Returns204()
        {
            var client = _factory.CreateClient().AsAdmin();
            // seed recenzija 4 pripada korisniku 4 — admin smije
            var response = await client.DeleteAsync("/api/reviews/4");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/reviews/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
