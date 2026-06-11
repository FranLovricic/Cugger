using System.Net;
using System.Net.Http.Json;
using Cugger.Models.Dto;
using Cugger.Tests.Infrastructure;
using Xunit;

namespace Cugger.Tests.Api
{
    /// <summary>
    /// Lab-5: integracijski testovi za upload datoteka uz pivo
    /// (/api/beers/{id}/photos + /api/photos/{id}).
    /// </summary>
    public class BeerPhotosApiTests : IClassFixture<CuggerApiFactory>
    {
        private readonly CuggerApiFactory _factory;

        public BeerPhotosApiTests(CuggerApiFactory factory) => _factory = factory;

        private static MultipartFormDataContent PngUpload(string fileName = "test.png")
        {
            // Minimalni PNG header — dovoljno za upload test
            var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(fileContent, "file", fileName);
            return content;
        }

        [Fact]
        public async Task GetPhotos_ForSeededBeer_ReturnsEmptyList()
        {
            var client = _factory.CreateClient();
            var photos = await client.GetFromJsonAsync<List<BeerPhotoDto>>("/api/beers/1/photos");

            Assert.NotNull(photos);
        }

        [Fact]
        public async Task GetPhotos_UnknownBeer_Returns404()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/beers/99999/photos");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Upload_AsMember_Returns201_AndAppearsInList()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var response = await client.PostAsync("/api/beers/2/photos", PngUpload("moja-fotka.png"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<BeerPhotoDto>();
            Assert.NotNull(created);
            Assert.Equal(2, created!.BeerId);
            Assert.Equal("moja-fotka.png", created.FileName);
            Assert.StartsWith("/uploads/beers/2/", created.Url);
            Assert.Equal(2, created.UploadedBy?.Id);

            // AJAX lista datoteka mora sadržavati novu datoteku
            var photos = await client.GetFromJsonAsync<List<BeerPhotoDto>>("/api/beers/2/photos");
            Assert.Contains(photos!, p => p.Id == created.Id);
        }

        [Fact]
        public async Task Upload_AsAnonymous_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("/api/beers/2/photos", PngUpload());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Upload_ToUnknownBeer_Returns404()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsync("/api/beers/99999/photos", PngUpload());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Upload_DisallowedExtension_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsync("/api/beers/2/photos", PngUpload("virus.exe"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Upload_WithoutFile_Returns400ValidationError()
        {
            var client = _factory.CreateClient().AsMember();
            var response = await client.PostAsync("/api/beers/2/photos", new MultipartFormDataContent { { new StringContent("x"), "nesto" } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Delete_OwnPhoto_AsMember_Returns204_AndRemovesFromList()
        {
            var client = _factory.CreateClient().AsMember(userId: 2);
            var uploadResponse = await client.PostAsync("/api/beers/3/photos", PngUpload("za-brisanje.png"));
            var created = await uploadResponse.Content.ReadFromJsonAsync<BeerPhotoDto>();

            var response = await client.DeleteAsync($"/api/photos/{created!.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var photos = await client.GetFromJsonAsync<List<BeerPhotoDto>>("/api/beers/3/photos");
            Assert.DoesNotContain(photos!, p => p.Id == created.Id);
        }

        [Fact]
        public async Task Delete_SomeoneElsesPhoto_AsMember_Returns403()
        {
            var admin = _factory.CreateClient().AsAdmin();
            var uploadResponse = await admin.PostAsync("/api/beers/4/photos", PngUpload("adminova.png"));
            var created = await uploadResponse.Content.ReadFromJsonAsync<BeerPhotoDto>();

            var member = _factory.CreateClient().AsMember(userId: 3);
            var response = await member.DeleteAsync($"/api/photos/{created!.Id}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Delete_SomeoneElsesPhoto_AsAdmin_Returns204()
        {
            var member = _factory.CreateClient().AsMember(userId: 2);
            var uploadResponse = await member.PostAsync("/api/beers/5/photos", PngUpload("memberova.png"));
            var created = await uploadResponse.Content.ReadFromJsonAsync<BeerPhotoDto>();

            var admin = _factory.CreateClient().AsAdmin();
            var response = await admin.DeleteAsync($"/api/photos/{created!.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_Returns404()
        {
            var client = _factory.CreateClient().AsAdmin();
            var response = await client.DeleteAsync("/api/photos/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
