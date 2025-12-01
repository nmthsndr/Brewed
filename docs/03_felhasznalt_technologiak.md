# 3. Felhasznált technológiák

Az informatikában a technológiai fejlődés egyik legfontosabb hajtóereje a fejlesztői közösség együttműködése és az open-source projektek. Számtalan olyan eszköz és keretrendszer létezik, amelyek célja más programozók munkájának megkönnyítése, hatékonyságának növelése. Szakdolgozatom során is ezekre a bevált, közösség által támogatott technológiákra támaszkodtam, hogy egy modern, jól karbantartható webalkalmazást hozzak létre.

A projekt során különös figyelmet fordítottam arra, hogy olyan technológiákat válasszak, amelyek nemcsak a jelenlegi igényeket elégítik ki, hanem hosszú távon is fenntarthatóak és könnyen bővíthetőek. A választott technológiai stack két fő részre bontható: a backend (szerveroldali) és a frontend (kliensoldali) technológiákra.

## 3.1. Git, GitHub és verziókezelés

A Git ma már elengedhetetlen eszköz a modern szoftverfejlesztésben. Ez a decentralizált verziókezelő rendszer lehetővé teszi, hogy nyomon kövessük a projekt minden változását, bármikor visszatérhessünk egy korábbi állapotra, és párhuzamosan dolgozzunk különböző funkciókon anélkül, hogy a többi fejlesztő munkáját zavarnánk [1].

Személyes tapasztalataim szerint a Git használata nélkülözhetetlen minden komolyabb szoftverprojekt során. Középiskola óta használom, és számtalanszor bizonyította hasznát bonyolultabb fejlesztések során. A verziókezelés legnagyobb előnye, hogy biztonságot nyújt: ha valami elromlik, vagy egy kísérlet nem sikerül, egyszerűen visszatérhetünk egy működő verzióhoz. Ez a szakdolgozati projekt során is nélkülözhetetlen volt, különösen amikor nagyobb refaktorálásokat végeztem.

A Git mellett természetesen a GitHub platformot is intenzíven használtam. A GitHub nemcsak egy felhőalapú Git tárhelyszolgáltatás, hanem egy komplex fejlesztői platform, amely számos további funkciót kínál [2]. A projektem során különösen hasznosnak találtam a GitHub következő funkcióit:

- **Távoli tárolás és biztonsági mentés**: A kód automatikusan szinkronizálódik a felhőbe, így több helyről is hozzáférhetek, és nem kell aggódnom az adatvesztés miatt.
- **Issue tracking**: A fejlesztendő funkciókat és javítandó hibákat strukturáltan tudom nyomon követni.
- **Branch management**: A GitHub webes felülete átlátható képet ad a különböző ágakról és azok állapotáról.

A verziókezelés mindennapi használatához a GitHub Desktop alkalmazást használom. Bár a választott fejlesztői környezetem (Visual Studio Code) beépítetten támogatja a Git funkciókat, a GitHub Desktop egyszerűsége és felhasználóbarát felülete miatt mégis ezt preferálom. Különösen hasznos számomra a különböző GitHub fiókok közötti egyszerű váltás funkció, ami lehetővé teszi, hogy gyorsan válthassak a munkahelyi és személyes projektjeim között [3].

## 3.2. Visual Studio Code

A Visual Studio Code (VSCode) jelenleg az egyik legnépszerűbb kódszerkesztő a fejlesztői közösségben, és ennek megvannak a jó okai [4]. A Microsoft által fejlesztett, ingyenes és nyílt forráskódú szerkesztő rendkívül testreszabható, gyors, és bővítményeknek köszönhetően szinte bármilyen programozási nyelvhez és keretrendszerhez alkalmazható.

Középiskola óta használom a VSCode-ot különböző projektekhez, legyen az webalkalmazás, Python script, vagy akár játékmodok fejlesztése. A szakdolgozati projektem során különösen az alábbi beépített funkciókat és bővítményeket találtam hasznosnak:

### 3.2.1. Beépített funkciók

- **IntelliSense**: Intelligens kódkiegészítés, amely nemcsak a nyelvi elemeket, hanem a projekt-specifikus kódot is ajánlja.
- **JavaScript és TypeScript nyelvi támogatás**: Beépített szintaxis kiemelés, típusellenőrzés és automatikus import szervezés.
- **Integrált terminál**: Közvetlenül a szerkesztőből futtathatók a build parancsok és tesztek.
- **Git integráció**: Beépített verziókezelés vizuális visszajelzéssel a módosított fájlokról.

### 3.2.2. Fontosabb bővítmények

- **ESLint**: Automatikus kódminőség ellenőrzés és formázás JavaScript és TypeScript projektekhez. Ez segít abban, hogy konzisztens kódstílust tartsak fenn és már fejlesztés közben észrevegyem a potenciális hibákat [5].

- **ES7+ React/Redux/React-Native snippets**: Gyors kódrészletek (snippets) React komponensek és hookok létrehozásához. Ez jelentősen felgyorsítja a fejlesztést, mivel a gyakran használt kódmintákat néhány karakterrel be tudom illeszteni.

- **GitHub Copilot**: Mesterséges intelligencia alapú kódsegéd, amely kontextus-érzékeny javaslatokat ad. A projektben való használata során gyakran hasznos mintakódokat kaptam, amelyek felgyorsították a fejlesztést, különösen ismétlődő logikai minták implementálásakor [6].

- **C# Dev Kit**: Átfogó támogatás C# fejlesztéshez, amely az IntelliSense-t, debuggolást és projektkezelést biztosítja a backend fejlesztéséhez.

A VSCode moduláris felépítése lehetővé teszi, hogy minden projekthez testre szabjam a fejlesztői környezetet, így mindig csak azok a funkciók aktívak, amelyekre ténylegesen szükségem van.

## 3.3. Frontend technológiák

A modern webalkalmazások fejlesztése jelentősen megváltozott az elmúlt években. A statikus HTML oldalaktól eljutottunk a dinamikus, interaktív Single Page Application (SPA) alkalmazásokig. A projektben a frontend fejlesztéséhez olyan technológiákat választottam, amelyek ma az iparági standard részét képezik.

### 3.3.1. React

A React egy JavaScript könyvtár felhasználói felületek építéséhez, amelyet a Facebook (Meta) fejlesztett és tart karban [7]. A React filozófiája a komponens-alapú fejlesztés, amely lehetővé teszi, hogy újrafelhasználható UI elemeket hozzunk létre. Minden komponens saját logikával és megjelenéssel rendelkezik, így a kód moduláris és könnyen karbantartható marad.

A React választása mellett több érv szólt a projektemben:

**Komponens-alapú architektúra**: A webalkalmazás különböző részeit (például termékek listája, rendelési űrlap, navigáció) független komponensekként fejlesztettem, amelyek később könnyedén újrafelhasználhatók más kontextusban is.

**Virtuális DOM**: A React egy virtuális DOM-ot használ, amely jelentősen javítja az alkalmazás teljesítményét. Amikor az állapot megváltozik, a React először a virtuális DOM-ban hajtja végre a módosításokat, majd egy hatékony algoritmussal csak a tényleges változásokat rendereli újra a böngészőben [8].

**Gazdag ökoszisztéma**: A React körül hatalmas közösség és rengeteg könyvtár alakult ki. Ez azt jelenti, hogy szinte bármilyen problémára találhatunk már kész megoldást vagy segítséget.

**React Hooks**: A modern React fejlesztés alapja a Hooks használata, amely lehetővé teszi az állapotkezelést és mellékhatások kezelését funkcionális komponensekben. A projekt során intenzíven használtam a `useState`, `useEffect`, `useContext` és egyéni hookokat is készítettem [9].

Például a projektben egy ilyen egyéni hook segíti az autentikáció kezelését:

```typescript
// useAuth hook az autentikációhoz
const useAuth = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Token validálás és user betöltése
  }, []);

  return { user, loading };
};
```

### 3.3.2. TypeScript

A TypeScript egy típusos, JavaScript-re fordítódó programozási nyelv, amelyet a Microsoft fejlesztett [10]. A TypeScript lényegében a JavaScript szintaxisát bővíti típusokkal, amely jelentősen növeli a kód megbízhatóságát és karbantarthatóságát.

A TypeScript használatának legfőbb előnyei a projektemben:

**Típusbiztonság**: A típusok használata már fejlesztési időben kiszűri a sok tipikus hibát. Például ha egy függvény számot vár, de stringet kapna, a TypeScript fordító azonnal jelzi a hibát, még mielőtt a kódot futtatnánk.

**Jobb fejlesztői élmény**: Az IDE-k (mint a VSCode) pontosabb kódkiegészítést és dokumentációt tudnak nyújtani, ha ismerik a típusokat.

**Refaktorálás biztonsága**: Amikor nagyobb változtatásokat végzek a kódban, a TypeScript azonnal jelzi, ha valamit elfelejtettem módosítani.

**Interface-ek és típus definíciók**: A projekt során definiáltam minden fontos adatstruktúrát, ami egyértelművé teszi a kód működését:

```typescript
interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  categoryId: number;
}

interface CartItem {
  product: Product;
  quantity: number;
}
```

### 3.3.3. Vite

A Vite egy modern frontend build eszköz, amely jelentősen gyorsabb a hagyományos bundler-eknél (mint például a Webpack) [11]. A Vite kifejezetten a modern böngészők ES modulok támogatására épül, és fejlesztői módban nem bundelezi a kódot, hanem közvetlenül szolgálja ki a modulokat.

A Vite választásának okai:

**Rendkívül gyors fejlesztői szerver**: A Vite a natív ES modulokat használja, így a szerverindítás és a hot module replacement (HMR) szinte azonnali, még nagy projekteknél is.

**Optimalizált production build**: Éles környezetbe a Vite a Rollup-ot használja, amely hatékonyan optimalizálja és bundelja a kódot.

**Egyszerű konfiguráció**: A Vite alapértelmezett beállításai jól működnek a legtöbb projektnél, így minimális konfigurációval lehet elindulni.

A projektem `vite.config.ts` fájlja rendkívül egyszerű:

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000
  }
})
```

### 3.3.4. React Router DOM

A React Router DOM az egyik leggyakrabban használt routing könyvtár React alkalmazásokhoz [12]. Single Page Application-öknél elengedhetetlen, hogy különböző URL-eken különböző tartalmakat jelenítsünk meg anélkül, hogy teljes oldal újratöltés történne.

A projektben a React Router segítségével valósítottam meg:

- **Többszintű navigációt**: Főoldal, termékek listája, termék részletei, kosár, stb.
- **Védett útvonalakat**: Bizonyos oldalak csak bejelentkezett felhasználók számára elérhetők.
- **Programozott navigációt**: A sikeres műveletek után automatikus átirányítás.

```typescript
<Routes>
  <Route path="/" element={<HomePage />} />
  <Route path="/products" element={<ProductsPage />} />
  <Route path="/products/:id" element={<ProductDetailPage />} />
  <Route path="/cart" element={<ProtectedRoute><CartPage /></ProtectedRoute>} />
</Routes>
```

### 3.3.5. Mantine

A Mantine egy modern React komponens könyvtár, amely teljes funkcionalitású, jól dokumentált és kiválóan testreszabható UI komponenseket biztosít [13]. A projektben való használata jelentősen felgyorsította a fejlesztést, mivel nem kellett alapvető UI elemeket (gombok, input mezők, modálok, stb.) nulláról létrehozni.

A Mantine előnyei a projektben:

**Gazdag komponens készlet**: A Mantine több mint 100 komponenst kínál, beleértve az űrlapkezelést, értesítéseket, modálokat, dátumválasztókat és sok mást.

**Beépített téma rendszer**: Könnyedén testreszabható színek, betűtípusok és más vizuális elemek az egész alkalmazásban.

**Hooks és segédfunkciók**: A Mantine nemcsak komponenseket, hanem hasznos hook-okat is kínál (például `useForm`, `useDisclosure`, `useMediaQuery`).

**Hozzáférhetőség**: Minden Mantine komponens figyelembe veszi az akadálymentességi szempontokat (ARIA attribútumok, billentyűzet navigáció).

Például a projektben a formoknál a Mantine form kezelését használtam:

```typescript
import { useForm } from '@mantine/form';

const form = useForm({
  initialValues: {
    email: '',
    password: '',
  },
  validate: {
    email: (value) => (/^\S+@\S+$/.test(value) ? null : 'Érvénytelen email cím'),
  },
});
```

### 3.3.6. Axios

Az Axios egy népszerű HTTP kliens könyvtár, amely egyszerűsíti az API hívások kezelését JavaScript és TypeScript projektekben [14]. Bár a modern böngészők beépített `fetch` API-val rendelkeznek, az Axios számos további funkciót kínál, amely hasznos lehet komplex alkalmazásoknál.

A projektben az Axios használatának előnyei:

**Automatikus JSON transzformáció**: Az Axios automatikusan JSON-ná alakítja a válaszokat és kéréseket.

**Interceptor-ok**: Lehetőség van központi helyen kezelni a kéréseket és válaszokat. Például minden kérésnél automatikusan hozzáadhatjuk a JWT tokent:

```typescript
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

**Hibakezelés**: Az Axios konzisztens hibakezelést biztosít, könnyű megkülönböztetni a hálózati hibákat a szerver oldali hibáktól.

**Request cancellation**: Lehetőség van egy folyamatban lévő kérés megszakítására, ami hasznos például keresés során.

### 3.3.7. További frontend könyvtárak

**jwt-decode**: Ez a könyvtár lehetővé teszi a JWT tokenek dekódolását kliens oldalon, így könnyen hozzáférhetünk a tokenben tárolt információkhoz (például felhasználói ID, szerepkör) anélkül, hogy szerverhez kellene fordulnunk [15].

**dayjs**: Egy könnyűsúlyú dátum-kezelő könyvtár, amely a Moment.js modern alternatívája [16]. A projektben dátumok formázására, időzónák kezelésére és dátum számításokra használtam.

**Tabler Icons**: Egy nyílt forráskódú ikon készlet, amely több mint 4000 vektoros ikont tartalmaz [17]. A Mantine-nel tökéletesen integrálódik, és konzisztens vizuális nyelvet biztosít az alkalmazásban.

## 3.4. Backend technológiák

A backend a webalkalmazás szerveroldali része, amely az üzleti logikát, adatbázis műveleteket és az API-t biztosítja. A projektben a Microsoft .NET ökoszisztémára építettem, amely enterprise szintű, megbízható és jól teljesítő backend alkalmazások fejlesztését teszi lehetővé.

### 3.4.1. ASP.NET Core

Az ASP.NET Core a Microsoft keresztplatformos, nyílt forráskódú keretrendszere webalkalmazások és API-k építésére [18]. A .NET 9.0 verzióját használtam, amely a legújabb funkciókat és teljesítményjavításokat tartalmazza.

Az ASP.NET Core előnyei a projektben:

**Keresztplatform**: Ellentétben a korábbi .NET Framework-kel, az ASP.NET Core futtatható Windows, Linux és macOS rendszereken is. Ez rugalmasságot biztosít a deployment során.

**Nagy teljesítmény**: Az ASP.NET Core az egyik leggyorsabb webes keretrendszer, benchmark-ok szerint sokszor felülmúlja más népszerű platformokat (mint például Node.js vagy Django) [19].

**Beépített dependency injection**: Az ASP.NET Core beépített DI konténerrel rendelkezik, amely támogatja a clean architecture és SOLID elvek alkalmazását.

**Middleware pipeline**: A kérések feldolgozása egy jól strukturált middleware láncban történik, amely könnyen testreszabható és bővíthető.

**RESTful API támogatás**: Az ASP.NET Core kiválóan alkalmas RESTful API-k építésére, beépített támogatással a routing, model binding, és content negotiation-höz.

A projektben kontrollereket használtam az API végpontok meghatározására:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }
}
```

### 3.4.2. Entity Framework Core

Az Entity Framework Core (EF Core) egy modern object-relational mapper (ORM), amely lehetővé teszi, hogy .NET objektumokkal dolgozzunk az adatbázis helyett közvetlenül SQL lekérdezésekkel [20]. Az EF Core 9.0 verziót használtam a projektben.

Az EF Core használatának előnyei:

**Code-First megközelítés**: Az adatbázis sémát C# osztályokból generáltam, így a kód az egyetlen igazság forrása (single source of truth).

**LINQ támogatás**: Típusbiztos lekérdezéseket írhatok LINQ szintaxissal, amit az EF Core automatikusan SQL-re fordít.

**Migrations**: Az adatbázis séma változásokat verziókezelt migration-ökben követhetem nyomon, ami megkönnyíti a deployment-et és a csapatmunkát.

**Lazy és Eager loading**: Rugalmasan választhatok, hogy mikor töltöm be a kapcsolódó entitásokat.

Példa egy entitás definícióra:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
```

### 3.4.3. SQL Server

A Microsoft SQL Server egy relációs adatbázis-kezelő rendszer, amely az egyik leggyakrabban használt enterprise adatbázis megoldás [21]. A projektben a SQL Server a perzisztens adattárolást biztosítja.

A SQL Server választásának okai:

**Integráció az ASP.NET Core-ral**: Az EF Core kiválóan működik együtt a SQL Server-rel, optimalizált SQL lekérdezéseket generál.

**Megbízhatóság és teljesítmény**: A SQL Server robusztus tranzakciókezelést, indexelést és lekérdezés-optimalizálást biztosít.

**Fejlesztői eszközök**: A SQL Server Management Studio (SSMS) és Azure Data Studio kiváló eszközök az adatbázis kezelésére és monitorozására.

**Skálázhatóság**: A SQL Server támogatja a replikációt, particionálást és más skálázási technikákat nagyobb rendszereknél.

### 3.4.4. JWT Authentication

A JSON Web Token (JWT) egy nyílt szabvány (RFC 7519) a biztonságos információcsere megvalósítására [22]. A projektben JWT-t használok az autentikáció és autorizáció megvalósítására.

A JWT alapú autentikáció előnyei:

**Stateless**: A szerver nem tárol session információt, minden szükséges információ a tokenben van. Ez egyszerűsíti a skálázást.

**Keresztplatform**: A JWT szabvány, így bármilyen platform és programozási nyelv használható a token generálására és validálására.

**Biztonságos**: A tokent digitálisan aláírják, így nem módosítható a kliens oldalon.

A projektben a bejelentkezés során a szerver generál egy JWT tokent:

```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[] {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    }),
    Expires = DateTime.UtcNow.AddDays(7),
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
```

### 3.4.5. AutoMapper

Az AutoMapper egy objektum-objektum mapper könyvtár .NET-hez, amely automatizálja a különböző típusú objektumok közötti leképezést [23]. A projektben az adatbázis entitások és a Data Transfer Object-ek (DTO-k) közötti konverzióra használom.

Az AutoMapper előnyei:

**Kódismétlés csökkentése**: Nem kell manuálisan írni az objektum másolási logikát minden helyen.

**Karbantarthatóság**: A leképezési logika központi helyen van definiálva.

**Típusbiztonság**: Compile-time ellenőrzés, hogy a leképezések helyesek-e.

Példa mapping profil:

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
    }
}
```

### 3.4.6. Swagger/Swashbuckle

A Swagger (OpenAPI) egy specifikáció RESTful API-k dokumentálására, a Swashbuckle pedig az ASP.NET Core implementációja [24]. A projektben fejlesztés során a Swagger UI-t használom az API végpontok tesztelésére és dokumentálására.

A Swagger használatának előnyei:

**Automatikus dokumentáció**: A kódból automatikusan generálódik az API dokumentáció.

**Interaktív tesztelés**: A Swagger UI-ban közvetlenül kipróbálhatók az API végpontok.

**API szerződés**: A generált OpenAPI specifikáció használható kliens kód generálására különböző nyelveken.

### 3.4.7. QuestPDF

A QuestPDF egy modern, nyílt forráskódú C# könyvtár PDF dokumentumok generálására [25]. A projektben a QuestPDF segítségével valósítottam meg a számlák és jelentések PDF formátumban való előállítását.

A QuestPDF előnyei:

**Fluent API**: Intuitív, könnyen olvasható szintaxis a dokumentumok felépítésére.

**Teljesítmény**: Gyors és memória-hatékony PDF generálás.

**Rugalmasság**: Teljes kontroll a dokumentum megjelenése felett.

Példa egyszerű PDF generálásra:

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Text("Számla").FontSize(20);
        page.Content().Column(column =>
        {
            column.Item().Text($"Dátum: {DateTime.Now:yyyy-MM-dd}");
            column.Item().Text($"Összeg: {total} Ft");
        });
    });
}).GeneratePdf("szamla.pdf");
```

## 3.5. Fejlesztői eszközök és munkafolyamat

### 3.5.1. Node.js és npm

A Node.js egy JavaScript futtatókörnyezet, amely a V8 JavaScript motorra épül [26]. Bár a projektben a backend .NET alapú, a frontend fejlesztéséhez elengedhetetlen a Node.js, amely lehetővé teszi JavaScript eszközök és build folyamatok futtatását a fejlesztői gépen.

Az npm (Node Package Manager) a Node.js csomagkezelője, amely a világ legnagyobb szoftver registry-je [27]. Az npm segítségével telepíthetők és kezelhetők a projekt függőségei. A `package.json` fájl tartalmazza az összes függőséget és npm script-et:

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

### 3.5.2. NuGet

A NuGet a .NET csomagkezelője, amely hasonló szerepet tölt be, mint az npm a JavaScript világában [28]. A projektben a backend függőségek (Entity Framework, AutoMapper, stb.) kezelésére használom.

## 3.6. Összegzés

A projekt során használt technológiák egy jól megalapozott, modern web stack-et alkotnak. A frontend oldalon a React, TypeScript és Vite kombinációja gyors fejlesztést és kiváló felhasználói élményt tesz lehetővé, míg a backend oldalon az ASP.NET Core és Entity Framework robusztus, skálázható megoldást biztosít. Minden választott technológia mögött aktív közösség és jó dokumentáció áll, ami megkönnyíti a fejlesztést és a problémamegoldást.

A technológiai választások során figyelembe vettem a teljesítményt, karbantarthatóságot, és a hosszú távú támogatottságot is. Úgy érzem, hogy a projekt során használt eszközök lehetővé tették számomra, hogy hatékonyan dolgozzak és minőségi szoftvert hozzak létre.

---

## Források

[1] Git. (n.d.). *Git - About Version Control*. Elérhető: https://git-scm.com/book/en/v2/Getting-Started-About-Version-Control

[2] GitHub. (n.d.). *GitHub Documentation*. Elérhető: https://docs.github.com/

[3] GitHub Desktop. (n.d.). *GitHub Desktop Documentation*. Elérhető: https://docs.github.com/en/desktop

[4] Visual Studio Code. (n.d.). *Visual Studio Code - Code Editing. Redefined*. Elérhető: https://code.visualstudio.com/

[5] ESLint. (n.d.). *ESLint - Pluggable JavaScript linter*. Elérhető: https://eslint.org/

[6] GitHub Copilot. (n.d.). *GitHub Copilot Documentation*. Elérhető: https://docs.github.com/en/copilot

[7] React. (n.d.). *React – A JavaScript library for building user interfaces*. Elérhető: https://react.dev/

[8] React Team. (n.d.). *Virtual DOM and Internals*. Elérhető: https://legacy.reactjs.org/docs/faq-internals.html

[9] React. (n.d.). *Hooks at a Glance*. Elérhető: https://react.dev/reference/react

[10] TypeScript. (n.d.). *TypeScript: JavaScript With Syntax For Types*. Elérhető: https://www.typescriptlang.org/

[11] Vite. (n.d.). *Vite - Next Generation Frontend Tooling*. Elérhető: https://vitejs.dev/

[12] React Router. (n.d.). *React Router - Declarative routing for React*. Elérhető: https://reactrouter.com/

[13] Mantine. (n.d.). *Mantine - A fully featured React components library*. Elérhető: https://mantine.dev/

[14] Axios. (n.d.). *Axios - Promise based HTTP client*. Elérhető: https://axios-http.com/

[15] jwt-decode. (n.d.). *jwt-decode - Small browser library that helps decoding JWTs*. Elérhető: https://github.com/auth0/jwt-decode

[16] Day.js. (n.d.). *Day.js - 2KB JavaScript date utility library*. Elérhető: https://day.js.org/

[17] Tabler Icons. (n.d.). *Tabler Icons - 4000+ Open source free SVG icons*. Elérhető: https://tabler-icons.io/

[18] Microsoft. (n.d.). *Introduction to ASP.NET Core*. Elérhető: https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core

[19] TechEmpower. (n.d.). *Web Framework Benchmarks*. Elérhető: https://www.techempower.com/benchmarks/

[20] Microsoft. (n.d.). *Entity Framework Core*. Elérhető: https://learn.microsoft.com/en-us/ef/core/

[21] Microsoft. (n.d.). *SQL Server technical documentation*. Elérhető: https://learn.microsoft.com/en-us/sql/sql-server/

[22] Jones, M., Bradley, J., & Sakimura, N. (2015). *JSON Web Token (JWT)*. RFC 7519. Elérhető: https://datatracker.ietf.org/doc/html/rfc7519

[23] AutoMapper. (n.d.). *AutoMapper - A convention-based object-object mapper*. Elérhető: https://automapper.org/

[24] Swagger. (n.d.). *Swagger - API Documentation & Design Tools*. Elérhető: https://swagger.io/

[25] QuestPDF. (n.d.). *QuestPDF - Modern open-source library for PDF document generation*. Elérhető: https://www.questpdf.com/

[26] Node.js. (n.d.). *About Node.js*. Elérhető: https://nodejs.org/en/about

[27] npm. (n.d.). *npm Documentation*. Elérhető: https://docs.npmjs.com/

[28] NuGet. (n.d.). *NuGet Documentation*. Elérhető: https://learn.microsoft.com/en-us/nuget/
