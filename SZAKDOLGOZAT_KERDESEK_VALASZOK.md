# Brewed - E-Commerce Kávéshop Platform
## Szakdolgozat Védés - Kérdések és Válaszok

---

## 1. ÁLTALÁNOS KÉRDÉSEK

### Miről szól a szakdolgozatod?
A szakdolgozatom egy teljes körű e-commerce webáruház platformot mutat be, amely kifejezetten kávé termékek értékesítésére specializálódott. A rendszer támogatja a regisztrált felhasználókat és vendég vásárlókat egyaránt, komplett webshop funkcionalitást biztosítva: termék böngészés, kosár kezelés, rendelés leadás, kupon rendszer, értékelések és számlázás.

---

## 2. TECHNOLÓGIAI KÉRDÉSEK

### Milyen programozási nyelveket használtál és miért?

**Backend: C# (.NET 9.0)**
- **Miért választottam:**
  - Erősen típusos nyelv, amely segít elkerülni a futásidejű hibákat
  - Kiváló enterprise támogatás és széles körű library ökoszisztéma
  - Entity Framework Core kiváló ORM képességei
  - Aszinkron programozás beépített támogatása (async/await)
  - Platform-független a .NET Core-nak köszönhetően
  - Személyes tapasztalat és a C# modern nyelvi tulajdonságai (LINQ, pattern matching, nullable reference types)

**Frontend: TypeScript**
- **Miért választottam:**
  - Típusbiztonság JavaScript-ben, amely csökkenti a hibák számát
  - Jobb IDE támogatás (IntelliSense, refactoring)
  - Könnyebb karbantarthatóság nagy projekteknél
  - A React ökoszisztéma kiválóan támogatja
  - Interfészekkel tisztán definiálhatók az API szerződések

### Milyen keretrendszereket használtál?

**Backend keretrendszerek:**

1. **ASP.NET Core 9.0**
   - Modern, cross-platform web framework
   - Beépített dependency injection
   - Middleware-based request pipeline
   - Kiváló teljesítmény és skálázhatóság

2. **Entity Framework Core 9.0.9**
   - ORM (Object-Relational Mapping) megoldás
   - Code-First megközelítés az adatbázis kezelésére
   - LINQ támogatás típusbiztos lekérdezésekhez
   - Migration-based séma verziókezelés
   - Automatikus kapcsolat kezelés és tranzakciók

3. **AutoMapper 12.0.1**
   - Object-to-object mapping könyvtár
   - Egyszerűsíti az Entity és DTO közötti konverziót
   - Csökkenti a boilerplate kódot
   - Központi mapping konfigurációval

4. **JWT Bearer Authentication**
   - Stateless autentikáció
   - Token-based megközelítés
   - Jól skálázható több szerver esetén is
   - 30 napos token lejárati idő

5. **QuestPDF 2025.7.4**
   - Modern PDF generálás C#-ban
   - Fluent API számla készítéshez
   - Egyszerű használat, jó teljesítmény

**Frontend keretrendszerek:**

1. **React 18.3.1**
   - **Miért választottam:**
     - Komponens-alapú architektúra, újrafelhasználható UI elemek
     - Virtual DOM a jobb teljesítményért
     - Nagy közösség, rengeteg third-party library
     - Context API beépített state management-hez
     - Hooks API a modern fejlesztéshez

2. **Vite 5.4.0**
   - Ultra gyors development server
   - Hot Module Replacement (HMR)
   - Optimalizált production build
   - TypeScript natív támogatás

3. **Mantine UI 7.12.0**
   - **Miért választottam:**
     - Komplett, modern component library
     - TypeScript-first megközelítés
     - Beépített form kezelés és validáció
     - Notification rendszer
     - Dátum kezelő komponensek
     - Könnyen testreszabható témák
     - Dokumentáció kiválóan megírt

4. **React Router 6.26.0**
   - Client-side routing
   - Nested routes támogatás
   - Protected routes implementálás
   - URL parameter kezelés

5. **Axios 1.7.4**
   - HTTP kliens
   - Interceptors támogatás (automatikus token hozzáadás)
   - Request/response transzformáció
   - Promise-based API

---

## 3. ARCHITEKTÚRA ÉS TERVEZÉSI MINTÁK

### Milyen architektúrát követtél?

**Háromrétegű (3-tier) layered architektúra:**

1. **Presentation Layer (Brewed API projekt)**
   - Controllers: HTTP kérések fogadása, response készítés
   - Middleware: CORS, Authentication, Exception handling
   - Dependency Injection konfiguráció
   - Swagger dokumentáció

2. **Business Logic Layer (Brewed.Services projekt)**
   - Service interfészek és implementációk
   - Üzleti logika és validációk
   - Email küldés (SMTP)
   - PDF generálás
   - AutoMapper profilok

3. **Data Access Layer (Brewed.DataContext projekt)**
   - Entity modellek (domain objektumok)
   - DbContext konfiguráció
   - Migrations
   - DTOs (Data Transfer Objects)

**Előnyök:**
- Separation of Concerns (SoC)
- Könnyebb tesztelhetőség (unit testek service rétegre)
- Független fejleszthetőség
- Könnyebb karbantartás és bővítés

### Milyen tervezési mintákat használtál?

1. **Repository Pattern (implicit az EF Core-ban)**
   - DbContext és DbSet<T> repository-ként működik
   - Absztrakció az adatelérés felett

2. **Dependency Injection (DI)**
   - Services regisztrálva a DI containerben
   - Constructor injection
   - Lazább kapcsolat az osztályok között
   - Könnyebb unit testing

3. **DTO Pattern**
   - Elkülöníti az entitásokat a külvilágnak küldött adatoktól
   - Kevesebb adat átvitel (csak a szükséges mezők)
   - Biztonság (pl. PasswordHash nem megy ki)

4. **Service Pattern**
   - Üzleti logika elkülönítése
   - Újrafelhasználható service metódusok
   - IUserService, IProductService, stb.

5. **Context Pattern (Frontend)**
   - AuthContext: autentikációs állapot globális kezelése
   - CartContext: kosár állapot megosztása komponensek között

6. **Provider Pattern (React)**
   - AuthProvider és CartProvider wrapperek
   - Központi state management

---

## 4. ADATBÁZIS

### Milyen adatbázist használtál és miért?

**Microsoft SQL Server (LocalDB a fejlesztéshez)**

**Miért választottam:**
- Erős integráció az Entity Framework Core-ral
- ACID tulajdonságok (Atomicity, Consistency, Isolation, Durability)
- Relációs adatbázis, amely jól illeszkedik a domain modellhez
- Széles körű enterprise támogatás
- LocalDB egyszerű fejlesztői élményt biztosít (nincs szükség külön szerver telepítésre)
- Könnyű migrálás Azure SQL-re vagy teljes SQL Server-re élesítéskor

### Hogyan van felépítve az adatbázis sémája?

**Fő táblák és relációk:**

1. **Users (Felhasználók)**
   - One-to-Many: Orders, Reviews, Addresses, UserCoupons
   - One-to-One: Cart

2. **Products (Termékek)**
   - Many-to-One: Category
   - One-to-Many: ProductImages, Reviews, OrderItems, CartItems

3. **Orders (Rendelések)**
   - Many-to-One: User (nullable, vendég rendelésekhez)
   - Many-to-One: ShippingAddress, BillingAddress
   - One-to-Many: OrderItems
   - One-to-One: Invoice, GuestOrderDetails

4. **Coupons (Kuponok)**
   - Many-to-Many: Users (UserCoupons junction table-ön keresztül)

**Normalizáció:**
- A séma 3NF (Third Normal Form) normalizált
- Redundancia minimalizálva
- Relációs integritás foreign key constraint-ekkel

**Indexek:**
- Primary keys automatikus indexelése
- Unique index: Review táblán (UserId, ProductId) - egy user csak egyszer értékelhet egy terméket
- Foreign key indexek a jobb join teljesítményért

---

## 5. AUTENTIKÁCIÓ ÉS BIZTONSÁG

### Hogyan működik az autentikáció?

**JWT (JSON Web Token) alapú autentikáció:**

1. **Regisztráció folyamat:**
   - User kitölti a regisztrációs formot
   - Backend SHA256-tal hashelje a jelszót
   - User rekord mentése az adatbázisba
   - Email confirmation token generálás
   - Visszaigazoló email küldés
   - User a linkre kattintva megerősíti az emailt

2. **Bejelentkezés folyamat:**
   - User email és jelszó megadása
   - Backend ellenőrzi a hashed jelszót
   - Ha sikeres, JWT token generálás (30 nap lejárat)
   - Token tartalmazza: UserId, Email, Name, Role
   - Frontend localStorage-ban tárolja a tokent
   - Minden API híváshoz Authorization header-ben elküldi

3. **Token validáció:**
   - Backend minden védett endpoint-nál ellenőrzi a tokent
   - Issuer és Audience validáció
   - Signature ellenőrzés
   - Lejárati idő ellenőrzés

**Jelszó biztonság:**
- SHA256 hashing
- Jelszó egyszer hashelve, nem tárolva plain text-ben
- Password reset token-based, időkorláttal (például 1 óra)

### Milyen biztonsági megoldásokat alkalmaztál?

1. **CORS (Cross-Origin Resource Sharing)**
   - Megadott origin engedélyezése (frontend URL)
   - Credential támogatás

2. **Role-based Authorization**
   - [Authorize] attribútum controller metódusokon
   - Role check: Admin vagy RegisteredUser
   - Frontend oldalon protected routes

3. **Input validáció**
   - DTO-k data annotation-ökkel (Required, EmailAddress, stb.)
   - Email formátum ellenőrzés
   - Numerikus mezők validációja

4. **SQL Injection védelem**
   - Entity Framework paraméteres lekérdezések
   - LINQ típusbiztos query-k

5. **Password Reset biztonság**
   - Token-based reset
   - Token egyszer használatos
   - Lejárati idő (1 óra)

6. **Email Confirmation**
   - Csak megerősített email-lel lehet teljes funkcionalitás

---

## 6. FŐBB FUNKCIÓK IMPLEMENTÁCIÓJA

### Hogyan működik a kosár rendszer?

**Kettős megközelítés:**

1. **Bejelentkezett felhasználók:**
   - UserId alapján persisted kosár az adatbázisban
   - Cart és CartItems táblák
   - Bejelentkezés után a kosár megmarad
   - Több eszközön is szinkronizált

2. **Vendég felhasználók:**
   - SessionId alapján session-based kosár
   - Ugyanaz a Cart entitás, de UserId NULL, SessionId kitöltve
   - Frontend generál egy egyedi SessionId-t (UUID)
   - Regisztráció után migrálhatók a kosár elemek

**API működés:**
- GET /api/cart - Kosár lekérdezés (UserId vagy SessionId alapján)
- POST /api/cart/items - Termék hozzáadás (ha már létezik, quantity növelés)
- PUT /api/cart/items/{id} - Mennyiség módosítás
- DELETE /api/cart/items/{id} - Termék eltávolítás
- DELETE /api/cart - Teljes kosár ürítés

### Hogyan működik a kupon rendszer?

**Kupon típusok:**
- Százalékos kedvezmény (DiscountType: Percentage)
- Fix összegű kedvezmény (DiscountType: FixedAmount)

**Kupon tulajdonságok:**
- Minimum rendelési összeg (MinimumOrderAmount)
- Kezdő és lejárati dátum
- Maximális felhasználási szám (globális)
- Aktív/inaktív státusz

**UserCoupon rendszer:**
- Admin hozzárendel kuponokat userhez
- User csak neki assigned kuponokat használhatja
- IsUsed flag jelzi, hogy felhasználta-e már
- UsedDate és OrderId követés

**Validáció:**
- Kupon létezik és aktív?
- Dátum tartományban van?
- User-hez assigned?
- Még nem használta fel?
- Min. rendelési összeg teljesül?
- Globális max használat nem lépte túl?

**Alkalmazás:**
- Checkout során kupon kód megadása
- Backend validálja és kedvezményt számol
- Order létrehozáskor UserCoupon IsUsed = true

### Hogyan működik az értékelési (review) rendszer?

**Constraint:**
- Egy felhasználó egy terméket csak egyszer értékelhet
- Unique index: (UserId, ProductId)

**Védelem visszaélés ellen:**
- Csak olyan termékeket lehet értékelni, amit a user megvásárolt
- Backend ellenőrzi: `HasPurchasedProduct(userId, productId)`
- Ellenőrzi az Orders és OrderItems táblákat

**Értékelés komponensek:**
- Rating: 1-5 csillag
- Title: Rövid összefoglaló
- Comment: Részletes szöveges értékelés
- CreatedAt: Időbélyeg

**Moderáció:**
- Admin törölhet értékeléseket
- User saját értékelését törölheti

### Hogyan működik a számla generálás?

**Technológia: QuestPDF**

**Folyamat:**
1. Rendelés leadása után admin generálhat számlát
2. QuestPDF fluent API-val PDF kreálás:
   - Számla adatok (number, date, amount)
   - Megrendelő adatok
   - Termékek listája (név, mennyiség, ár)
   - Összegzés (subtotal, shipping, discount, total)
3. PDF mentése a szerveren (`wwwroot/invoices/{InvoiceNumber}.pdf`)
4. Invoice rekord mentése az adatbázisba (OrderId kapcsolat)
5. User letöltheti GET `/api/orders/{orderId}/invoice/pdf` endpoint-ról

**Előnyök:**
- Professzionális számla megjelenés
- Letölthető PDF formátum
- Perzisztens tárolás

---

## 7. FRONTEND ARCHITEKTÚRA

### Hogyan szervezted meg a frontend kódot?

**Mappák szerkezet:**

```
src/
├── api/              # API kommunikáció (axios config, service)
├── components/       # Újrafelhasználható komponensek
│   └── Layout/       # Layout komponensek (Header, Navbar)
├── context/          # React Context providers (Auth, Cart)
├── hooks/            # Custom hooks (useAuth)
├── interfaces/       # TypeScript interface definíciók
├── pages/            # Oldal komponensek (route-okhoz)
│   ├── Admin/        # Admin oldalak
│   └── Auth/         # Autentikációs oldalak
└── App.tsx           # Fő app routing
```

**Komponens hierarchia:**
- `App.tsx` - Router setup
- `BasicLayout` - Közös layout header + sidebar + outlet
- Page komponensek - Konkrét oldalak
- Reusable komponensek - ProductCard, ReviewCard, stb.

### Hogyan oldottad meg a state management-et?

**Context API használata:**

1. **AuthContext:**
   - Token tárolás
   - User info (id, name, email, role)
   - Login/logout funkciók
   - isAuthenticated computed érték
   - LocalStorage sync

2. **CartContext:**
   - Cart count (badge megjelenítéshez)
   - Refresh cart metódus
   - Összes komponens eléri

**Miért nem Redux?**
- Kisebb projekt, nem volt szükség Redux komplexitására
- Context API elegendő a globális state-hez
- Kevesebb boilerplate
- React beépített megoldás

### Hogyan működik az API kommunikáció?

**Centralizált API service (`api.ts`):**

```typescript
// Axios instance konfigurációval
const api = axios.create({
  baseURL: 'https://localhost:7269/api',
  headers: { 'Content-Type': 'application/json' }
})

// Request interceptor: automatikus token hozzáadás
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor: 401-nél logout
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      // Logout, redirect to login
    }
    return Promise.reject(error)
  }
)
```

**Előnyök:**
- Központi konfiguráció
- Automatikus auth token kezelés
- Error handling egy helyen
- TypeScript interfészek típusbiztonsághoz

---

## 8. KIHÍVÁSOK ÉS MEGOLDÁSOK

### Milyen kihívásokkal találkoztál a fejlesztés során?

**1. Vendég vásárlók kosár kezelése**
- **Probléma:** Hogyan tároljam a kosarat bejelentkezés nélkül?
- **Megoldás:** SessionId alapú kosár, frontend generál UUID-t, backend ugyanazt a Cart entitást használja

**2. Kupon rendszer komplexitása**
- **Probléma:** Globális max használat + user-specifikus assigned kuponok + felhasználás tracking
- **Megoldás:** UserCoupon junction table, külön validációs logika, IsUsed flag

**3. Értékelés visszaélés védelme**
- **Probléma:** User ne tudjon értékelni olyan terméket, amit nem vásárolt
- **Megoldás:** Backend ellenőrzés HasPurchasedProduct, Orders és OrderItems táblák JOIN-ja

**4. CORS problémák fejlesztés során**
- **Probléma:** Frontend (localhost:5173) nem tudott hívni backend-et (localhost:7269)
- **Megoldás:** CORS policy konfiguráció backend-en, WithOrigins és AllowCredentials

**5. JWT token lejárat kezelés**
- **Probléma:** Mi történjen, ha a token lejár használat közben?
- **Megoldás:** Response interceptor 401-re logout és redirect login-ra

**6. TypeScript interfészek szinkronban tartása backend DTO-kkal**
- **Probléma:** Frontend interfészek és backend DTO-k eltérhetnek
- **Megoldás:** Manuális szinkronizálás, future: code generation (pl. NSwag)

---

## 9. TESZTELÉS ÉS MINŐSÉGBIZTOSÍTÁS

### Hogyan teszteltél?

**Manuális tesztelés:**
- Swagger UI az API endpoint-ok teszteléséhez
- Postman a komplex API flow-k teszteléséhez
- Browser DevTools network tab
- Különböző user role-ok (Admin, User, Guest) tesztelése

**Code Quality:**
- TypeScript strict mode
- C# nullable reference types
- Konzisztens kódolási stílus

**Lehetséges bővítések:**
- Unit tesztek (xUnit backend, Jest/Vitest frontend)
- Integration tesztek
- E2E tesztek (Playwright/Cypress)

---

## 10. DEPLOYMENT ÉS ÉLESÍTÉS

### Hogyan lehet élesíteni az alkalmazást?

**Backend élesítés:**
1. Connection string módosítás valós SQL Server-re vagy Azure SQL-re
2. appsettings.Production.json konfiguráció
3. JWT kulcs környezeti változóba (ne legyen hardcoded)
4. SMTP beállítások production értékekre
5. Deploy Azure App Service-re vagy IIS-re
6. HTTPS tanúsítvány beállítás

**Frontend élesítés:**
1. `npm run build` - Production build Vite-tal
2. API baseURL környezeti változóból (VITE_API_URL)
3. Deploy Azure Static Web Apps / Vercel / Netlify-ra
4. HTTPS konfigurálás

**Adatbázis migráció:**
- EF Core migrations futtatása: `dotnet ef database update`
- Seed data éles környezetben (opcionális)

---

## 11. JÖVŐBELI FEJLESZTÉSI LEHETŐSÉGEK

### Mit lehetne még hozzáadni/fejleszteni?

1. **Fizetési integráció**
   - Stripe, PayPal integráció
   - Valós bankkártyás fizetés

2. **Email template-ek**
   - Szebb HTML email-ek (jelenleg plain text)
   - Template engine (Razor, HandleBars)

3. **Termék variánsok**
   - Különböző kiszerelések (250g, 500g, 1kg)
   - Őrlési fok választása

4. **Wishlist (kívánságlista)**
   - Kedvenc termékek mentése

5. **Előfizetéses modell**
   - Havonta automatikus kávé szállítás

6. **Élő chat támogatás**
   - SignalR WebSocket integráció

7. **Multi-language támogatás**
   - i18n frontend-en
   - Lokalizált backend

8. **Teljesítmény optimalizáció**
   - Redis cache
   - CDN képekhez
   - Database query optimization

9. **Analytics**
   - Google Analytics integráció
   - User behavior tracking

10. **Mobilalkalmazás**
    - React Native vagy Flutter
    - Ugyanaz az API backend

---

## 12. ZÁRÓ GONDOLATOK

### Mit tanultál a projekt során?

- **Full-stack fejlesztés:** Teljes alkalmazás tervezése és implementálása
- **API tervezés:** RESTful API best practices
- **Biztonság:** Autentikáció és autorizáció implementálása
- **State management:** Frontend és backend state kezelés
- **Adatbázis tervezés:** Normalizáció, relációk, indexek
- **Modern tooling:** Vite, EF Core, Mantine, TypeScript előnyei
- **Problémamegoldás:** Komplex business logika implementálása (kuponok, értékelések)

### Miért választottad ezt a tech stack-et?

- Modern, aktívan fejlesztett technológiák
- Jó dokumentáció és közösségi támogatás
- Személyes tapasztalat és tanulási szándék
- Industry-standard megoldások (React, .NET)
- TypeScript és C# típusbiztonság előnyei
- Jó teljesítmény és skálázhatóság

---

**Készítve:** 2025-11-28
**Projekt:** Brewed - E-Commerce Coffee Shop Platform
**Készítette:** [Neved]
