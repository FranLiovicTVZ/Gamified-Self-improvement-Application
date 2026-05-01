# Sitemap - URL usmjeravanje aplikacije

## Home / Dashboard URLs

| URL | Controller | Akcija | View | Opis |
|-----|-----------|--------|------|------|
| `/` | HomeController | Dashboard | Dashboard.cshtml | Početna stranica - sveukupan pregled |
| `/home` | HomeController | Dashboard | Dashboard.cshtml | Početna stranica - alternativni URL |
| `/dashboard` | HomeController | Dashboard | Dashboard.cshtml | Dashboard stranica |

---

## Aktivnosti URLs

| URL | Controller | Akcija | View | Opis |
|-----|-----------|--------|------|------|
| `/aktivnosti` | ActivityController | Index | Activity/Index.cshtml | Lista svih aktivnosti |
| `/aktivnosti/{id}` | ActivityController | Details | Activity/Details.cshtml | Detalji o specifičnoj aktivnosti |
| `/aktivnosti/po-korisniku/{userId}` | ActivityController | Index | Activity/Index.cshtml | Aktivnosti specifičnog korisnika |

### Parametri

- **{id}** - Jedinstveni identifikator aktivnosti (int)
- **{userId}** - Jedinstveni identifikator korisnika (int)

### Primjeri

- `/aktivnosti` - Prikazuje sve aktivnosti
- `/aktivnosti/5` - Prikazuje detalje aktivnosti s ID-om 5
- `/aktivnosti/po-korisniku/2` - Prikazuje sve aktivnosti korisnika s ID-om 2

---

## Korisnici URLs

| URL | Controller | Akcija | View | Opis |
|-----|-----------|--------|------|------|
| `/korisnici` | UserController | Index | User/Index.cshtml | Lista svih korisnika |
| `/korisnici/{id}` | UserController | Details | User/Details.cshtml | Detalji o specifičnom korisniku |
| `/profil/{id}` | UserController | Details | User/Details.cshtml | Alternativni URL za profil korisnika |

### Parametri

- **{id}** - Jedinstveni identifikator korisnika (int)

### Primjeri

- `/korisnici` - Prikazuje sve korisnike
- `/korisnici/1` - Prikazuje detalje korisnika s ID-om 1
- `/profil/1` - Prikazuje profil korisnika s ID-om 1 (alternativni URL)

---

## Custom Routing - Atributi korišteni

### HomeController
```csharp
[Route("")]
[Route("home")]
public class HomeController : Controller
{
    [Route("")]
    [Route("dashboard")]
    public IActionResult Dashboard() { ... }
}
```

### ActivityController
```csharp
[Route("aktivnosti")]
public class ActivityController : Controller
{
    [Route("")]
    [Route("po-korisniku/{userId:int}")]
    public IActionResult Index(int? userId = null) { ... }
    
    [Route("{id:int}")]
    public IActionResult Details(int id) { ... }
}
```

### UserController
```csharp
[Route("korisnici")]
public class UserController : Controller
{
    [Route("")]
    public IActionResult Index() { ... }
    
    [Route("{id:int}")]
    [Route("/profil/{id:int}")]
    public IActionResult Details(int id) { ... }
}
```

---

## Routing konstrikcije

### Route Constraints korišteni

- `{id:int}` - Parametar mora biti cijeli broj
- `{userId:int}` - Parametar mora biti cijeli broj

### Primjer mapiranja

- URL `/aktivnosti/po-korisniku/3` aktivira `ActivityController.Index(userId: 3)`
- URL `/profil/1` aktivira `UserController.Details(id: 1)`

---

## Napomene

1. **Default Routing** je zadržan u Program.cs:
   ```csharp
   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Dashboard}/{id?}");
   ```

2. **Atribut Routing** ima prioritet ispred default routinga, pa će se prvo pokušati upotreba custom ruta definirane na controllerima.

3. **Svi URL-i su case-insensitive** - `/Aktivnosti`, `/AKTIVNOSTI` i `/aktivnosti` daju isti rezultat.

4. **Parametri kao query stringi** su također dostupni:
   - `/aktivnosti?userId=2` - List aktivnosti korisnika 2
   - `/korisnici?id=1` - Detaljne informacije korisnika 1

---

## Summary - 6+ Custom Ruta

1. `/` → HomeController.Dashboard
2. `/home` → HomeController.Dashboard
3. `/dashboard` → HomeController.Dashboard
4. `/aktivnosti` → ActivityController.Index
5. `/aktivnosti/{id}` → ActivityController.Details
6. `/aktivnosti/po-korisniku/{userId}` → ActivityController.Index
7. `/korisnici` → UserController.Index
8. `/korisnici/{id}` → UserController.Details
9. `/profil/{id}` → UserController.Details

**Ukupno: 9 custom ruta implementirano**
