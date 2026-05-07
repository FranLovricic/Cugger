using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cugger.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Breweries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FoundedYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breweries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<double>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Beers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Style = table.Column<int>(type: "INTEGER", nullable: false),
                    ABV = table.Column<double>(type: "decimal(4,2)", nullable: false),
                    IBU = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    BreweryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beers_Breweries_BreweryId",
                        column: x => x.BreweryId,
                        principalTable: "Breweries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    BeerId = table.Column<int>(type: "INTEGER", nullable: false),
                    VenueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<double>(type: "decimal(3,2)", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckIns_Beers_BeerId",
                        column: x => x.BeerId,
                        principalTable: "Beers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    BeerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<double>(type: "decimal(3,2)", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Likes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Beers_BeerId",
                        column: x => x.BeerId,
                        principalTable: "Beers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Breweries",
                columns: new[] { "Id", "City", "Country", "Description", "FoundedYear", "LogoUrl", "Name", "WebsiteUrl" },
                values: new object[,]
                {
                    { 1, "Karlovac", "Hrvatska", "Najstarija pivovara u Hrvatskoj", 1854, "", "Karlovačka Pivovara", "https://www.karlovacka.hr" },
                    { 2, "San Diego", "SAD", "Poznata za svoje IPA piva", 1996, "", "Stone Brewing", "https://www.stonebrewing.com" },
                    { 3, "Dublin", "Irska", "Legendarni proizvođač Guinnessa", 1759, "", "Guinness Brewery", "https://www.guinness.com" },
                    { 4, "Zagreb", "Hrvatska", "Hrvatska craft pivovara s karakterom", 2014, "", "Zmajska Pivovara", "https://zmajska.hr" },
                    { 5, "Ellon", "Škotska", "Punk craft revolucija iz Škotske", 2007, "", "BrewDog", "https://brewdog.com" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "Bio", "Email", "FirstName", "LastName", "RegistrationDate", "Username" },
                values: new object[,]
                {
                    { 1, "https://ui-avatars.com/api/?name=Dragan+Maric&background=F59E0B&color=fff", "Apsolventist pivarstva i ljubitelj kvalitetnih piva", "dragan@example.com", "Dragan", "Marić", new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "pivo_lover" },
                    { 2, "https://ui-avatars.com/api/?name=Marko+Horvat&background=D97706&color=fff", "IPA entuzijast, traži nove craft pivovare", "marko@example.com", "Marko", "Horvat", new DateTime(2023, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "hop_king" },
                    { 3, "https://ui-avatars.com/api/?name=Ana+Novak&background=FCD34D&color=111", "Ljubiteljica tamnih piva i europskih pivovara", "ana@example.com", "Ana", "Novak", new DateTime(2023, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "stout_fan" },
                    { 4, "https://ui-avatars.com/api/?name=Luka+Kovac&background=A16207&color=fff", "Putujem svijetom u potrazi za savršenim pivom", "luka@example.com", "Luka", "Kovač", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "craft_explorer" }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "Id", "Address", "City", "Country", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { 1, "Ulica 1, broj 10", "Zagreb", "Hrvatska", 45.814999999999998, 15.981999999999999, "The Beer Garden" },
                    { 2, "Ilica 25", "Zagreb", "Hrvatska", 45.816000000000003, 15.984999999999999, "Craft Beer Pub" },
                    { 3, "O'Connell Street, broj 1", "Dublin", "Irska", 53.348999999999997, -6.2599999999999998, "Irish Pub Dublin" },
                    { 4, "Tkalčićeva 36", "Zagreb", "Hrvatska", 45.814999999999998, 15.978, "Mali Medo" },
                    { 5, "Ulica grada Vukovara 269", "Zagreb", "Hrvatska", 45.798000000000002, 15.989000000000001, "Pivnica Pinta" }
                });

            migrationBuilder.InsertData(
                table: "Beers",
                columns: new[] { "Id", "ABV", "BreweryId", "Description", "IBU", "ImageUrl", "Name", "Style" },
                values: new object[,]
                {
                    { 1, 5.0999999999999996, 1, "Klasično hrvatsko lager pivo", 20, "", "Karlovačko", 0 },
                    { 2, 6.9000000000000004, 2, "Aromatično IPA pivo s bogatom gorčinom", 77, "", "Stone IPA", 2 },
                    { 3, 4.2999999999999998, 3, "Klasični Guinness Stout sa karakterističnom tamnom bojom", 45, "", "Guinness Extra Stout", 3 },
                    { 4, 5.0, 1, "Premium belgijsko pilsner pivo", 30, "", "Stella Artois", 1 },
                    { 5, 7.7000000000000002, 2, "Ekstremno hopno IPA s intenzivnom gorčinom", 100, "", "Stone Ruination", 2 },
                    { 6, 5.2000000000000002, 4, "Zmajska Pale Ale - hrvatski craft klasik", 35, "", "Pale Ale", 5 },
                    { 7, 5.5999999999999996, 5, "Trans-atlantska post-punk IPA", 40, "", "Punk IPA", 2 },
                    { 8, 4.7999999999999998, 4, "Tradicionalno pšenično pivo s notama citrusa", 12, "", "Pšenica", 6 }
                });

            migrationBuilder.InsertData(
                table: "Friendships",
                columns: new[] { "Id", "CreatedAt", "FromUserId", "ToUserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 2 },
                    { 2, new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 1 },
                    { 3, new DateTime(2024, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 3 },
                    { 4, new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 3 },
                    { 5, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 1 },
                    { 6, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 4 }
                });

            migrationBuilder.InsertData(
                table: "CheckIns",
                columns: new[] { "Id", "BeerId", "CheckInDate", "Comment", "CreatedAt", "Rating", "UserId", "VenueId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Odličan izbor za topli dan", new DateTime(2024, 3, 15, 19, 30, 0, 0, DateTimeKind.Unspecified), 4.0, 1, 1 },
                    { 2, 2, new DateTime(2024, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sjajna IPA, preporučujem svima", new DateTime(2024, 3, 16, 20, 15, 0, 0, DateTimeKind.Unspecified), 4.5, 1, 2 },
                    { 3, 2, new DateTime(2024, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Savršeno! Najbolja IPA koju sam pio", new DateTime(2024, 3, 17, 21, 45, 0, 0, DateTimeKind.Unspecified), 5.0, 2, 1 },
                    { 4, 5, new DateTime(2024, 3, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jako hopno, za prave IPA ljubitelje", new DateTime(2024, 3, 18, 19, 20, 0, 0, DateTimeKind.Unspecified), 4.0, 2, 2 },
                    { 5, 3, new DateTime(2024, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pravi Guinness u Dublinu - nema boljeg!", new DateTime(2024, 3, 19, 18, 0, 0, 0, DateTimeKind.Unspecified), 5.0, 3, 3 },
                    { 6, 1, new DateTime(2024, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dobro hrvatsko pivo, čvrst izbor", new DateTime(2024, 3, 20, 20, 30, 0, 0, DateTimeKind.Unspecified), 3.5, 3, 1 },
                    { 7, 3, new DateTime(2024, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Klasičan Stout, topla preporuka", new DateTime(2024, 3, 21, 19, 0, 0, 0, DateTimeKind.Unspecified), 4.5, 1, 1 },
                    { 8, 6, new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hrvatski craft je stvarno došao daleko", new DateTime(2024, 4, 5, 21, 10, 0, 0, DateTimeKind.Unspecified), 4.5, 4, 4 },
                    { 9, 7, new DateTime(2024, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Punk attitude u svakom gutljaju", new DateTime(2024, 4, 12, 22, 0, 0, 0, DateTimeKind.Unspecified), 4.0, 4, 5 },
                    { 10, 8, new DateTime(2024, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Osvježavajuće, za ljetni dan", new DateTime(2024, 4, 20, 17, 30, 0, 0, DateTimeKind.Unspecified), 3.5, 2, 4 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "BeerId", "Comment", "CreatedAt", "Likes", "Rating", "UserId" },
                values: new object[,]
                {
                    { 1, 2, "Odličan balans između gorčine i arome", new DateTime(2024, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, 4.5, 1 },
                    { 2, 2, "Jedna od najboljih IPA-a koju sam ikad probao", new DateTime(2024, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 23, 5.0, 2 },
                    { 3, 3, "Irski stout kakav treba biti", new DateTime(2024, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, 5.0, 3 },
                    { 4, 6, "Zmajska zna što radi - svaka čast.", new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, 4.5, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beers_BreweryId",
                table: "Beers",
                column: "BreweryId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_BeerId",
                table: "CheckIns",
                column: "BeerId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_UserId",
                table: "CheckIns",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_VenueId",
                table: "CheckIns",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FromUserId",
                table: "Friendships",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_ToUserId",
                table: "Friendships",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BeerId",
                table: "Reviews",
                column: "BeerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropTable(
                name: "Beers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Breweries");
        }
    }
}
