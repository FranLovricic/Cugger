using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic;
using Anthropic.Models.Messages;
using Cugger.Data;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Services
{
    /// <summary>
    /// Rezultat AI parsiranja korisnikovog upita u strukturirani unos podataka.
    /// </summary>
    public class AiParseResult
    {
        [JsonPropertyName("entityType")]
        public string EntityType { get; set; } = "unknown";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        // Beer / Brewery / Venue
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("style")] public string? Style { get; set; }
        [JsonPropertyName("abv")] public double? Abv { get; set; }
        [JsonPropertyName("ibu")] public int? Ibu { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("breweryName")] public string? BreweryName { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("address")] public string? Address { get; set; }
        [JsonPropertyName("foundedYear")] public int? FoundedYear { get; set; }
        [JsonPropertyName("websiteUrl")] public string? WebsiteUrl { get; set; }
        [JsonPropertyName("latitude")] public double? Latitude { get; set; }
        [JsonPropertyName("longitude")] public double? Longitude { get; set; }

        // CheckIn / Review
        [JsonPropertyName("beerName")] public string? BeerName { get; set; }
        [JsonPropertyName("venueName")] public string? VenueName { get; set; }
        [JsonPropertyName("rating")] public double? Rating { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
    }

    /// <summary>
    /// AI integracija (Claude API): parsira upit na prirodnom jeziku
    /// ("Dodaj pivo Ožujsko, lager 5.2% iz Zagrebačke pivovare") u strukturirani
    /// prijedlog unosa koji korisnik potvrđuje prije spremanja.
    /// </summary>
    public class AiEntryService
    {
        private readonly CuggerDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AiEntryService> _logger;

        public AiEntryService(CuggerDbContext db, IConfiguration config, ILogger<AiEntryService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_config["Anthropic:ApiKey"])
            || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));

        private AnthropicClient CreateClient()
        {
            var apiKey = _config["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "Anthropic API ključ nije konfiguriran. Postavi 'Anthropic:ApiKey' u appsettings.json " +
                    "ili ANTHROPIC_API_KEY varijablu okoline.");

            return new AnthropicClient { ApiKey = apiKey };
        }

        /// <summary>
        /// Parsira korisnikov upit u strukturirani prijedlog unosa (bez spremanja u bazu).
        /// </summary>
        public async Task<AiParseResult> ParseAsync(string prompt)
        {
            var client = CreateClient();

            // Kontekst iz baze — modelu dajemo postojeće entitete radi točnog povezivanja
            var breweries = await _db.Breweries.OrderBy(b => b.Name).Select(b => b.Name).Take(100).ToListAsync();
            var beers = await _db.Beers.OrderBy(b => b.Name).Select(b => b.Name).Take(150).ToListAsync();
            var venues = await _db.Venues.OrderBy(v => v.Name).Select(v => v.Name).Take(100).ToListAsync();

            var systemPrompt = $"""
                Ti si asistent za unos podataka u Cugger — aplikaciju za ocjenjivanje piva (Untappd clone).
                Korisnik prirodnim jezikom (hrvatski ili engleski) opisuje što želi unijeti, a ti to
                pretvaraš u strukturirani JSON.

                Vrste unosa (entityType):
                - "beer"    — novo pivo (name, style, abv, ibu, description, breweryName)
                - "brewery" — nova pivovara (name, country, city, foundedYear, description, websiteUrl)
                - "venue"   — novi lokal/birtija (name, address, city, country, latitude, longitude)
                - "checkin" — check-in piva (beerName, venueName, rating 0-5, comment)
                - "review"  — recenzija piva (beerName, rating 0-5, comment)
                - "unknown" — upit nije razumljiv ili nije unos podataka

                Dozvoljeni stilovi piva (style): Lager, Pilsner, IPA, Stout, Porter, Ale, Wheat, Sour, Cider, Other.
                Ako stil nije jasan, odaberi najbliži ili Other.

                Postojeće pivovare u bazi: {string.Join(", ", breweries)}
                Postojeća piva u bazi: {string.Join(", ", beers)}
                Postojeći lokali u bazi: {string.Join(", ", venues)}

                Pravila:
                - Za checkin/review, beerName MORA odgovarati jednom od postojećih piva (odaberi najsličnije ime s popisa).
                - Za novo pivo, ako korisnik navede pivovaru koja postoji na popisu, koristi TOČNO ime s popisa u breweryName.
                - Popuni samo polja koja se mogu izvesti iz upita; ostala izostavi.
                - "message" je kratko objašnjenje na hrvatskom što si razumio (1 rečenica).
                - Ocjene su na skali 0-5 (npr. "četvorka" = 4, "peterica/savršeno" = 5).
                """;

            var schema = new Dictionary<string, JsonElement>
            {
                ["type"] = JsonSerializer.SerializeToElement("object"),
                ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
                ["required"] = JsonSerializer.SerializeToElement(new[] { "entityType", "message" }),
                ["properties"] = JsonSerializer.SerializeToElement(new Dictionary<string, object>
                {
                    ["entityType"] = new { type = "string", @enum = new[] { "beer", "brewery", "venue", "checkin", "review", "unknown" } },
                    ["message"] = new { type = "string", description = "Kratko objašnjenje na hrvatskom" },
                    ["name"] = new { type = "string" },
                    ["style"] = new { type = "string", @enum = Enum.GetNames<Models.BeerStyle>() },
                    ["abv"] = new { type = "number" },
                    ["ibu"] = new { type = "integer" },
                    ["description"] = new { type = "string" },
                    ["breweryName"] = new { type = "string" },
                    ["country"] = new { type = "string" },
                    ["city"] = new { type = "string" },
                    ["address"] = new { type = "string" },
                    ["foundedYear"] = new { type = "integer" },
                    ["websiteUrl"] = new { type = "string" },
                    ["latitude"] = new { type = "number" },
                    ["longitude"] = new { type = "number" },
                    ["beerName"] = new { type = "string" },
                    ["venueName"] = new { type = "string" },
                    ["rating"] = new { type = "number" },
                    ["comment"] = new { type = "string" },
                }),
            };

            var model = _config["Anthropic:Model"] ?? "claude-opus-4-8";
            _logger.LogInformation("AI unos: šaljem upit modelu {Model}: {Prompt}", model, prompt);

            var response = await client.Messages.Create(new MessageCreateParams
            {
                Model = model,
                MaxTokens = 2048,
                System = systemPrompt,
                OutputConfig = new OutputConfig
                {
                    Format = new JsonOutputFormat { Schema = schema },
                },
                Messages = [new() { Role = Role.User, Content = prompt }],
            });

            var json = string.Concat(response.Content
                .Select(b => b.Value)
                .OfType<TextBlock>()
                .Select(t => t.Text));

            _logger.LogInformation("AI unos: odgovor modela: {Json}", json);

            var result = JsonSerializer.Deserialize<AiParseResult>(json)
                         ?? new AiParseResult { EntityType = "unknown", Message = "Prazan odgovor modela." };
            return result;
        }
    }
}
