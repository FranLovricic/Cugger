# Cugger — Semantic DB Model

> Sažeti popis tablica (modela), glavnih svojstava i veza unutar Cugger aplikacije.
> Generirano za Lab 3.

## Pregled

Cugger je clone Untappd-a — društvena platforma za rate-anje piva i check-iniranje na lokalima.
Objektni model se sastoji od **7 entiteta** (i 1 enuma) koji su preslikani u tablice kroz Entity Framework Core.

Glavni DbContext: `Cugger.Data.CuggerDbContext` ([Data/CuggerDbContext.cs](../Data/CuggerDbContext.cs))

| Set u DbContextu  | Entitet      | Tablica       |
|-------------------|--------------|---------------|
| `Breweries`       | `Brewery`    | `Breweries`   |
| `Beers`           | `Beer`       | `Beers`       |
| `Users`           | `User`       | `Users`       |
| `Venues`          | `Venue`      | `Venues`      |
| `CheckIns`        | `CheckIn`    | `CheckIns`    |
| `Reviews`         | `Review`     | `Reviews`     |
| `Friendships`     | `Friendship` | `Friendships` |

Vlastiti enum: `BeerStyle` (Lager, Pilsner, IPA, Stout, Porter, Ale, Wheat, Sour, Cider, Other)

---

## Brewery — pivovara

`Models/Brewery.cs`

| Svojstvo       | Tip      | Anotacije                          |
|----------------|----------|------------------------------------|
| **Id** (PK)    | `int`    | `[Key]`                            |
| Name           | `string` | `[Required] [StringLength(150)]`   |
| Country        | `string` | `[Required] [StringLength(100)]`   |
| City           | `string` | `[Required] [StringLength(100)]`   |
| FoundedYear    | `int`    |                                    |
| Description    | `string` | `[StringLength(2000)]`             |
| WebsiteUrl     | `string` | `[StringLength(300)]`              |
| LogoUrl        | `string` | `[StringLength(500)]`              |
| Beers          | `ICollection<Beer>` | navigacija (1-N)        |

**Veze:**
- `Brewery 1 — N Beer`

---

## Beer — pivo

`Models/Beer.cs`

| Svojstvo       | Tip          | Anotacije                                 |
|----------------|--------------|-------------------------------------------|
| **Id** (PK)    | `int`        | `[Key]`                                   |
| Name           | `string`     | `[Required] [StringLength(150)]`          |
| Style          | `BeerStyle`  | (vlastiti enum)                           |
| ABV            | `double`     | `[Column(TypeName="decimal(4,2)")]`       |
| IBU            | `int`        |                                           |
| Description    | `string`     | `[StringLength(2000)]`                    |
| ImageUrl       | `string`     | `[StringLength(500)]`                     |
| **BreweryId** (FK) | `int`    | `[ForeignKey(nameof(Brewery))]`           |
| Brewery        | `Brewery?`   | navigacija (N-1)                          |
| CheckIns       | `ICollection<CheckIn>` | navigacija (1-N)                |
| Reviews        | `ICollection<Review>`  | navigacija (1-N)                |
| RatingCount    | `int`        | `[NotMapped]` (calc property)             |
| AverageRating  | `double`     | `[NotMapped]` (calc property)             |

**Veze:**
- `Beer N — 1 Brewery`
- `Beer 1 — N CheckIn`
- `Beer 1 — N Review`

---

## User — korisnik

`Models/User.cs`

| Svojstvo            | Tip      | Anotacije                                 |
|---------------------|----------|-------------------------------------------|
| **Id** (PK)         | `int`    | `[Key]`                                   |
| Username            | `string` | `[Required] [StringLength(60)]` (unique) |
| Email               | `string` | `[Required] [EmailAddress] [StringLength(200)]` (unique) |
| FirstName           | `string` | `[Required] [StringLength(80)]`           |
| LastName            | `string` | `[Required] [StringLength(80)]`           |
| RegistrationDate    | `DateTime` | (DateTime svojstvo)                     |
| Bio                 | `string` | `[StringLength(500)]`                     |
| AvatarUrl           | `string` | `[StringLength(500)]`                     |
| CheckIns            | `ICollection<CheckIn>`   | navigacija (1-N)                  |
| Reviews             | `ICollection<Review>`    | navigacija (1-N)                  |
| FromFriendships     | `ICollection<Friendship>`| N-N strana A                      |
| ToFriendships       | `ICollection<Friendship>`| N-N strana B                      |

**Veze:**
- `User 1 — N CheckIn`
- `User 1 — N Review`
- `User N — N User` preko `Friendship` (N-N relacija)

Indexi: `Username` UNIQUE, `Email` UNIQUE

---

## Venue — lokal

`Models/Venue.cs`

| Svojstvo       | Tip      | Anotacije                          |
|----------------|----------|------------------------------------|
| **Id** (PK)    | `int`    | `[Key]`                            |
| Name           | `string` | `[Required] [StringLength(150)]`   |
| Address        | `string` | `[Required] [StringLength(250)]`   |
| City           | `string` | `[Required] [StringLength(100)]`   |
| Country        | `string` | `[Required] [StringLength(100)]`   |
| Latitude       | `double` | `[Column(TypeName="decimal(9,6)")]`|
| Longitude      | `double` | `[Column(TypeName="decimal(9,6)")]`|
| CheckIns       | `ICollection<CheckIn>` | navigacija (1-N)        |

**Veze:**
- `Venue 1 — N CheckIn`

---

## CheckIn — zapis o konzumaciji

`Models/CheckIn.cs`

| Svojstvo        | Tip        | Anotacije                                      |
|-----------------|------------|------------------------------------------------|
| **Id** (PK)     | `int`      | `[Key]`                                        |
| **UserId** (FK) | `int`      | `[ForeignKey(nameof(User))]`                   |
| **BeerId** (FK) | `int`      | `[ForeignKey(nameof(Beer))]`                   |
| **VenueId** (FK)| `int`      | `[ForeignKey(nameof(Venue))]`                  |
| Rating          | `double`   | `[Range(0,5)] [Column(TypeName="decimal(3,2)")]` |
| Comment         | `string`   | `[StringLength(1000)]`                         |
| CheckInDate     | `DateTime` | (DateTime svojstvo)                            |
| CreatedAt       | `DateTime` | (DateTime svojstvo)                            |
| User            | `User?`    | navigacija                                     |
| Beer            | `Beer?`    | navigacija                                     |
| Venue           | `Venue?`   | navigacija                                     |

**Veze:**
- `CheckIn N — 1 User`
- `CheckIn N — 1 Beer`
- `CheckIn N — 1 Venue`

> Bilješka: sve N-1 veze za CheckIn imaju `OnDelete(DeleteBehavior.Restrict)` da se izbjegnu cascade-cycle problemi u SQL Serveru.

---

## Review — recenzija piva

`Models/Review.cs`

| Svojstvo        | Tip        | Anotacije                                      |
|-----------------|------------|------------------------------------------------|
| **Id** (PK)     | `int`      | `[Key]`                                        |
| **UserId** (FK) | `int`      | `[ForeignKey(nameof(User))]`                   |
| **BeerId** (FK) | `int`      | `[ForeignKey(nameof(Beer))]`                   |
| Rating          | `double`   | `[Range(0,5)] [Column(TypeName="decimal(3,2)")]` |
| Comment         | `string`   | `[StringLength(2000)]`                         |
| CreatedAt       | `DateTime` | (DateTime svojstvo)                            |
| Likes           | `int`      |                                                |
| User            | `User?`    | navigacija                                     |
| Beer            | `Beer?`    | navigacija                                     |

**Veze:**
- `Review N — 1 User`
- `Review N — 1 Beer`

---

## Friendship — N-N veza prijateljstvo

`Models/Friendship.cs`

| Svojstvo            | Tip        | Anotacije                                |
|---------------------|------------|------------------------------------------|
| **Id** (PK)         | `int`      | `[Key]`                                  |
| **FromUserId** (FK) | `int`      | `[ForeignKey(nameof(FromUser))]`         |
| **ToUserId** (FK)   | `int`      | `[ForeignKey(nameof(ToUser))]`           |
| CreatedAt           | `DateTime` | (DateTime svojstvo)                      |
| FromUser            | `User?`    | navigacija                               |
| ToUser              | `User?`    | navigacija                               |

**Veze:**
- Implementira N-N između dva `User`-a kroz međutablicu sa Idjem.

---

## Konceptualni dijagram

```
                        ┌──────────────┐
                        │   Brewery    │
                        └──────┬───────┘
                               │ 1
                               │
                               │ N
                        ┌──────┴───────┐
                        │     Beer     │◄────────────┐
                        └──────┬───────┘             │
                               │ 1                   │ 1
                               │                     │
                               │ N                   │ N
                        ┌──────┴───────┐    ┌────────┴───┐
                        │   CheckIn    │    │   Review   │
                        └──────┬───────┘    └────────┬───┘
                               │ N                   │ N
                               │                     │
                               │ 1                   │ 1
                        ┌──────┴───────┐    ┌────────┴───┐
                        │     Venue    │    │    User    │
                        └──────────────┘    └────────┬───┘
                                                     │
                                              ┌──────┘
                                              │ User-User
                                              │ (N-N kroz Friendship)
                                              ▼
                                        ┌──────────────┐
                                        │  Friendship  │
                                        └──────────────┘
```

## Repozitoriji (DAL)

Pristup podacima ide kroz EF-bazirane repozitorije injectane preko DI:

- [BeerRepository](../Repositories/BeerRepository.cs) — `GetAll`, `GetById`, `GetByBrewery`, `GetByStyle`, `Search`, `GetTopRated`, `GetAverageRating`, `GetRatingCount`
- [BreweryRepository](../Repositories/BreweryRepository.cs) — `GetAll`, `GetById`, `GetByCountry`
- [UserRepository](../Repositories/UserRepository.cs) — `GetAll`, `GetById`, `GetByUsername`, `GetFriends`, `GetMostActive`, ...
- [VenueRepository](../Repositories/VenueRepository.cs) — `GetAll`, `GetById`, `GetByCity`
- [CheckInRepository](../Repositories/CheckInRepository.cs) — `GetAll`, `GetById`, `GetByUser`, `GetByBeer`, `GetByVenue`, `GetRecent`
- [ReviewRepository](../Repositories/ReviewRepository.cs) — `GetAll`, `GetById`, `GetByBeer`, `GetByUser`, `GetTopLiked`
- [FriendshipRepository](../Repositories/FriendshipRepository.cs) — `GetAll`, `GetById`

Svi su registrirani kao `Scoped` u `Program.cs`.
