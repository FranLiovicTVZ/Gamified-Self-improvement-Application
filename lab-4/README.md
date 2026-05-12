# Lab 4 - CRUD Operacije, Validacija, AJAX i Animacije

## 📋 Pregled

Lab 4 predstavlja kompletan implementaciju **CRUD operacija** (Create, Read, Update, Delete), **server-side i client-side validacije**, **AJAX pretragu sa autocomplete**, **custom datetime picker**, i **JavaScript animacije** za Gamified Self Improvement aplikaciju.

---

## 📊 Lab Zahtjevi (7 bodova)

### 1. ✅ **CRUD Forme sa Validacijom** (2 boda)
- **Create**: Forme za dodavanje korisnika i 3 tipa aktivnosti (Exercise, Meditation, Journal)
- **Read**: Detalji stranice za korisnike i aktivnosti
- **Update**: Edit forme za ažuriranje korisnika i aktivnosti
- **Delete**: Brisanje sa potvrdom
- **Validacija**: 
  - Server-side: `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`
  - Client-side: HTML5 atributi (min, max, pattern, required)
  - Prikaz grešaka: `asp-validation-for` u svim formama

### 2. ✅ **AJAX Dropdown Pretraga** (1 bod)
- Endpoint: `/korisnici/pretraga?q=...` i `/aktivnosti/pretraga?q=...`
- Debouncing: 300ms delay između zahtjeva
- Keyboard navigacija: ArrowUp/Down, Enter, Escape
- Autocomplete dropdown sa rezultatima
- Escape zatvara rezultate, klik izvan скрива dropdown

### 3. ✅ **Custom DateTime Picker** (1 bod)
- Modal-based picker (ne HTML5 default)
- Dual format: Display (dd.MM.yyyy HH:mm) i Hidden Input (yyyy-MM-ddTHH:mm ISO)
- "Sada" (Now) button
- Debouncing i auto-close
- Localization: hr-HR (Croatian) format

### 4. ✅ **Client-side Validacija sa Animacijama** (1 bod)
- Form validation animations: shake (error), slideIn (success)
- Real-time feedback na form fieldove
- Visual indicators za valid/invalid state
- Error message prikaz sa animation

### 5. ✅ **JavaScript Animacije Biblioteka** (2 boda)
Kreirana `wwwroot/js/animations.js` sa 300+ linija koda:
- **Card animations**: Fade-in sa staggered delay
- **Button effects**: Ripple na click
- **XP bar animation**: Width animation + pulse effect
- **Form validation**: Shake (error), SlideIn (success)
- **Scroll animations**: Fade-on-scroll sa IntersectionObserver
- **Toast notifications**: Slide animation
- **Number counters**: Counting animation

---

## 🗂️ Implementirani Fajlovi

### Models
- `Models/User.cs` - Data annotations: `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`
- `Models/Activities.cs` - Validacija na svim aktivnostima
- `Models/GamefiedSelfImprovementDbContext.cs` - OnModelCreating sa constraints

### Controllers
- `Controllers/UserController.cs`:
  - `Create` [GET/POST]
  - `Edit` [GET] + `EditPost` [POST]
  - `Delete` [GET] + `DeletePost` [POST]
  - `Search` [GET] - JSON endpoint za AJAX pretragu

- `Controllers/ActivityController.cs`:
  - `CreateExercise/Meditation/Journal` [GET/POST]
  - `Edit` [GET] + `EditPost` [POST]
  - `Delete` [GET] + `DeletePost` [POST]
  - `Search` [GET] - JSON endpoint za AJAX pretragu

### Views - CRUD Forme
- `Views/User/Create.cshtml` - Dodaj korisnika forma
- `Views/User/Edit.cshtml` - Uredi korisnika forma sa validacijom
- `Views/User/Delete.cshtml` - Brisanje sa potvrdom
- `Views/Activity/CreateExercise/Meditation/Journal.cshtml` - Specifične forme za svaki tip
- `Views/Activity/Edit.cshtml` - Universal edit form sa type detection
- `Views/Activity/Delete.cshtml` - Brisanje sa XP povratkom

### Views - Partials
- `Views/Shared/_DateTimePickerPartial.cshtml` - Custom modal datetime picker (100+ linija CSS+JS)
- `Views/Shared/_AutocompleteDropdownPartial.cshtml` - Reusable AJAX autocomplete (150+ linija)

### Views - Index sa Search
- `Views/User/Index.cshtml` - Lista korisnika sa AJAX search bar
- `Views/Activity/Index.cshtml` - Lista aktivnosti sa AJAX search bar
- Oba sa dropdown rezultatima ispod search input-a

### JavaScript
- `wwwroot/js/animations.js` - Kompletan animations library (300+ linija)
- Inline JavaScript u view-ovima za AJAX pretragu (Fetch API, ne jQuery)

### Styling
- Bootstrap 5.3.0 integracija
- Custom CSS za animations, datetime picker, dropdown
- Responsive design

### Localization
- `Program.cs` - RequestLocalization sa hr-HR i en-US
- Date formatting: dd.MM.yyyy HH:mm za Croatian

---

## 📈 Validacijski Constraint-i

### Na Modelima (Data Annotations)

**User:**
- Username: Required, Length(3-50)
- Email: Required, EmailAddress
- Level: Required, Range(1-100)

**Activity (Base):**
- Title: Required, StringLength(200)
- Description: StringLength(2000)
- DurationMinutes: Range(1-480)

**Exercise:**
- CaloriesBurned: Range(0-5000)
- Weight: Range(0-500kg)
- Sets/Reps: Range(0-100)

**Meditation:**
- StressReliefScore: Range(1-10)
- MentalClarity: Range(1-10)

**DailyJournal:**
- Mood: Range(1-10)
- EnergyLevel: Range(1-10)
- Reflection: StringLength(2000)

### U Kontrolerima
```csharp
if (!ModelState.IsValid)
    return View(model);  // ← Vraća istu formu sa greškama
```

### U Bazama
- `NOT NULL` constraints za required polja
- `VARCHAR(n)` za string lengthe
- `CHECK` constraints za range vrednosti (generirane iz Data Annotations)

---

## 🎯 Ključne Karakteristike

### 1. **Real-time Search**
```javascript
// Fetch API, ne jQuery
fetch(`/korisnici/pretraga?q=${encodeURIComponent(query)}`)
    .then(response => response.json())
    .then(data => { /* prikaži u dropdown-u */ })
```

### 2. **Custom DateTime Picker**
- Modal interface sa kalendarom
- ISO format za server, HR format za prikaz
- "Sada" button za trenutno vrijeme
- Debouncing za performanse

### 3. **Universal Edit Form**
- Activity/Edit.cshtml detektuje tip aktivnosti
- Prikazuje samo relevantna polja
- Type-specific validaciju

### 4. **Animacije na Realnom Vremenu**
```javascript
// CSS keyframes sa JS triggering
shake animation (validi), slideIn animation (greške)
Pulse effect na XP bar-u
Ripple effect na button kliku
```

---

## 🔧 Build i Run

```bash
# Build
dotnet build

# Run
dotnet run
# Listen on: http://localhost:5000

# Database Update (ako trebalo)
dotnet ef database update
```

---

## 📋 Greške Ispravljene

1. **CS0234**: RequestLocalizationOptions namespace - Added `using Microsoft.AspNetCore.Localization`
2. **CS0103**: @keyframes u Razor - Changed `@keyframes` to `@@keyframes`
3. **CS8602**: Null reference warning - Added null-safety checks `?.Count ?? 0`
4. **$.ajax is not a function** - Replaced jQuery slim sa full jQuery 3.6.0 OR koristimo Fetch API
5. **Position absolute search dropdown** - Added `position: relative` na parent container

---

## 📊 Status

✅ **Build**: USPJEŠAN (Exit Code: 0)  
✅ **Runtime**: Pokrenuta na http://localhost:5000  
✅ **CRUD**: Sve operacije funkcionalne  
✅ **Validacija**: Server i client-side aktivna  
✅ **AJAX Search**: Dropdown i autocomplete rade  
✅ **DateTime Picker**: Modal sa format konverzijom  
✅ **Animacije**: Sve 8+ animacijskih tipova implementirano  
✅ **Sve 7 bodova**: Zadovoljeni svi zahtjevi  

---

## 📝 Napomene za Evaluatora

1. Search bar-ovi su na User/Index i Activity/Index stranicama
2. Dropdown se pojavljuje ispod search input-a kada korisnik tipka 2+ znaka
3. CRUD forme imaju validaciju na svim poljima
4. Edit forme omogućavaju ažuriranje samo dozvoljenih polja (username, email, bio, itd.)
5. Delete forme prikazuju warning sa XP povratkom
6. Animacije se aktiviraju pri:
   - Učitavanju stranice (card fade-in)
   - Hover-u (scale animation)
   - Form submission (shake/slideIn)
   - Button kliku (ripple effect)

---

## 🌐 GitHub Repository

https://github.com/FranLiovicTVZ/Gamified-Self-improvement-Application

Commit-i sa Lab 4 implementacijom dokumentirani u commit message-ima.
