using System;
using Microsoft.EntityFrameworkCore.Metadata;
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
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Breweries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FoundedYear = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WebsiteUrl = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breweries", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegistrationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvatarUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Beers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Style = table.Column<int>(type: "int", nullable: false),
                    ABV = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    IBU = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BreweryId = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromUserId = table.Column<int>(type: "int", nullable: false),
                    ToUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BeerPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BeerId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "varchar(260)", maxLength: 260, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StoredFileName = table.Column<string>(type: "varchar(260)", maxLength: 260, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    RelativePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeerPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeerPhotos_Beers_BeerId",
                        column: x => x.BeerId,
                        principalTable: "Beers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeerPhotos_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BeerId = table.Column<int>(type: "int", nullable: false),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Comment = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheckInDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BeerId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    Comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Likes = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, "a0000000-0000-0000-0000-000000000001", "Admin", "ADMIN" },
                    { 2, "a0000000-0000-0000-0000-000000000002", "Member", "MEMBER" }
                });

            migrationBuilder.InsertData(
                table: "Breweries",
                columns: new[] { "Id", "City", "Country", "Description", "FoundedYear", "LogoUrl", "Name", "WebsiteUrl" },
                values: new object[,]
                {
                    { 1, "Karlovac", "Hrvatska", "Najstarija pivovara u Hrvatskoj", 1854, "", "Karlovačka Pivovara", "https://www.karlovacka.hr" },
                    { 2, "San Diego", "SAD", "Poznata za svoje IPA piva", 1996, "", "Stone Brewing", "https://www.stonebrewing.com" },
                    { 3, "Dublin", "Irska", "Legendarni proizvođač Guinness piva", 1759, "", "Guinness Brewery", "https://www.guinness.com" },
                    { 4, "Zagreb", "Hrvatska", "Hrvatska craft pivovara s karakterom", 2014, "", "Zmajska Pivovara", "https://zmajska.hr" },
                    { 5, "Ellon", "Škotska", "Punk craft revolucija iz Škotske", 2007, "", "BrewDog", "https://brewdog.com" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "AvatarUrl", "Bio", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RegistrationDate", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { 1, 0, "https://ui-avatars.com/api/?name=Dragan+Maric&background=F59E0B&color=fff", "Apsolvent pivarstva i ljubitelj kvalitetnih piva", "c0000000-0000-0000-0000-000000000001", "dragan@example.com", true, "Dragan", "Marić", true, null, "DRAGAN@EXAMPLE.COM", "PIVO_LOVER", "SEED_NEEDS_HASH", null, false, new DateTime(2023, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "b0000000-0000-0000-0000-000000000001", false, "pivo_lover" },
                    { 2, 0, "https://ui-avatars.com/api/?name=Marko+Horvat&background=D97706&color=fff", "IPA entuzijast, traži nove craft pivovare", "c0000000-0000-0000-0000-000000000002", "marko@example.com", true, "Marko", "Horvat", true, null, "MARKO@EXAMPLE.COM", "HOP_KING", "SEED_NEEDS_HASH", null, false, new DateTime(2023, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "b0000000-0000-0000-0000-000000000002", false, "hop_king" },
                    { 3, 0, "https://ui-avatars.com/api/?name=Ana+Novak&background=FCD34D&color=111", "Ljubiteljica tamnih piva i europskih pivovara", "c0000000-0000-0000-0000-000000000003", "ana@example.com", true, "Ana", "Novak", true, null, "ANA@EXAMPLE.COM", "STOUT_FAN", "SEED_NEEDS_HASH", null, false, new DateTime(2023, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "b0000000-0000-0000-0000-000000000003", false, "stout_fan" },
                    { 4, 0, "https://ui-avatars.com/api/?name=Luka+Kovac&background=A16207&color=fff", "Putujem svijetom u potrazi za savršenim pivom", "c0000000-0000-0000-0000-000000000004", "luka@example.com", true, "Luka", "Kovač", true, null, "LUKA@EXAMPLE.COM", "CRAFT_EXPLORER", "SEED_NEEDS_HASH", null, false, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "b0000000-0000-0000-0000-000000000004", false, "craft_explorer" }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "Id", "Address", "City", "Country", "Latitude", "Longitude", "Name" },
                values: new object[,]
                {
                    { 1, "Ulica 1, broj 10", "Zagreb", "Hrvatska", 45.815m, 15.982m, "The Beer Garden" },
                    { 2, "Ilica 25", "Zagreb", "Hrvatska", 45.816m, 15.985m, "Craft Beer Pub" },
                    { 3, "O'Connell Street, broj 1", "Dublin", "Irska", 53.349m, -6.26m, "Irish Pub Dublin" },
                    { 4, "Tkalčićeva 36", "Zagreb", "Hrvatska", 45.815m, 15.978m, "Mali Medo" },
                    { 5, "Ulica grada Vukovara 269", "Zagreb", "Hrvatska", 45.798m, 15.989m, "Pivnica Pinta" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 }
                });

            migrationBuilder.InsertData(
                table: "Beers",
                columns: new[] { "Id", "ABV", "BreweryId", "Description", "IBU", "ImageUrl", "Name", "Style" },
                values: new object[,]
                {
                    { 1, 5.1m, 1, "Klasično hrvatsko lager pivo", 20, "", "Karlovačko", 0 },
                    { 2, 6.9m, 2, "Aromatično IPA pivo s bogatom gorčinom", 77, "", "Stone IPA", 2 },
                    { 3, 4.3m, 3, "Klasični Guinness Stout s karakterističnom tamnom bojom", 45, "", "Guinness Extra Stout", 3 },
                    { 4, 5m, 1, "Premium belgijsko pilsner pivo", 30, "", "Stella Artois", 1 },
                    { 5, 7.7m, 2, "Ekstremno hopna IPA s intenzivnom gorčinom", 100, "", "Stone Ruination", 2 },
                    { 6, 5.2m, 4, "Zmajska Pale Ale - hrvatski craft klasik", 35, "", "Pale Ale", 5 },
                    { 7, 5.6m, 5, "Trans-atlantska post-punk IPA", 40, "", "Punk IPA", 2 },
                    { 8, 4.8m, 4, "Tradicionalno pšenično pivo s notama citrusa", 12, "", "Pšenica", 6 }
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
                    { 1, 1, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Odličan izbor za topli dan", new DateTime(2024, 3, 15, 19, 30, 0, 0, DateTimeKind.Unspecified), 4m, 1, 1 },
                    { 2, 2, new DateTime(2024, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sjajna IPA, preporučujem svima", new DateTime(2024, 3, 16, 20, 15, 0, 0, DateTimeKind.Unspecified), 4.5m, 1, 2 },
                    { 3, 2, new DateTime(2024, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Savršeno! Najbolja IPA koju sam pio", new DateTime(2024, 3, 17, 21, 45, 0, 0, DateTimeKind.Unspecified), 5m, 2, 1 },
                    { 4, 5, new DateTime(2024, 3, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jako hopno, za prave IPA ljubitelje", new DateTime(2024, 3, 18, 19, 20, 0, 0, DateTimeKind.Unspecified), 4m, 2, 2 },
                    { 5, 3, new DateTime(2024, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pravi Guinness u Dublinu - nema boljeg!", new DateTime(2024, 3, 19, 18, 0, 0, 0, DateTimeKind.Unspecified), 5m, 3, 3 },
                    { 6, 1, new DateTime(2024, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dobro hrvatsko pivo, čvrst izbor", new DateTime(2024, 3, 20, 20, 30, 0, 0, DateTimeKind.Unspecified), 3.5m, 3, 1 },
                    { 7, 3, new DateTime(2024, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Klasičan Stout, topla preporuka", new DateTime(2024, 3, 21, 19, 0, 0, 0, DateTimeKind.Unspecified), 4.5m, 1, 1 },
                    { 8, 6, new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hrvatski craft je stvarno došao daleko", new DateTime(2024, 4, 5, 21, 10, 0, 0, DateTimeKind.Unspecified), 4.5m, 4, 4 },
                    { 9, 7, new DateTime(2024, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Punk attitude u svakom gutljaju", new DateTime(2024, 4, 12, 22, 0, 0, 0, DateTimeKind.Unspecified), 4m, 4, 5 },
                    { 10, 8, new DateTime(2024, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Osvježavajuće, za ljetni dan", new DateTime(2024, 4, 20, 17, 30, 0, 0, DateTimeKind.Unspecified), 3.5m, 2, 4 }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "BeerId", "Comment", "CreatedAt", "Likes", "Rating", "UserId" },
                values: new object[,]
                {
                    { 1, 2, "Odličan balans između gorčine i arome", new DateTime(2024, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, 4.5m, 1 },
                    { 2, 2, "Jedna od najboljih IPA-a koju sam ikad probao", new DateTime(2024, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 23, 5m, 2 },
                    { 3, 3, "Irski stout kakav treba biti", new DateTime(2024, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 18, 5m, 3 },
                    { 4, 6, "Zmajska zna što radi - svaka čast.", new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, 4.5m, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BeerPhotos_BeerId",
                table: "BeerPhotos",
                column: "BeerId");

            migrationBuilder.CreateIndex(
                name: "IX_BeerPhotos_UploadedByUserId",
                table: "BeerPhotos",
                column: "UploadedByUserId");

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
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BeerPhotos");

            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

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
