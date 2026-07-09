using System.Text.Json;
using System.Text.Json.Serialization;
using Cugger.Data;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Services
{
    public class AiParseResult
    {
        [JsonPropertyName("entityType")]
        public string EntityType { get; set; } = "unknown";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("style")]
        public string? Style { get; set; }

        [JsonPropertyName("abv")]
        public double? Abv { get; set; }

        [JsonPropertyName("ibu")]
        public int? Ibu { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("breweryName")]
        public string? BreweryName { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("foundedYear")]
        public int? FoundedYear { get; set; }

        [JsonPropertyName("websiteUrl")]
        public string? WebsiteUrl { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("beerName")]
        public string? BeerName { get; set; }

        [JsonPropertyName("venueName")]
        public string? VenueName { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }


    public class AiEntryService
    {
        private readonly CuggerDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AiEntryService> _logger;


        public AiEntryService(
            CuggerDbContext db,
            IConfiguration config,
            ILogger<AiEntryService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }


        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_config["Gemini:ApiKey"])
            ||
            !string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            );


        private Client CreateClient()
        {
            var key = _config["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(key))
            {
                key = Environment.GetEnvironmentVariable(
                    "GEMINI_API_KEY");
            }


            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException(
                    "Gemini API ključ nije konfiguriran."
                );


            return new Client(apiKey: key);
        }



        public async Task<AiParseResult> ParseAsync(string prompt)
        {
            var client = CreateClient();


            var breweries =
                await _db.Breweries
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .Take(100)
                .ToListAsync();


            var beers =
                await _db.Beers
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .Take(150)
                .ToListAsync();


            var venues =
                await _db.Venues
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .Take(100)
                .ToListAsync();



            var systemPrompt = $"""
Ti si AI asistent za Cugger aplikaciju.

Pretvori korisnikov prirodni jezični upit u JSON.

Vrste:
beer
brewery
venue
checkin
review
unknown


Pravila:

- checkin/review moraju koristiti postojeće pivo
- ako postoji točno ime koristi ga
- popuni samo poznata polja
- message mora biti kratko objašnjenje na hrvatskom


Postojeće pivovare:
{string.Join(", ", breweries)}


Postojeća piva:
{string.Join(", ", beers)}


Postojeći lokali:
{string.Join(", ", venues)}
""";



            var schema = new Schema
            {
                Type = "OBJECT",

                Properties = new Dictionary<string, Schema>
                {
                    ["entityType"] = new()
                    {
                        Type="STRING"
                    },

                    ["message"] = new()
                    {
                        Type="STRING"
                    },

                    ["name"] = new()
                    {
                        Type="STRING"
                    },

                    ["style"] = new()
                    {
                        Type="STRING"
                    },

                    ["abv"] = new()
                    {
                        Type="NUMBER"
                    },

                    ["ibu"] = new()
                    {
                        Type="INTEGER"
                    },

                    ["description"] = new()
                    {
                        Type="STRING"
                    },

                    ["breweryName"] = new()
                    {
                        Type="STRING"
                    },

                    ["country"] = new()
                    {
                        Type="STRING"
                    },

                    ["city"] = new()
                    {
                        Type="STRING"
                    },

                    ["address"] = new()
                    {
                        Type="STRING"
                    },

                    ["foundedYear"] = new()
                    {
                        Type="INTEGER"
                    },

                    ["websiteUrl"] = new()
                    {
                        Type="STRING"
                    },

                    ["latitude"] = new()
                    {
                        Type="NUMBER"
                    },

                    ["longitude"] = new()
                    {
                        Type="NUMBER"
                    },

                    ["beerName"] = new()
                    {
                        Type="STRING"
                    },

                    ["venueName"] = new()
                    {
                        Type="STRING"
                    },

                    ["rating"] = new()
                    {
                        Type="NUMBER"
                    },

                    ["comment"] = new()
                    {
                        Type="STRING"
                    }
                },

                Required =
                [
                    "entityType",
                    "message"
                ]
            };



            var model =
                _config["Gemini:Model"]
                ??
                "gemini-2.5-flash";



            _logger.LogInformation(
                "Gemini AI unos: {Prompt}",
                prompt);



            var response =
                await client.Models.GenerateContentAsync(
                    model,
                    prompt,
                    new GenerateContentConfig
                    {
                        SystemInstruction =
                            new Content(systemPrompt),

                        ResponseMimeType =
                            "application/json",

                        ResponseSchema =
                            schema
                    });



            var json = response.Text;



            _logger.LogInformation(
                "Gemini odgovor: {Json}",
                json);



            return JsonSerializer.Deserialize<AiParseResult>(json)
                ??
                new AiParseResult
                {
                    EntityType="unknown",
                    Message="Gemini nije vratio rezultat."
                };
        }
    }
}