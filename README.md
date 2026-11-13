# Alkalmazas

ASP.NET Core Web API (MySQL, Entity Framework Core) és egy WPF kliens (CicalanyProjektWPF).

Teljesen kezdőknek szóló, angol nyelvű magyarázat az indítástól a kód működéséig: [How this project works](PROJECT_GUIDE.md).

## Végpontok

Mindhárom erőforrásnál van teljes CRUD (GET lista, GET egy elem, POST, PUT, DELETE) és saját függvény.

| Erőforrás | Alap CRUD | Saját függvények |
|-----------|-----------|------------------|
| `api/users` | GET, GET `{id}`, POST (regisztráció), PUT `{id}`, DELETE `{id}` | POST `login` (JSON: `name`, `password`) |
| `api/catgirls` | GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}` | GET `search?name=&minAge=&maxAge=&minHeight=&maxHeight=`, GET `stats` |
| `api/orders` | GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}` | GET `user/{userId}` |

A hirdetés feladásakor a WPF `POST api/catgirls/with-image` kérést küld: a multipart űrlap `details` mezője a JSON-adatokat, a kötelező `image` mező a fényképet tartalmazza (kép nélkül 400-as hibát kap).

Minden cicálány ára alapból 20 000 Ft, és 2 órát vállal (`Models/CatGirl.cs`: `DefaultPrice`, `DefaultDurationHours`). A rendelés `orderDate` mezője a foglalás kezdő időpontja (dátum és óra), pl. `2026-12-01T18:00:00`; a rendeléshez a megrendelő neve, e-mail címe és telefonszáma kötelező (`customerName`, `customerEmail`, `customerPhone`); a WPF a rendelés előtt kiírja az idősávot (pl. `18-20` vagy `13:31-15:31`) és azt, hogy a fizetés a helyszínen történik.

Példa kérések: `Alkalmazas/Alkalmazas.http`.

## Indítás

A WPF kliens a `http://localhost:5077/` API-címet használja. A regisztráció
`POST api/users`, a bejelentkezés `POST api/users/login` JSON-kérést küld;
a felhasználókat az API MySQL-ben tárolja. Nincs helyi tesztfiók vagy offline belépés.
A katalógus, az új hirdetések és a rendelések szintén az API-t használják, és az adatbázisban maradnak az alkalmazás bezárása után is.

.NET 10 SDK és futó Docker Desktop szükséges. A projekt gyökérmappájából indítsd a MySQL-t, majd az API-t:

```sh
docker compose up -d
dotnet run --project Alkalmazas --launch-profile http
```

Az API induláskor alkalmazza a meglévő migrációkat. Üres katalógus esetén létrehozza a nyolc kezdő cicalányt a képeikkel együtt.
Amikor megjelent a `http://localhost:5077` cím, egy másik terminálból indítsd a WPF klienst Windowson:

```sh
dotnet run --project CicalanyProjektWPF
```

Az API címe a `CicalanyProjektWPF/Services/ApiAuthService.cs` fájlban állítható.
Csak a még le nem adott kosár marad a WPF alkalmazás memóriájában. Sikeres rendelés után a leadott tételek kikerülnek belőle; hiba esetén megmaradnak.
Az új cicalány űrlapján a „Kép kiválasztása” gombbal JPG vagy PNG fénykép megadása kötelező.
A kép előnézete az űrlapon, mentés után pedig a katalógusban látható.
Az API a képfájlt az `Alkalmazas/App_Data/Images` mappába,
az elérési útját pedig az adatbázisba menti. A WPF a képeket HTTP-n keresztül tölti le.
Biztonsági másolathoz az adatbázis mellett ezt a képmappát is meg kell őrizni.
A megengedett magasság 0–300 cm, a súly 0–500 kg (a határértékeket is beleértve).

## Adatbázis

A létrehozás paraméterei a `docker-compose.yml` fájlban szerepelnek, a kapcsolódási karakterlánc az `Alkalmazas/appsettings.json` fájlban található:

| Paraméter | Érték |
|-----------|-------|
| Adatbáziskezelő | MySQL 8.0 |
| Szerver | localhost |
| Port | 3309 (konténeren belül 3306) |
| Adatbázis | Alkalmazas |
| Felhasználó | root |
| Helyi fejlesztői jelszó | jelszo |
| Adattárolás | Docker `db_data` kötet |

A már mellékelt migrációkat az API automatikusan futtatja. Kézzel is alkalmazhatók:

```sh
dotnet ef database update --project Alkalmazas
```

## Ellenőrzés

A futó API-val a WPF adatszolgáltatását is használó integrációs ellenőrzés indítható:

```sh
dotnet run --project tests/ApiChecks -- http://localhost:5077/
```

Az ellenőrzés saját felhasználókat, hirdetéseket és rendelést hoz létre, majd törli ezeket a tesztrekordokat.
