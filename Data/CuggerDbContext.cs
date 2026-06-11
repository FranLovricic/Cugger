using Cugger.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Data
{
    /// <summary>
    /// Lab-5: kontekst naslijeđuje IdentityDbContext kako bi ASP.NET Core Identity
    /// (AppUser + role) živio u istoj bazi kao i domenski entiteti.
    /// </summary>
    public class CuggerDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public CuggerDbContext(DbContextOptions<CuggerDbContext> options) : base(options) { }

        public DbSet<Brewery> Breweries { get; set; } = null!;
        public DbSet<Beer> Beers { get; set; } = null!;
        public DbSet<Venue> Venues { get; set; } = null!;
        public DbSet<CheckIn> CheckIns { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Friendship> Friendships { get; set; } = null!;
        public DbSet<BeerPhoto> BeerPhotos { get; set; } = null!;

        // Napomena: DbSet<AppUser> Users dolazi naslijeđen iz IdentityDbContext.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Zadrži naziv tablice "Users" iz prethodnih labova (umjesto default "AspNetUsers")
            modelBuilder.Entity<AppUser>().ToTable("Users");

            // Spriječi cascade-delete cikluse koje SQL Server ne voli
            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.User)
                .WithMany(u => u.CheckIns)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.Beer)
                .WithMany(b => b.CheckIns)
                .HasForeignKey(c => c.BeerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CheckIn>()
                .HasOne(c => c.Venue)
                .WithMany(v => v.CheckIns)
                .HasForeignKey(c => c.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Beer)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BeerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.FromUser)
                .WithMany(u => u.FromFriendships)
                .HasForeignKey(f => f.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.ToUser)
                .WithMany(u => u.ToFriendships)
                .HasForeignKey(f => f.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Beer>()
                .HasOne(b => b.Brewery)
                .WithMany(br => br.Beers)
                .HasForeignKey(b => b.BreweryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lab-5: uploadane datoteke vezane uz konkretno pivo
            modelBuilder.Entity<BeerPhoto>()
                .HasOne(p => p.Beer)
                .WithMany(b => b.Photos)
                .HasForeignKey(p => p.BeerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BeerPhoto>()
                .HasOne(p => p.UploadedBy)
                .WithMany()
                .HasForeignKey(p => p.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder mb)
        {
            mb.Entity<Brewery>().HasData(
                new Brewery { Id = 1, Name = "Karlovačka Pivovara", Country = "Hrvatska", City = "Karlovac", FoundedYear = 1854, Description = "Najstarija pivovara u Hrvatskoj", WebsiteUrl = "https://www.karlovacka.hr", LogoUrl = "" },
                new Brewery { Id = 2, Name = "Stone Brewing", Country = "SAD", City = "San Diego", FoundedYear = 1996, Description = "Poznata za svoje IPA piva", WebsiteUrl = "https://www.stonebrewing.com", LogoUrl = "" },
                new Brewery { Id = 3, Name = "Guinness Brewery", Country = "Irska", City = "Dublin", FoundedYear = 1759, Description = "Legendarni proizvođač Guinness piva", WebsiteUrl = "https://www.guinness.com", LogoUrl = "" },
                new Brewery { Id = 4, Name = "Zmajska Pivovara", Country = "Hrvatska", City = "Zagreb", FoundedYear = 2014, Description = "Hrvatska craft pivovara s karakterom", WebsiteUrl = "https://zmajska.hr", LogoUrl = "" },
                new Brewery { Id = 5, Name = "BrewDog", Country = "Škotska", City = "Ellon", FoundedYear = 2007, Description = "Punk craft revolucija iz Škotske", WebsiteUrl = "https://brewdog.com", LogoUrl = "" }
            );

            mb.Entity<Beer>().HasData(
                new Beer { Id = 1, Name = "Karlovačko", BreweryId = 1, Style = BeerStyle.Lager, ABV = 5.1, IBU = 20, Description = "Klasično hrvatsko lager pivo", ImageUrl = "" },
                new Beer { Id = 2, Name = "Stone IPA", BreweryId = 2, Style = BeerStyle.IPA, ABV = 6.9, IBU = 77, Description = "Aromatično IPA pivo s bogatom gorčinom", ImageUrl = "" },
                new Beer { Id = 3, Name = "Guinness Extra Stout", BreweryId = 3, Style = BeerStyle.Stout, ABV = 4.3, IBU = 45, Description = "Klasični Guinness Stout s karakterističnom tamnom bojom", ImageUrl = "" },
                new Beer { Id = 4, Name = "Stella Artois", BreweryId = 1, Style = BeerStyle.Pilsner, ABV = 5.0, IBU = 30, Description = "Premium belgijsko pilsner pivo", ImageUrl = "" },
                new Beer { Id = 5, Name = "Stone Ruination", BreweryId = 2, Style = BeerStyle.IPA, ABV = 7.7, IBU = 100, Description = "Ekstremno hopna IPA s intenzivnom gorčinom", ImageUrl = "" },
                new Beer { Id = 6, Name = "Pale Ale", BreweryId = 4, Style = BeerStyle.Ale, ABV = 5.2, IBU = 35, Description = "Zmajska Pale Ale - hrvatski craft klasik", ImageUrl = "" },
                new Beer { Id = 7, Name = "Punk IPA", BreweryId = 5, Style = BeerStyle.IPA, ABV = 5.6, IBU = 40, Description = "Trans-atlantska post-punk IPA", ImageUrl = "" },
                new Beer { Id = 8, Name = "Pšenica", BreweryId = 4, Style = BeerStyle.Wheat, ABV = 4.8, IBU = 12, Description = "Tradicionalno pšenično pivo s notama citrusa", ImageUrl = "" }
            );

            mb.Entity<Venue>().HasData(
                new Venue { Id = 1, Name = "The Beer Garden", Address = "Ulica 1, broj 10", City = "Zagreb", Country = "Hrvatska", Latitude = 45.815, Longitude = 15.982 },
                new Venue { Id = 2, Name = "Craft Beer Pub", Address = "Ilica 25", City = "Zagreb", Country = "Hrvatska", Latitude = 45.816, Longitude = 15.985 },
                new Venue { Id = 3, Name = "Irish Pub Dublin", Address = "O'Connell Street, broj 1", City = "Dublin", Country = "Irska", Latitude = 53.349, Longitude = -6.260 },
                new Venue { Id = 4, Name = "Mali Medo", Address = "Tkalčićeva 36", City = "Zagreb", Country = "Hrvatska", Latitude = 45.815, Longitude = 15.978 },
                new Venue { Id = 5, Name = "Pivnica Pinta", Address = "Ulica grada Vukovara 269", City = "Zagreb", Country = "Hrvatska", Latitude = 45.798, Longitude = 15.989 }
            );

            // ====== Identity role (lab-5): Admin + Member ======
            mb.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "a0000000-0000-0000-0000-000000000001" },
                new IdentityRole<int> { Id = 2, Name = "Member", NormalizedName = "MEMBER", ConcurrencyStamp = "a0000000-0000-0000-0000-000000000002" }
            );

            // ====== Identity korisnici ======
            // Sentinel vrijednost — Program.cs nakon migracije zamjenjuje ovo s pravim
            // Identity hashom za default password "Cugger123!".
            const string seedPasswordHash = "SEED_NEEDS_HASH";

            mb.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = 1, UserName = "pivo_lover", NormalizedUserName = "PIVO_LOVER",
                    Email = "dragan@example.com", NormalizedEmail = "DRAGAN@EXAMPLE.COM", EmailConfirmed = true,
                    FirstName = "Dragan", LastName = "Marić", RegistrationDate = new DateTime(2023, 1, 15),
                    Bio = "Apsolvent pivarstva i ljubitelj kvalitetnih piva",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Dragan+Maric&background=F59E0B&color=fff",
                    PasswordHash = seedPasswordHash,
                    SecurityStamp = "b0000000-0000-0000-0000-000000000001",
                    ConcurrencyStamp = "c0000000-0000-0000-0000-000000000001",
                    LockoutEnabled = true
                },
                new AppUser
                {
                    Id = 2, UserName = "hop_king", NormalizedUserName = "HOP_KING",
                    Email = "marko@example.com", NormalizedEmail = "MARKO@EXAMPLE.COM", EmailConfirmed = true,
                    FirstName = "Marko", LastName = "Horvat", RegistrationDate = new DateTime(2023, 3, 20),
                    Bio = "IPA entuzijast, traži nove craft pivovare",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Marko+Horvat&background=D97706&color=fff",
                    PasswordHash = seedPasswordHash,
                    SecurityStamp = "b0000000-0000-0000-0000-000000000002",
                    ConcurrencyStamp = "c0000000-0000-0000-0000-000000000002",
                    LockoutEnabled = true
                },
                new AppUser
                {
                    Id = 3, UserName = "stout_fan", NormalizedUserName = "STOUT_FAN",
                    Email = "ana@example.com", NormalizedEmail = "ANA@EXAMPLE.COM", EmailConfirmed = true,
                    FirstName = "Ana", LastName = "Novak", RegistrationDate = new DateTime(2023, 6, 10),
                    Bio = "Ljubiteljica tamnih piva i europskih pivovara",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Ana+Novak&background=FCD34D&color=111",
                    PasswordHash = seedPasswordHash,
                    SecurityStamp = "b0000000-0000-0000-0000-000000000003",
                    ConcurrencyStamp = "c0000000-0000-0000-0000-000000000003",
                    LockoutEnabled = true
                },
                new AppUser
                {
                    Id = 4, UserName = "craft_explorer", NormalizedUserName = "CRAFT_EXPLORER",
                    Email = "luka@example.com", NormalizedEmail = "LUKA@EXAMPLE.COM", EmailConfirmed = true,
                    FirstName = "Luka", LastName = "Kovač", RegistrationDate = new DateTime(2024, 2, 1),
                    Bio = "Putujem svijetom u potrazi za savršenim pivom",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Luka+Kovac&background=A16207&color=fff",
                    PasswordHash = seedPasswordHash,
                    SecurityStamp = "b0000000-0000-0000-0000-000000000004",
                    ConcurrencyStamp = "c0000000-0000-0000-0000-000000000004",
                    LockoutEnabled = true
                }
            );

            // pivo_lover je Admin, svi su Member
            mb.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = 1, RoleId = 1 },
                new IdentityUserRole<int> { UserId = 1, RoleId = 2 },
                new IdentityUserRole<int> { UserId = 2, RoleId = 2 },
                new IdentityUserRole<int> { UserId = 3, RoleId = 2 },
                new IdentityUserRole<int> { UserId = 4, RoleId = 2 }
            );

            mb.Entity<CheckIn>().HasData(
                new CheckIn { Id = 1, UserId = 1, BeerId = 1, VenueId = 1, Rating = 4.0, Comment = "Odličan izbor za topli dan", CheckInDate = new DateTime(2024, 3, 15), CreatedAt = new DateTime(2024, 3, 15, 19, 30, 0) },
                new CheckIn { Id = 2, UserId = 1, BeerId = 2, VenueId = 2, Rating = 4.5, Comment = "Sjajna IPA, preporučujem svima", CheckInDate = new DateTime(2024, 3, 16), CreatedAt = new DateTime(2024, 3, 16, 20, 15, 0) },
                new CheckIn { Id = 3, UserId = 2, BeerId = 2, VenueId = 1, Rating = 5.0, Comment = "Savršeno! Najbolja IPA koju sam pio", CheckInDate = new DateTime(2024, 3, 17), CreatedAt = new DateTime(2024, 3, 17, 21, 45, 0) },
                new CheckIn { Id = 4, UserId = 2, BeerId = 5, VenueId = 2, Rating = 4.0, Comment = "Jako hopno, za prave IPA ljubitelje", CheckInDate = new DateTime(2024, 3, 18), CreatedAt = new DateTime(2024, 3, 18, 19, 20, 0) },
                new CheckIn { Id = 5, UserId = 3, BeerId = 3, VenueId = 3, Rating = 5.0, Comment = "Pravi Guinness u Dublinu - nema boljeg!", CheckInDate = new DateTime(2024, 3, 19), CreatedAt = new DateTime(2024, 3, 19, 18, 00, 0) },
                new CheckIn { Id = 6, UserId = 3, BeerId = 1, VenueId = 1, Rating = 3.5, Comment = "Dobro hrvatsko pivo, čvrst izbor", CheckInDate = new DateTime(2024, 3, 20), CreatedAt = new DateTime(2024, 3, 20, 20, 30, 0) },
                new CheckIn { Id = 7, UserId = 1, BeerId = 3, VenueId = 1, Rating = 4.5, Comment = "Klasičan Stout, topla preporuka", CheckInDate = new DateTime(2024, 3, 21), CreatedAt = new DateTime(2024, 3, 21, 19, 00, 0) },
                new CheckIn { Id = 8, UserId = 4, BeerId = 6, VenueId = 4, Rating = 4.5, Comment = "Hrvatski craft je stvarno došao daleko", CheckInDate = new DateTime(2024, 4, 5), CreatedAt = new DateTime(2024, 4, 5, 21, 10, 0) },
                new CheckIn { Id = 9, UserId = 4, BeerId = 7, VenueId = 5, Rating = 4.0, Comment = "Punk attitude u svakom gutljaju", CheckInDate = new DateTime(2024, 4, 12), CreatedAt = new DateTime(2024, 4, 12, 22, 0, 0) },
                new CheckIn { Id = 10, UserId = 2, BeerId = 8, VenueId = 4, Rating = 3.5, Comment = "Osvježavajuće, za ljetni dan", CheckInDate = new DateTime(2024, 4, 20), CreatedAt = new DateTime(2024, 4, 20, 17, 30, 0) }
            );

            mb.Entity<Review>().HasData(
                new Review { Id = 1, UserId = 1, BeerId = 2, Rating = 4.5, Comment = "Odličan balans između gorčine i arome", CreatedAt = new DateTime(2024, 3, 16), Likes = 12 },
                new Review { Id = 2, UserId = 2, BeerId = 2, Rating = 5.0, Comment = "Jedna od najboljih IPA-a koju sam ikad probao", CreatedAt = new DateTime(2024, 3, 17), Likes = 23 },
                new Review { Id = 3, UserId = 3, BeerId = 3, Rating = 5.0, Comment = "Irski stout kakav treba biti", CreatedAt = new DateTime(2024, 3, 19), Likes = 18 },
                new Review { Id = 4, UserId = 4, BeerId = 6, Rating = 4.5, Comment = "Zmajska zna što radi - svaka čast.", CreatedAt = new DateTime(2024, 4, 5), Likes = 9 }
            );

            mb.Entity<Friendship>().HasData(
                new Friendship { Id = 1, FromUserId = 1, ToUserId = 2, CreatedAt = new DateTime(2024, 1, 10) },
                new Friendship { Id = 2, FromUserId = 2, ToUserId = 1, CreatedAt = new DateTime(2024, 1, 10) },
                new Friendship { Id = 3, FromUserId = 1, ToUserId = 3, CreatedAt = new DateTime(2024, 2, 5) },
                new Friendship { Id = 4, FromUserId = 2, ToUserId = 3, CreatedAt = new DateTime(2024, 2, 15) },
                new Friendship { Id = 5, FromUserId = 4, ToUserId = 1, CreatedAt = new DateTime(2024, 3, 1) },
                new Friendship { Id = 6, FromUserId = 1, ToUserId = 4, CreatedAt = new DateTime(2024, 3, 1) }
            );
        }
    }
}
