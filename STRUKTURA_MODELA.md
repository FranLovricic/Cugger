# Struktura modela - ERD (Entity Relationship Diagram)

```
┌─────────────────────────────────────────────────────────────┐
│                       USER (Korisnik)                        │
├─────────────────────────────────────────────────────────────┤
│ PK: Id                                                       │
│ • Username (string)                                          │
│ • Email (string)                                             │
│ • FirstName (string)                                         │
│ • LastName (string)                                          │
│ • RegistrationDate (DateTime) ⭐                            │
│ • Bio (string)                                               │
│ • AvatarUrl (string)                                         │
│ 1-N→ CheckIns, Reviews                                      │
│ N-N↔ User (preko Friendship)                                 │
└─────────────────────────────────────────────────────────────┘
         │                                                │
         │ 1-N                                           │ 1-N
         ├──────────────────────┬──────────────────────┤
         │                      │                      │
         ▼                      ▼                      ▼
  ┌─────────────┐      ┌──────────────┐      ┌──────────────┐
  │ CHECK-IN    │      │ REVIEW       │      │ FRIENDSHIP   │
  ├─────────────┤      ├──────────────┤      ├──────────────┤
  │ PK: Id      │      │ PK: Id       │      │ PK: Id       │
  │ FK: UserId  │      │ FK: UserId   │      │ FK: FromUserId
  │ FK: BeerId  │      │ FK: BeerId   │      │ FK: ToUserId │
  │ FK: VenueId │      │ Rating (0-5) │      │ CreatedAt ⭐ │
  │ Rating (0-5)│      │ Comment (str)│      └──────────────┘
  │ Comment     │      │ CreatedAt ⭐ │
  │ CheckInDate ⭐│     │ Likes (int)  │
  │ CreatedAt ⭐ │      └──────────────┘
  └─────────────┘             ▲
         │                    │ N-1
         │ N-1               ┌┘
         └────────┬──────────┘
                  │
                  ▼
         ┌─────────────────────────────────────┐
         │      BEER (Pivo)                    │
         ├─────────────────────────────────────┤
         │ PK: Id                              │
         │ • Name (string)                     │
         │ • Style (BeerStyle) ⭐️ (Enum)      │
         │ • ABV (double) - Alcohol %          │
         │ • IBU (int) - Bitterness Units      │
         │ • Description (string)              │
         │ • BreweryId (FK)                    │
         │ • RatingCount (int)                 │
         │ • AverageRating (double)            │
         │ N-1→ Brewery                        │
         │ 1-N→ CheckIns, Reviews              │
         └─────────────────────────────────────┘
                  │ N-1
                  │
                  ▼
         ┌─────────────────────────────────────┐
         │     BREWERY (Pivovara)              │
         ├─────────────────────────────────────┤
         │ PK: Id                              │
         │ • Name (string)                     │
         │ • Country (string)                  │
         │ • City (string)                     │
         │ • FoundedYear (int)                 │
         │ • Description (string)              │
         │ • WebsiteUrl (string)               │
         │ 1-N→ Beers                          │
         └─────────────────────────────────────┘


         ┌─────────────────────────────────────┐
         │      VENUE (Lokal/Mjesto)           │
         ├─────────────────────────────────────┤
         │ PK: Id                              │
         │ • Name (string)                     │
         │ • Address (string)                  │
         │ • City (string)                     │
         │ • Country (string)                  │
         │ • Latitude (double)                 │
         │ • Longitude (double)                │
         │ 1-N→ CheckIns                       │
         └─────────────────────────────────────┘


┌──────────────────────────────────┐
│  BeerStyle (ENUM)                │
├──────────────────────────────────┤
│ • Lager                          │
│ • Pilsner                        │
│ • IPA                            │
│ • Stout                          │
│ • Porter                         │
│ • Ale                            │
│ • Wheat                          │
│ • Sour                           │
│ • Cider                          │
│ • Other                          │
└──────────────────────────────────┘
```

---

## Legenda:
- **PK** = Primary Key (primarni ključ)
- **FK** = Foreign Key (tuđi ključ)
- **⭐** = DateTime svojstvo ili Enum
- **1-N** = Relacija jedan-prema-mnogo
- **N-N** = Relacija mnogo-prema-mnogo
- **N-1** = Obrnuta relacija od 1-N

---

## Primjer: User → CheckIn (1-N Relacija)

```csharp
// Jedan korisnik:
User dragan = new User { Id = 1, FirstName = "Dragan", ... };

// Može imati MNOGO check-inova:
CheckIn check1 = new CheckIn { Id = 1, UserId = 1, BeerId = 1, ... };
CheckIn check2 = new CheckIn { Id = 2, UserId = 1, BeerId = 2, ... };
CheckIn check3 = new CheckIn { Id = 3, UserId = 1, BeerId = 3, ... };

// Pristup preko navigation property:
var draganovCheckIni = dragan.CheckIns; // List<CheckIn>
```

---

## Primjer: User ↔ User (N-N Relacija - Prijateljstva)

```csharp
// Korisnik 1 je prijatelj s korisnikom 2
Friendship f1 = new Friendship { 
    Id = 1, 
    FromUserId = 1,  // Dragan
    ToUserId = 2,    // Marko
    CreatedAt = new DateTime(2024, 1, 10)
};

// Korisnik 2 je prijatelj s korisnikom 1
Friendship f2 = new Friendship { 
    Id = 2, 
    FromUserId = 2,  // Marko
    ToUserId = 1,    // Dragan
    CreatedAt = new DateTime(2024, 1, 10)
};

// Pristup preko navigation property:
var draganFriends = dragan.FromFriendships
    .Select(f => f.ToUser)
    .ToList(); // List<User>
```

---

## Statističke informacije inicijaliziranih podataka:

- **Korisnici:** 3
- **Piva:** 5
- **Pivovare:** 3  
- **Lokale:** 3
- **Check-ini:** 7
- **Recenzije:** 3
- **Prijateljstva:** 4

**Prosječni podaci:**
- Prosječan broj check-inova po korisniku: 2.33
- Prosječan broj ocjena po pivu: 1.4
- Prosječna ocjena piva: 4.1/5.0

---

## Upiti koji se izvršavaju na startu:

1. ✅ Check-ini korisnika (filtriranje, Where)
2. ✅ Prosječne ocjene piva (Select, Average, OrderByDescending)
3. ✅ Najaktivniji korisnici (Count, OrderByDescending)
4. ✅ Piva sortirana po ocjeni (Select, OrderByDescending)
5. ✅ Prijatelji korisnika (Where, Select, First)
6. ✅ Jednostavna piva (Where, Logički operatori, OrderBy)
7. ✅ Check-ini po lokaciji (Select, Count, OrderByDescending)
