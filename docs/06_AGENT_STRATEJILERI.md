# Agent Stratejileri ve Proje Hakimiyeti

> Bu belge yaşayan bir dokümandır. Her önemli değişiklikte güncellenmelidir.
> Son güncelleme: 2026-08-15

---

## 0. PROJEYİ HIZLA KAVRAMA PROTOKOLÜ (Proje Hakimiyeti)

> Bu bölüm en kritik bölümdür. Yeni session açıldığında, bağlam kaybolduğunda
> veya "acaba bu dosya ne durumda?" diye sorduğumda buraya bakıyorum.

---

### 0.1 Layered Reading (Katmanlı Okuma Stratejisi)

Token'ı akıllıca kullanmak için her şeyi okumak yerine **katman katman** oku.

**Katman 1 — Sadece bu strateji dosyası (bu dosya)**
- "Proje Haritası" bölümüne bak
- "Açık Borçlar" listesine bak
- "Modül İlerleme" tablosuna bak
- Token maliyeti: Neredeyse sıfır (zaten bağlamda)

**Katman 2 — Görev ve talimat dosyaları (sadece ilgili bölüm)**
- `04_FAZ1_GOREVLER.md` → sıradaki modülün görevlerine bak
- `00_PRENSIP_VE_TALIMATLAR.md` → çalışma kuralları
- Token maliyeti: Küçük (hedefli okuma)

**Katman 3 — Hedefli dosya okuma (sadece gerektiğinde)**
- Enum kullanacaksam → ilgili enum dosyasını oku
- Interface implemente edeceksem → interface dosyasını oku
- Entity property'lerine bakacaksam → entity dosyasını oku
- Token maliyeti: Küçük (10-50 satır dosyalar)

**Katman 4 — Grep ile yapısal analiz (tüm proje tarama)**
- "Bu sınıf nerede kullanılıyor?" → grep
- "Bu metod var mı?" → grep
- "Hangi dosyalar bu namespace'i import ediyor?" → grep
- Token maliyeti: Orta (sonuç sayısına göre)

**Katman 5 — Tam dosya okuma (son çare)**
- Sadece gerçekten içeriğin tamamına ihtiyacım olduğunda
- Önce: "Bu dosyanın sadece ilk 10 satırını okursam yeterli mi?" diye sor
- Token maliyeti: Büyük

---

### 0.2 Projeye Özel Quirks (Tuzaklar) — EZBER ET

Bu projeye özgü garip şeyler. Hafızaya güvenme, buraya bak:

```
1. DOSYA ADLARINDA BOŞLUK VAR:
   "AppDbContext .cs", "UnitOfWork .cs", "SanctionEntryService .cs"
   → Silip yeniden adlandırılana kadar bu şekilde çalışacak.

2. NPGSQL LEGACY TIMESTAMP:
   AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)
   → Program.cs'in en başında. Olmadan seed data çalışmaz.
   → DateTime'lar UTC olarak işaretlenmediği için gerekli.

3. MATCHTYPE - SYSTEM.IO ÇAKIŞMASI:
   Domain'de MatchType enum var, System.IO'da da MatchType var.
   → using WatchListScreening.Domain.Enums; ekleyince çakışma çıkar.
   → Çözüm: WatchListScreening.Domain.Enums.MatchType (fully qualified) kullan.

4. SEARCHTYPE = ENTITYTYPE:
   ScreeningRequest.SearchType alanı EntityType tipinde!
   → ScreeningStatus değil, EntityType. (Person/Organization arıyoruz)
   → DTO'da da CreateScreeningRequestDto.SearchType = EntityType tipinde.

5. SOFT DELETE YAKLAŞIMI:
   SanctionEntry.IsActive = false → silme değil, pasif etme.
   → Repository'de Delete() gerçekten siler, SanctionEntryService.DeleteAsync
     ise IsActive=false + DeactivatedAt set ediyor. İkisi farklı!

6. SEED DATA UTC SORUNU:
   SeedData.cs'teki DateTime'lar: new DateTime(2024, 01, 01)
   → Kind=Unspecified → legacy switch olmadan hata verir.

7. PORT:
   API: http://localhost:5256 (5000 değil!)
   PostgreSQL: localhost:5432
   Redis: localhost:6379
   RabbitMQ: localhost:5672 (UI: 15672)

8. INCLUDE() KATMAN KURALI — KRİTİK:
   .Include() EF Core'a ait → Microsoft.EntityFrameworkCore.
   Application katmanında EF Core reference yoktur ve olmamalıdır.
   YANLIŞ: Service içinde _unitOfWork.X.Query().Include(...) kullanmak
   DOĞRU:  Repository'de özel metod yaz → Include() Infrastructure'da kalır.
   PATTERN: ISanctionEntryRepository gibi IScreeningResultRepository oluştur,
            GetPendingWithDetailsAsync() gibi metodlar ekle,
            Infrastructure'da Include() ile implemente et.
```

---

### STRATEJİ H: Katman Kirlilik Testi
**Kural:** Herhangi bir katmanda kod yazarken using listesine bak.
```
Application'da bu using'ler YASAK:
  - Microsoft.EntityFrameworkCore (EF Core)
  - Npgsql (PostgreSQL driver)
  - RabbitMQ.Client (mesajlaşma)
  - StackExchange.Redis (cache)

Bunları görürsen → Infrastructure'a taşı veya yeni bir Repository metodu yaz.
```


---

### 0.3 Bağımlılık Grafiği (Dependency Graph)

Hangi katman kime bağımlı — kod yazarken her zaman bu yönü kontrol et:

```
Domain ──────────────────────────────────── Kimseye bağımlı değil (core)
   ↑
Application ──── Domain'i bilir, Infrastructure'ı bilmez
   ↑
Infrastructure ── Domain + Application'ı bilir, API'yi bilmez
   ↑
API ────────────── Application + Infrastructure'ı bilir
   ↑
Web (MVC) ──────── Application + Infrastructure'ı bilir

KURAL: Oklar yukarı akar, aşağıya inemez.
- Infrastructure, API'yi import edemez.
- Application, Infrastructure'ı import edemez.
- Domain, kimseyi import edemez.
```

---

### 0.4 Naming Convention (İsimlendirme Kuralları)

Bu projede kullanılan standartlar — tutarlılık için her yeni dosyada uygula:

```
Entities:    PascalCase, tekil   — SanctionEntry, ScreeningRequest
DTOs:        [Eylem]EntityDto    — CreateSanctionEntryDto, UpdateReviewDto
Interfaces:  I prefix            — IRepository<T>, ISanctionEntryService
Services:    EntityService       — SanctionEntryService, ScreeningService
Controllers: [Entity]sController — SanctionEntriesController (çoğul!)
Config:      [Entity]Configuration — SanctionEntryConfiguration
Tables:      DbSet adı = çoğul  — SanctionEntries, ScreeningRequests
Namespaces:  WatchListScreening.[Katman].[Alt klasör]
```

---

### 0.5 Pattern Recognition (Yerleşik Kodlama Kalıpları)

Bu projede yerleşik örüntüler var. Yeni bir şey yazarken bunlara bak:

**Controller Paterni:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class XController : ControllerBase
{
    private readonly IXService _service;
    public XController(IXService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
```

**Service MapToDto Paterni:**
```csharp
private static EntityDto MapToDto(Entity e) => new()
{
    Id = e.Id,
    FullName = e.FullName,
    CreatedAt = e.CreatedAt
};
```

**Service Create Paterni:**
```csharp
public async Task<EntityDto> CreateAsync(CreateEntityDto dto)
{
    var entity = new Entity { ..., CreatedAt = DateTime.UtcNow };
    await _unitOfWork.Entities.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    return MapToDto(entity);
}
```

**Fluent API Paterni:**
```csharp
public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Entities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);
        builder.HasIndex(x => x.Name).HasDatabaseName("IX_Entities_Name");
    }
}
```

---

### 0.6 Kod Yazmadan Önce Red Flag Checklist

Her yeni kod bloğu yazmadan önce şu soruları sor:

```
[ ] Kullandığım enum değerleri gerçekten var mı? (Bölüm 4 > Enum Değerleri)
[ ] Implement ettiğim interface'in metodları ne? (interface dosyasını oku)
[ ] Bu DTO'nun alanları ne? (DTO dosyasını oku)
[ ] Entity'nin property adları doğru mu? (Entity dosyasını oku)
[ ] Bu kod hangi katmanda? Bağımlılık yönü doğru mu? (Dependency Graph)
[ ] Aynı şeyi yapan bir şey zaten var mı? (grep ile kontrol)
[ ] Bu kod ileride kullanılacak mı? (YAGNI — hayırsa yazma)
[ ] Namespace convention uygun mu? (Naming Convention)
```

---

### 0.7 Hızlı Proje Durum Testi (Session Başı)

Bağlam kaybedince çalıştır, sonuçları bu dosyayla karşılaştır:

```powershell
# 1. Build sağlıklı mı?
dotnet build WatchListScreening.sln 2>&1 | Select-Object -Last 3

# 2. Hangi controller'lar var?
Get-ChildItem src/WatchListScreening.API/Controllers | Select-Object Name

# 3. Hangi service'ler implement edilmiş?
Get-ChildItem src/WatchListScreening.Application/Services | Select-Object Name

# 4. DB migration durumu
dotnet ef migrations list -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API
```

---

### 0.8 Token Verimliliği Kuralları

```
YAPMA: Her promptta tüm dosyaları oku
YAP:   Bu strateji dosyasını oku, sadece gerektiğinde derinleş

YAPMA: "Sanırım SanctionEntry'de şu alan vardı" diye yaz
YAP:   Emin değilsen grep et veya dosyayı oku, sonra yaz

YAPMA: Çalışmayan kodu paylaş, kullanıcı hata alınca düzelt
YAP:   Kodu önce mantık olarak doğrula, sonra paylaş

YAPMA: Uzun dosyaları baştan sona oku
YAP:   view_file ile sadece ilgili satır aralığını oku

YAPMA: "bitti devam" denince direkt kodu gönder
YAP:   Önce build al, doğrula, sonra bir sonraki adıma geç
```

---

## 1. Tespit Edilen Sorunlar (Geçmiş Hatalar)

| Hata | Neden Oldu | Önlem |
|---|---|---|
| `ScreeningStatus.InProgress` (yok, Processing var) | Hafızaya güvenmek | Verify-First |
| `ApiResponse<T>` oluşturuldu ama kullanılmıyor | YAGNI ihlali | Önce kullanım senaryosu belirle |
| `IScreeningService` boş ve internal kaldı | "Var mı?" kontrolü yapılmadı | Red Flag Checklist |
| `MatchType` namespace çakışması | Tam qualified name kullanılmadı | 0.2 Quirks listesi |

---

## 2. Çözüm Stratejileri

| Strateji | Ne Zaman Kullanılır |
|---|---|
| **A: Canlı Proje Haritası** | Her session başında — bu dosyayı oku |
| **B: Verify-First** | Enum/Interface/DTO referans vermeden önce |
| **C: Grep ile Hedefli Arama** | "Bu var mı?" sorusu geldiğinde |
| **D: Kullanım Takip** | Yeni şey oluşturunca — nerede kullanılacak? |
| **E: Oturum Başı Rutin** | Session açılışında |
| **F: YAGNI** | "İleride lazım olur" diye kod yazmadan önce |
| **G: Açık Borç Takibi** | Yarım kalan her şeyi kayıt altına al |

---

## 3. Açık Teknik Borçlar (Güncel — 2026-08-15)

| Borç | Neden Önemli | Kapatılacak Yer |
|---|---|---|
| `ApiResponse<T>` kullanılmıyor | Tutarsız response formatı | 3.6 ExceptionMiddleware |
| `PagedResult<T>` kullanılmıyor | GetAll sınırsız kayıt dönüyor | 3.1 iyileştirme |
| RiskLevel eşikleri uyumsuz (95+ yerine 90+) | Yanlış risk hesaplama | ScreeningService fix |
| Dosya adlarında boşluk (3 dosya) | Linux CI/CD sorunu | Rename |
| `MatchedFullName` MapToDto'da boş | UI'da isim görünmez | Results Controller + Include() |
| Exception handling yok | 500 yerine 404 dönmeli | ExceptionMiddleware |
| Validation yok | Boş veri kabul ediliyor | DataAnnotations |
| CORS yok | MVC panel API'ye erişemez | Program.cs |

---

## 4. Proje Haritası (Güncel — 2026-08-15)

### Enum Değerleri — KRİTİK, HAFIZAYA GÜVENME

```
EntityType:      Person=1, Organization=2
ScreeningStatus: Pending=1, Processing=2, Completed=3, Failed=4
RiskLevel:       Low=1(<50), Medium=2(50-74), High=3(75-89), Critical=4(>=90)
ReviewStatus:    Pending=1, UnderReview=2, Approved=3, Confirmed=4, Escalated=5
MatchType:       Exact=1, Contains=2, Fuzzy=3, Phonetic=4
```

### Domain Katmanı — TAMAMLANDI ✅

```
SanctionEntry    — FullName, EntityType, Country, ListSource, IsActive, Aliases(JSON), DeactivatedAt
ScreeningRequest — SearchQuery, SearchType(EntityType!), RequestedBy, Status, IsBulk, TotalMatches, Results[]
ScreeningResult  — MatchScore(decimal5,2), MatchedType, RiskLevel, ReviewStatus, ReviewedBy, ReviewedAt
AuditLog         — Action, EntityType, EntityId, PerformedBy, PerformedAt
BaseEntity       — Id(int), CreatedAt, UpdatedAt
```

### Application Katmanı — DEVAM EDİYOR 🔄

```
IRepository<T>           → GetById, GetAll, Add, Update, Delete, Query()
ISanctionEntryRepository → + SearchByName, GetByListSource
IUnitOfWork              → 4 repo + SaveChangesAsync + Dispose

ISanctionEntryService → DOLU — GetAll, GetById, Create, Update, Delete, Search
IScreeningService     → DOLU — ScreenAsync, GetByIdAsync
IScreeningResultService, ICacheService, IAuditLogService → BOŞ (henüz gerekli değil)

SanctionEntryService → Tam ✅
ScreeningService     → Tam ✅ (Processing + doğru RiskLevel eşikleri)
MatchingEngine       → Exact + Contains + Fuzzy(Levenshtein) ✅

ApiResponse<T>  → VAR ama kullanılmıyor ⚠️
PagedResult<T>  → VAR ama kullanılmıyor ⚠️
```

### Infrastructure Katmanı — TAMAMLANDI ✅

```
AppDbContext        → 4 DbSet + ApplyConfigurations + SeedData.Seed()
Repository<T>       → Generic CRUD + Query()
SanctionEntryRepository → + SearchByName + GetByListSource
UnitOfWork          → 4 repo + SaveChanges + Dispose
AddInfrastructure() → DbContext + UnitOfWork(Scoped) + Repository<>(Scoped) + SanctionEntryRepo(Scoped)
```

### API Katmanı — DEVAM EDİYOR 🔄

```
SanctionEntriesController → GET all, GET/:id, POST, PUT/:id, DELETE/:id, GET/search ✅
ScreeningController       → POST screen, GET/:id ✅

Program.cs:
  AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)
  AddInfrastructure(config) + AddApplicationServices()
  AddControllers + AddOpenApi

EKSIK: ExceptionMiddleware, Swagger UI, CORS, Serilog, ResultsController
```

### Veritabanı — TAMAMLANDI ✅

```
Host: localhost:5432 | DB: watchlist_db | User: postgres | Pass: postgres123
Migrations: InitialCreate + SeedInitialData (10 kayıt)
Tablolar: SanctionEntries, ScreeningRequests, ScreeningResults, AuditLogs
```

---

## 5. Çalışma Kuralları

```
1. Bu dosyayı oku (tarihe bak — güncel mi?)
2. 04_FAZ1_GOREVLER.md sıradaki göreve bak
3. Red Flag Checklist'i geç (0.6)
4. Kod yazmadan önce ilgili dosyayı oku/grep et
5. Her modül sonunda build al
6. Borç listesini güncelle
7. Bu dosyayı güncelle
```

---

## 6. Potansiyel Kör Noktalar

| Sorun | Risk | Çözüm |
|---|---|---|
| Navigation property lazy load yok | MatchedFullName boş döner | .Include() eager load |
| Exception handling yok | KeyNotFoundException → 500 | ExceptionMiddleware |
| Validation yok | Boş FullName kabul | [Required] veya FluentValidation |
| CORS yok | MVC panel API'ye erişemez | Program.cs policy |
| IsBulk CreateDto'da yok | Bulk endpoint patlar | DTO güncelle |
| Dosya adlarında boşluk | Linux CI/CD | Rename |

---

## 7. Modül İlerleme Tablosu

### Faz 1

| Modül | Durum |
|---|---|
| 1.1–1.5 Altyapı, Domain, EF, Seed | ✅ TAMAM |
| 2.1–2.4 Interfaces, DTOs, Repository, DI | ✅ TAMAM |
| 3.1 SanctionEntries Controller | ✅ TEST EDİLDİ |
| 3.2 Screening Controller | ✅ YAZILDI |
| 3.3 Results Controller | ⬜ BEKLIYOR |
| 3.4 SanctionEntryService | ✅ TAMAM |
| 3.5 MatchingEngine + ScreeningService | ✅ TAMAM |
| 3.6 API Altyapısı (Swagger, CORS, Middleware) | 🔄 DEVAM |
| 4.x MVC Panel | ⬜ BAŞLANMADI |
| 5.x Redis Cache | ✅ TAMAM |
| 6.x RabbitMQ | ⬜ BAŞLANMADI |

### Faz 2 — List Harvester

| Modül | Durum |
|---|---|
| F2.1 Domain + DB (Entity, Enum, Migration) | ⬜ BAŞLANMADI |
| F2.2 Application (Interface, DTO, Mesaj) | ⬜ BAŞLANMADI |
| F2.3 Scraper Projesi Oluşturma | ⬜ BAŞLANMADI |
| F2.4 Veri Temizleme Pipeline | ⬜ BAŞLANMADI |
| F2.5 HTTP Scraper (HtmlAgilityPack) | ⬜ BAŞLANMADI |
| F2.6 Selenium Scraper | ⬜ BAŞLANMADI |
| F2.7 Api/File Scraper | ⬜ BAŞLANMADI |
| F2.8 ScraperFactory + HarvestWorker | ⬜ BAŞLANMADI |
| F2.9 RabbitMQ Entegrasyonu | ⬜ BAŞLANMADI |
| F2.10 Hangfire Entegrasyonu | ⬜ BAŞLANMADI |
| F2.11 API Endpoint'leri | ⬜ BAŞLANMADI |
| F2.12 MVC Panel Sayfaları | ⬜ BAŞLANMADI |
| F2.13 Entegrasyon Testi (12 kaynak) | ⬜ BAŞLANMADI |

