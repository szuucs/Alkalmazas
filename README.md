# Alkalmazas

ASP.NET Core Web API (MySQL, Entity Framework Core) és egy WPF kliens (CicalanyProjektWPF).

## Végpontok

Mindhárom erőforrásnál van teljes CRUD (GET lista, GET egy elem, POST, PUT, DELETE) és saját függvény.

| Erőforrás | Alap CRUD | Saját függvények |
|-----------|-----------|------------------|
| `api/users` | GET, GET `{id}`, POST (regisztráció), PUT `{id}`, DELETE `{id}` | GET `login?name=&password=` |
| `api/catgirls` | GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}` | GET `search?name=&minAge=&maxAge=&minHeight=&maxHeight=`, GET `stats` |
| `api/orders` | GET, GET `{id}`, POST, PUT `{id}`, DELETE `{id}` | GET `user/{userId}` |

Példa kérések: `Alkalmazas/Alkalmazas.http`.

## Adatbázis

A modell változásakor új migráció kell (Package Manager Console, Default project: `Alkalmazas`):

```
Add-Migration ApiBovites
Update-Database
```
