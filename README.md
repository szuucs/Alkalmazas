# Alkalmazas

ASP.NET Core Web API (MySQL, Entity Framework Core) és egy WPF kliens (CicalanyProjektWPF).

Részletes, kezdőknek szóló angol útmutató: [How the whole project works](PROJECT_GUIDE.md).

## Végpontok

Mindhárom erőforrásnál van teljes CRUD (GET lista, GET egy elem, POST, PUT, DELETE) és saját függvény.

| Erőforrás | Alap CRUD | Saját függvények |
|-----------|-----------|------------------|
| `api/users` | GET, GET `{id}`, POST (regisztráció), PUT `{id}`, DELETE `{id}` | POST `login` (JSON: `name`, `password`) |
| `api/catgirls` | GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}` | GET `search?name=&minAge=&maxAge=&minHeight=&maxHeight=`, GET `stats` |
| `api/orders` | GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}` | GET `user/{userId}` |

Példa kérések: `Alkalmazas/Alkalmazas.http`.

## WPF bejelentkezés és regisztráció

A WPF kliens a `http://localhost:5077/` API-címet használja. A regisztráció
`POST api/users`, a bejelentkezés `POST api/users/login` JSON-kérést küld;
a felhasználókat az API MySQL-ben tárolja. Nincs helyi tesztfiók vagy offline belépés.

A MySQL elindítása után indítsd az API-t a `http` profillal, majd a WPF klienst Windowson:

```sh
dotnet run --project Alkalmazas --launch-profile http
dotnet run --project CicalanyProjektWPF
```

Az API címe a `CicalanyProjektWPF/Services/ApiAuthService.cs` fájlban állítható.
A katalógus, kosár és rendelések jelenleg továbbra is helyi, memóriában tárolt adatok.

## Adatbázis

A modell változásakor új migráció kell (Package Manager Console, Default project: `Alkalmazas`):

```
Add-Migration ApiBovites
Update-Database
```
