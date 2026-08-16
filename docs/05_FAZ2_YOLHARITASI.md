# 🕷️ Faz 2 — Otomatik Yaptırım Listesi Toplama Sistemi (List Harvester)

> Canlı kaynaklardan (web siteleri, resmi API'ler, dosya indirme) yaptırım/PEP/bireysel/kurumsal liste verilerini
> otomatik olarak toplayan, temizleyen, normalleştiren ve periyodik güncelleyen kapsamlı sistem.

---

## 📐 Mimari Genel Bakış

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         WEB PANELİ (MVC)                                     │
│                                                                              │
│  Kaynak Ekle/Yönet → Scraper Kodu Durumu → Periyot Ayarla → İzle/Yönet     │
│  Hangfire Dashboard embed → Manuel Tetikle → Çalışma Geçmişi → Çekilen Veri│
└─────────────────────────────────────┬────────────────────────────────────────┘
                                      │ HTTP API
┌─────────────────────────────────────▼────────────────────────────────────────┐
│                         API KATMANI                                          │
│  SourcesController   — Kaynak CRUD + durum sorgulama                        │
│  HarvestController   — Manuel tetikle + çalışma geçmişi                     │
│  HarvestedController — Çekilen ham verileri listele/onayla                   │
└─────────────────────────────────────┬────────────────────────────────────────┘
                                      │
          ┌───────────────────────────┼────────────────────────┐
          │                           │                        │
          ▼                           ▼                        ▼
┌─────────────────┐   ┌──────────────────────┐   ┌──────────────────────────┐
│  PostgreSQL DB  │   │     RabbitMQ          │   │    Hangfire               │
│                 │   │                       │   │    (PostgreSQL storage)   │
│ ListSources     │   │ harvest-commands →    │   │                          │
│ ListSourceRuns  │   │   → Scraper consume  │   │ Her kaynak kendi cron'u  │
│ HarvestedEntries│   │                       │   │ Runtime'da değiştir      │
│ SanctionEntries │   │ harvest-results →     │   │ Panelden yönet           │
│ AuditLogs       │   │   → API consume      │   │                          │
└─────────────────┘   └──────────────────────┘   └──────────────────────────┘
          ▲                                                │
          │                                                │
          │           ┌────────────────────────────────────▼──────────────────┐
          │           │        SCRAPER WORKER SERVİSİ (Ayrı Proje)            │
          │           │                                                       │
          │           │  ┌─ ScraperFactory (Factory Pattern)                  │
          │           │  │    Kayıt: ScraperType enum → ISourceScraper impl   │
          │           │  │                                                    │
          │           │  ├─ ISourceScraper (Strategy Pattern)                 │
          │           │  │    ├─ HttpScraper (HtmlAgilityPack)                │
          │           │  │    │    CSS Selector / XPath / sayfalama           │
          │           │  │    ├─ SeleniumScraper (Selenium WebDriver)         │
          │           │  │    │    Headless Chrome / bekleme / screenshot     │
          │           │  │    ├─ ApiScraper (HttpClient + JSON/XML parse)     │
          │           │  │    │    OFAC XML, EU CSV, UN XML indirme           │
          │           │  │    └─ FileScraper (dosya indirip parse et)         │
          │           │  │                                                    │
          │           │  ├─ IDataCleaner (Pipeline Pattern — Chain of Resp.)  │
          │           │  │    ├─ HtmlEntityDecoder (DeEntitize)               │
          │           │  │    ├─ UnicodeNormalizer (NFD→NFC, diacritics)      │
          │           │  │    ├─ NameNormalizer (unvan/suffix kaldır, trim)   │
          │           │  │    ├─ NameSplitter (FullName → First + Last)       │
          │           │  │    └─ DuplicateDetector (SHA256 hash)              │
          │           │  │                                                    │
          │           │  └─ CategoryClassifier                                │
          │           │       PEP / Individual / Corporate otomatik sınıfla   │
          │           │                                                       │
          └───────────│  Sonuç → DB kaydet → SanctionEntries güncelle         │
                      │  → Cache invalidate → AuditLog yaz                   │
                      └──────────────────────────────────────────────────────┘
```

---

## 🗄️ Veritabanı Tasarımı (Kapsamlı)

### ER Diyagramı (Yeni Tablolar)

```
┌─────────────────────────┐
│      ListSources        │       ┌─────────────────────────┐
├─────────────────────────┤       │    ListSourceRuns        │
│ Id (PK)                 │──1:N──│    (Çalışma Geçmişi)     │
│ Name                    │       ├─────────────────────────┤
│ Url                     │       │ Id (PK)                 │
│ Category (enum)         │       │ ListSourceId (FK)       │
│ ScraperType (enum)      │       │ StartedAt               │
│ ScraperConfig (JSON)    │       │ CompletedAt             │
│ CronExpression          │       │ Status (enum)           │
│ IsActive                │       │ TotalScraped            │
│ HasScraperImpl          │       │ TotalNew                │
│ ScraperClassName        │       │ TotalUpdated            │
│ LastHarvestAt           │       │ TotalSkipped            │
│ LastHarvestStatus       │       │ ErrorMessage            │
│ TotalRecordsHarvested   │       │ CreatedAt               │
│ Notes                   │       └────────────┬────────────┘
│ CreatedAt / UpdatedAt   │                    │ 1:N
└─────────────────────────┘                    │
                                  ┌────────────┴────────────┐
                                  │    HarvestedEntries      │
                                  │    (Çekilen Ham Veri)     │
                                  ├─────────────────────────┤
                                  │ Id (PK)                 │
                                  │ ListSourceId (FK)       │
                                  │ ListSourceRunId (FK)    │
                                  │ RawFullName             │
                                  │ CleanedFullName         │
                                  │ FirstName / LastName    │
                                  │ EntityType (enum)       │
                                  │ Category (enum)         │
                                  │ Country                 │
                                  │ DateOfBirth (string)    │
                                  │ NationalId              │
                                  │ Aliases (JSON)          │
                                  │ Passports (JSON)        │
                                  │ Addresses (JSON)        │
                                  │ AdditionalData (JSON)   │
                                  │ ContentHash (SHA256)    │
                                  │ IsProcessed             │
                                  │ ProcessedAt             │
                                  │ SanctionEntryId (FK?)   │
                                  │ CreatedAt               │
                                  └─────────────────────────┘
```

---

### 1. `ListSources` — Kaynak Tanımları

Web panelinden eklenip yönetilir. Her kaynak bir site/API/dosya kaynağıdır.

| Sütun | Tip | Zorunlu | Açıklama | Neden Var |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | Otomatik artan | Tekil tanımlayıcı |
| `Name` | varchar(300) | ✅ | "OFAC SDN List", "EU Sanctions", "MASAK Liste" | İnsan-okunur kaynak adı |
| `Url` | varchar(2000) | ✅ | Hedef URL | Scraper'ın gideceği adres |
| `Category` | smallint (enum) | ✅ | PEP / Individual / Corporate / Mixed | Hangi tipte veri çekiyor? Tarama filtreleme |
| `ScraperType` | smallint (enum) | ✅ | Http / Selenium / Api / File | Hangi teknikle veri çekilecek |
| `ScraperConfig` | text (JSON) | ❌ | CSS selectors, XPath'ler, sayfalama, alan eşleme kuralları | Her sitenin HTML yapısı farklı — JSON ile konfigüre et |
| `ScraperClassName` | varchar(200) | ❌ | "OfacSdnScraper", "MasakListeScraper" | Reflection ile doğru scraper sınıfını bul |
| `HasScraperImpl` | boolean | ✅ | Bu kaynak için scraper kodu yazıldı mı? | **Kullanıcının istediği kontrol** — kaynak eklendi ama kod yazılmadıysa çalıştırma |
| `CronExpression` | varchar(100) | ❌ | Hangfire cron: "0 */6 * * *" | Periyot — panelden değiştirilebilir |
| `IsActive` | boolean | ✅ | Kaynak aktif mi? | Pasif yapılınca Hangfire job durur |
| `LastHarvestAt` | timestamp | ❌ | Son başarılı çekme | "En son ne zaman güncellendi?" |
| `LastHarvestStatus` | smallint (enum) | ❌ | Success / Failed / PartialSuccess / Running | Son çalışma durumu |
| `TotalRecordsHarvested` | int | ❌ | Toplam çekilmiş kayıt | Kaynak ne kadar büyük? |
| `RetryCount` | int | ✅ (default 3) | Hata durumunda kaç kez denensin | Dayanıklılık |
| `TimeoutSeconds` | int | ✅ (default 120) | Timeout süresi | Selenium siteleri uzun sürebilir |
| `Notes` | text | ❌ | Kullanıcı notu | "Bu site bazen yavaş, dikkat" |
| `CreatedAt` | timestamp | ✅ | | |
| `UpdatedAt` | timestamp | ❌ | | |

#### `ScraperConfig` JSON Örneği
```json
{
  "tableSelector": "table.sanctions-list tbody tr",
  "nameColumn": "td:nth-child(2)",
  "countryColumn": "td:nth-child(4)",
  "dateOfBirthColumn": "td:nth-child(5)",
  "pagination": {
    "type": "url-param",
    "paramName": "page",
    "startPage": 1,
    "hasNextSelector": "a.next-page"
  },
  "waitForSelector": "#main-table",
  "scrollToLoad": false,
  "encoding": "utf-8"
}
```

> **HasScraperImpl Mantığı:** Panelden yeni bir kaynak eklendiğinde `HasScraperImpl = false` olur.
> Scraper kodu yazılıp deploy edildikten sonra `HasScraperImpl = true` yapılır.
> Hangfire job'ı **sadece HasScraperImpl = true olan kaynaklarda** çalışır.
> Bu sayede "kaynak eklendi ama kod yazılmadı" durumunda hata olmaz.

---

### 2. `ListSourceRuns` — Çalışma Geçmişi (Audit Trail)

Her veri çekme operasyonunun detaylı kaydı. Hangi run'da ne oldu, kaç kayıt eklendi/güncellendi?

| Sütun | Tip | Zorunlu | Açıklama | Neden Var |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | | |
| `ListSourceId` | int (FK) | ✅ | Hangi kaynaktan | Kaynak bazlı geçmiş |
| `TriggeredBy` | varchar(200) | ✅ | "Scheduler" / "Manual:admin" | Kim tetikledi? |
| `StartedAt` | timestamp | ✅ | Başlangıç zamanı | SLA takibi |
| `CompletedAt` | timestamp | ❌ | Bitiş zamanı | Süre hesaplama |
| `DurationMs` | long | ❌ | Süre (milisaniye) | Performans izleme |
| `Status` | smallint (enum) | ✅ | Running / Success / Failed / PartialSuccess | Mevcut durum |
| `TotalScraped` | int | ❌ | Siteden çekilen ham kayıt sayısı | "Kaç tane veri geldi?" |
| `TotalNew` | int | ❌ | Yeni eklenen (hash eşleşmedi) | "Kaç tanesi yeni?" |
| `TotalUpdated` | int | ❌ | Güncellenen mevcut kayıt | "Kaç tanesi güncellendi?" |
| `TotalSkipped` | int | ❌ | Hash eşleştiği için atlanan | "Kaç tanesi zaten vardı?" |
| `ErrorMessage` | text | ❌ | Hata mesajı | Debug için |
| `ErrorStackTrace` | text | ❌ | Stack trace | Detaylı debug |
| `CreatedAt` | timestamp | ✅ | | |

**Index'ler:**
- `IX_ListSourceRuns_ListSourceId` — kaynağa göre geçmiş
- `IX_ListSourceRuns_Status` — hatalı çalışmaları bul
- `IX_ListSourceRuns_StartedAt` — tarih bazlı sorgulama

---

### 3. `HarvestedEntries` — Çekilen Ham Veriler

Siteden çekilen ham veriyi saklar. **Direkt SanctionEntries'e yazmıyoruz** çünkü:
- Ham veri kirli olabilir → temizlenmeli
- Duplicate kontrolü → hash karşılaştırması
- Audit trail → "nereden geldi" izlenebilmeli
- Geri alma → yanlış veri geldiyse run bazında geri alınabilir

| Sütun | Tip | Zorunlu | Açıklama | Neden Var |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | | |
| `ListSourceId` | int (FK) | ✅ | Hangi kaynaktan | Kaynak takibi |
| `ListSourceRunId` | int (FK) | ✅ | Hangi çalışma sırasında | Run bazlı geri alma |
| `RawFullName` | varchar(500) | ✅ | Ham isim (temizlenmemiş) | Orijinal veri korunsun |
| `CleanedFullName` | varchar(500) | ❌ | Normalleştirilmiş isim | Eşleştirme bu alan üzerinden |
| `FirstName` | varchar(250) | ❌ | Ayrıştırılmış ad | Kısmi eşleştirme |
| `LastName` | varchar(250) | ❌ | Ayrıştırılmış soyad | Kısmi eşleştirme |
| `EntityType` | smallint (enum) | ❌ | Person / Organization | Kişi mi kuruluş mu |
| `Category` | smallint (enum) | ❌ | PEP / Individual / Corporate | Alt kategori |
| `Country` | varchar(100) | ❌ | Ülke | Coğrafi filtreleme |
| `DateOfBirth` | varchar(100) | ❌ | Doğum tarihi (string) | Siteler farklı format verir |
| `NationalId` | varchar(100) | ❌ | Kimlik no | Kesin doğrulama |
| `Aliases` | text (JSON) | ❌ | Takma adlar | Özellikle büyük kişilerin birden fazla adı |
| `Passports` | text (JSON) | ❌ | Pasaport numaraları | Yaptırım listelerinde sık geçer |
| `Addresses` | text (JSON) | ❌ | Adres bilgileri | Coğrafi doğrulama |
| `Positions` | text (JSON) | ❌ | Görev/pozisyon (PEP için) | "Eski Bakan", "Milletvekili" |
| `RelatedEntities` | text (JSON) | ❌ | İlişkili kişi/kuruluşlar | Ağ analizi |
| `SanctionProgram` | varchar(200) | ❌ | Hangi yaptırım programı (OFAC SDN, EU, vb.) | Detaylı kaynak bilgisi |
| `AdditionalData` | text (JSON) | ❌ | Siteden alınabilen diğer her şey | Yapısal olmayan veri |
| `ContentHash` | varchar(64) | ✅ | SHA256 hash | **Duplicate önleme** |
| `IsProcessed` | boolean | ✅ | SanctionEntries'e aktarıldı mı? | İşlem durumu |
| `ProcessedAt` | timestamp | ❌ | Ne zaman aktarıldı | Takip |
| `SanctionEntryId` | int (FK?) | ❌ | Eşleştirildiği SanctionEntry | Bağlantı |
| `CreatedAt` | timestamp | ✅ | | |

**Index'ler:**
- `IX_HarvestedEntries_ContentHash` (UNIQUE) — duplicate önleme (en kritik)
- `IX_HarvestedEntries_ListSourceId` — kaynağa göre filtreleme
- `IX_HarvestedEntries_IsProcessed` — işlenmemiş kayıtları bul
- `IX_HarvestedEntries_ListSourceRunId` — run bazlı geri alma

---

### Yeni Enum'lar

```csharp
public enum SourceCategory
{
    PEP = 1,           // Siyasi nüfuz sahibi kişiler (politically exposed persons)
    Individual = 2,     // Bireysel yaptırım (kişi bazlı)
    Corporate = 3,      // Kurumsal yaptırım (şirket/kuruluş)
    Mixed = 4           // Karışık (hepsi bir arada)
}

public enum ScraperType
{
    Http = 1,           // HtmlAgilityPack — statik HTML sayfalar
    Selenium = 2,       // Selenium WebDriver — JavaScript render siteleri
    Api = 3,            // HttpClient — JSON/XML API (OFAC, UN, EU resmi indirme)
    File = 4            // Dosya indirip parse et (CSV, XML dosyaları)
}

public enum HarvestStatus
{
    Running = 1,         // Çalışıyor
    Success = 2,         // Başarılı
    Failed = 3,          // Hata oluştu
    PartialSuccess = 4   // Kısmen başarılı (bazı sayfalar hatalı)
}
```

---

## 🔐 Hash Stratejisi (DB Sürekli Ekleme Önleme)

```
ContentHash = SHA256(
    CleanedFullName.ToLowerInvariant().Trim()
    + "|" + ListSourceId
    + "|" + (DateOfBirth ?? "")
    + "|" + (NationalId ?? "")
)
```

**Senaryo:**
```
İlk çalışma:  "Ali Hassan Mohammed" → Hash: abc123 → INSERT (yeni kayıt)
İkinci çalışma: "Ali Hassan Mohammed" → Hash: abc123 → SKIP (hash zaten var)
Üçüncü çalışma: "Ali Hassan Mohammed Jr." → Hash: def456 → INSERT (farklı hash)
```

**Güncelleme mantığı:**
- Hash eşleşti ama diğer alanlar değişti (ülke, adres vb.) → UPDATE mevcut kaydı
- Hash eşleşti ve hiçbir şey değişmedi → SKIP (hiçbir DB işlemi yapma)
- Hash eşleşmedi → INSERT yeni kayıt

---

## 🧹 İsim Temizleme Pipeline'ı (IDataCleaner)

Her adım bir `ICleaningStep` interface'i implement eder — Chain of Responsibility Pattern.

```
Adım 1: HtmlEntityDecoder
   "&amp;Ali  Hassan&nbsp;Mohammed" → "Ali  Hassan Mohammed"

Adım 2: InvisibleCharacterRemover
   "Ali\u200BHassan" → "Ali Hassan"  (zero-width space temizle)

Adım 3: UnicodeNormalizer
   "Müller" → NFD → NFC → "Muller" (opsiyonel, diacritic kaldır)
   Türkçe karakterler: "Ömer" → "Omer" (sadece hash için, orijinal korunur)

Adım 4: NameNormalizer
   "  DR.  ALİ   HASSAN  mohammed  (Jr.)  "
   → Trim + çoklu boşluk → "DR. ALİ HASSAN mohammed (Jr.)"
   → Unvan kaldır (Dr., Mr., Mrs., Jr., Sr., III, Sheikh, Ayatollah)
   → "ALİ HASSAN mohammed"
   → Title Case → "Ali Hassan Mohammed"

Adım 5: NameSplitter
   "Ali Hassan Mohammed"
   → FirstName: "Ali Hassan"
   → LastName: "Mohammed"
   (Kural: Son kelime soyadı, geri kalanı ad — kültürel farklılıklar için override)

Adım 6: DuplicateDetector
   → SHA256 hash oluştur
   → DB'de hash kontrolü yap
   → Sonuç: New / Update / Skip
```

---

## 🏗️ Proje Yapısı

### Neden Ayrı Proje: `WatchListScreening.Scraper`

| Sebep | Açıklama |
|---|---|
| Selenium ağır bağımlılık | ChromeDriver + Headless Chrome, API/Web'i şişirmemeli |
| Bağımsız deploy | Ayrı Docker container, farklı kaynak ihtiyacı (RAM, CPU) |
| Test edilebilirlik | `dotnet run --project Scraper` ile izole test |
| Hangfire Dashboard | Scraper projesinde ayrı dashboard UI (port 5020) |
| Farklı ölçekleme | Gerekirse birden fazla Scraper instance çalıştır |

### Solution Yapısı (Güncel)

```
WatchListScreening.sln
├── src/
│   ├── WatchListScreening.Domain/           # Entity: ListSource, HarvestedEntry eklenir
│   ├── WatchListScreening.Application/      # Interface + DTO + Service
│   │   ├── Interfaces/
│   │   │   ├── Scraping/
│   │   │   │   ├── ISourceScraper.cs        # Strategy: siteden veri çek
│   │   │   │   ├── IScraperFactory.cs       # Factory: ScraperType → Scraper instance
│   │   │   │   ├── IDataCleaner.cs          # Pipeline: ham veri → temiz veri
│   │   │   │   └── ICleaningStep.cs         # Pipeline adımı
│   │   │   └── Services/
│   │   │       ├── IListSourceService.cs    # Kaynak CRUD
│   │   │       └── IHarvestService.cs       # Harvest tetikle/sorgula
│   │   └── DTOs/
│   │       ├── ListSourceDto.cs
│   │       ├── CreateListSourceDto.cs
│   │       ├── HarvestRunDto.cs
│   │       └── HarvestedEntryDto.cs
│   │
│   ├── WatchListScreening.Infrastructure/   # EF, Repository, Redis, RabbitMQ
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   │   ├── ListSourceConfiguration.cs
│   │   │   │   ├── ListSourceRunConfiguration.cs
│   │   │   │   └── HarvestedEntryConfiguration.cs
│   │   │   └── Repositories/
│   │   │       ├── ListSourceRepository.cs
│   │   │       └── HarvestedEntryRepository.cs
│   │   └── Messaging/
│   │       ├── HarvestCommandPublisher.cs   # RabbitMQ'ya komut gönder
│   │       └── HarvestResultConsumer.cs     # RabbitMQ'dan sonuç al
│   │
│   ├── WatchListScreening.API/              # REST API
│   │   └── Controllers/
│   │       ├── SourcesController.cs
│   │       └── HarvestController.cs
│   │
│   ├── WatchListScreening.Web/              # MVC Panel
│   │   ├── Controllers/
│   │   │   ├── SourceController.cs
│   │   │   └── HarvestController.cs
│   │   └── Views/
│   │       ├── Source/
│   │       │   ├── Index.cshtml             # Kaynak listesi
│   │       │   ├── Create.cshtml            # Yeni kaynak ekle
│   │       │   └── Details.cshtml           # Çalışma geçmişi
│   │       └── Harvest/
│   │           └── Entries.cshtml           # Çekilen veriler
│   │
│   └── WatchListScreening.Scraper/          # ← YENİ Worker Service
│       ├── Scrapers/
│       │   ├── ScraperFactory.cs            # Factory Pattern: ScraperType → Scraper
│       │   ├── HttpScraper.cs               # HtmlAgilityPack ile
│       │   ├── SeleniumScraper.cs           # Selenium WebDriver ile
│       │   ├── ApiScraper.cs                # JSON/XML API (OFAC, UN)
│       │   └── FileScraper.cs               # Dosya indirip parse
│       ├── Cleaners/
│       │   ├── CleaningPipeline.cs          # Tüm adımları sırayla çalıştır
│       │   ├── HtmlEntityDecoder.cs
│       │   ├── InvisibleCharacterRemover.cs
│       │   ├── UnicodeNormalizer.cs
│       │   ├── NameNormalizer.cs
│       │   ├── NameSplitter.cs
│       │   └── DuplicateDetector.cs
│       ├── Workers/
│       │   └── HarvestWorker.cs             # RabbitMQ consumer — ana iş mantığı
│       ├── Jobs/
│       │   └── ScheduledHarvestJob.cs       # Hangfire recurring job
│       └── Program.cs                       # Host: Hangfire + RabbitMQ + DI
```

---

## 📡 İletişim Akışı (RabbitMQ + Hangfire)

### Senaryo 1: Otomatik (Periyodik)
```
1. Hangfire Timer tetiklenir (cron'a göre)
   → ScheduledHarvestJob çalışır
   → ListSource tablosundan aktif + HasScraperImpl=true olanları al
   → Her biri için "harvest-commands" kuyruğuna mesaj at

2. Mesaj formatı (harvest-commands):
   {
     "SourceId": 3,
     "RunId": 42,
     "TriggeredBy": "Scheduler",
     "Url": "https://scsanctions.un.org/consolidated/",
     "ScraperType": "Api",
     "ScraperConfig": { ... },
     "TimeoutSeconds": 120,
     "RetryCount": 3
   }

3. HarvestWorker (Scraper projesinde) → mesajı al
   → ScraperFactory.Create(ScraperType) → uygun scraper seç
   → Scraper.ScrapeAsync(url, config) → ham veri listesi
   → CleaningPipeline.Process(rawEntries) → temiz veri
   → DB'ye yaz (HarvestedEntries + SanctionEntries güncelle)
   → Cache invalidate ("sanctions:all", "dashboard:stats")
   → "harvest-results" kuyruğuna sonuç at

4. Sonuç formatı (harvest-results):
   {
     "RunId": 42,
     "SourceId": 3,
     "Status": "Success",
     "TotalScraped": 55,
     "TotalNew": 5,
     "TotalUpdated": 2,
     "TotalSkipped": 48,
     "DurationMs": 12340
   }

5. API tarafı → sonucu al → ListSourceRuns tablosunu güncelle
   → ListSources.LastHarvestAt güncelle → AuditLog yaz
```

### Senaryo 2: Manuel (Panelden)
```
1. Kullanıcı Web panelde "Şimdi Çek" butonuna tıklar
2. MVC Controller → API'ye POST /api/harvest/trigger/{sourceId}
3. API → ListSourceRun oluştur → "harvest-commands" kuyruğuna at
4. Geri kalan akış aynı (Senaryo 1, adım 3'ten devam)
```

---

## 🖥️ Panel Yönetimi (Web MVC)

### Kaynak Yönetimi Sayfası (`/Sources`)

| Özellik | Açıklama |
|---|---|
| Kaynak listesi | Tablo: Ad, URL, Kategori, Scraper Tipi, Son Çekme, Durum, Kod Durumu |
| Yeni kaynak ekle | Form: URL, Ad, Kategori, Scraper Tipi, Periyot (cron), Timeout |
| Düzenle | İnline veya modal form |
| Aktif/Pasif toggle | Hangfire job'ını da durdurur |
| Kod durumu göstergesi | 🟢 Scraper yazıldı / 🔴 Henüz yazılmadı (HasScraperImpl) |
| Manuel tetikle | "Şimdi Çek" butonu → RabbitMQ'ya komut |
| Çalışma geçmişi | Son 50 run: başlangıç, süre, yeni/güncellenen/atlanan sayıları |
| Periyot değiştirme | Cron expression'ı panelden değiştir → Hangfire runtime'da günceller |
| Hangfire Dashboard | Embedded iframe veya link → port 5020 |

### Çekilen Veriler Sayfası (`/Harvested`)

| Özellik | Açıklama |
|---|---|
| Ham veri listesi | İsim, kaynak, hash, durum, çekilme tarihi |
| İşlenmemiş filtre | Sadece IsProcessed = false olanlar |
| Detay modal | Tüm alanlar: aliases, passports, addresses, positions |
| Manuel onay/red | İşlenmemiş kaydı SanctionEntries'e aktar veya sil |

---

## 🌐 Teyit Edilmiş Veri Kaynakları (10+ Kaynak)

> Aşağıdaki kaynaklar araştırılmış ve doğrulanmıştır. Her kaynağın erişim yöntemi,
> scraper tipi ve çekilebilecek alanlar belirtilmiştir.

### Dosya/API Tabanlı Kaynaklar (ScraperType: Api veya File)

Bu kaynaklar resmi XML/CSV/JSON dosyaları sağlar. HTML scraping gerekmez.

| # | Kaynak | Gerçek URL | Format | ScraperType | Kategori | Çekilecek Alanlar |
|---|---|---|---|---|---|---|
| 1 | **OFAC SDN List** | `https://sanctionslistservice.ofac.treas.gov/` → SDN_ENHANCED.XML | XML | `File` | Individual + Corporate | FullName, FirstName, LastName, Aliases(AKA), Program, Identifiers(passport/national ID), Country, DateOfBirth, Remarks |
| 2 | **UN Consolidated List** | `https://scsanctions.un.org/resources/xml/en/consolidated.xml` | XML (statik HTML tablo da var) | `Api` | Individual + Corporate | FullName, Aliases, DateOfBirth, Nationality, Designation, ListType, Comments, Document references |
| 3 | **EU Financial Sanctions** | `https://webgate.ec.europa.eu/fsd/fsf/public/files/xmlFullSanctionsList_1_1/content` | XML | `File` | Individual + Corporate | FullName, BirthDate, Citizenship, Address, SubjectType, Regulation, Programme |
| 4 | **OpenSanctions** | `https://data.opensanctions.org/datasets/latest/default/entities.ftm.json` | JSON (bulk) | `Api` | PEP + Individual + Corporate | FullName, Aliases, BirthDate, Nationality, Topics(PEP/sanction/crime), Positions, Relationships, Source URL |
| 5 | **UK HM Treasury** | `https://assets.publishing.service.gov.uk/media/` (Consolidated List CSV) | CSV | `File` | Individual + Corporate | Name1-6, DOB, CountryOfBirth, Nationality, PassportDetails, GroupType, Programme |

> ✅ **Teyit:** Bu kaynaklar programatik olarak indirilebilir. Scraping gerekmez, direkt dosya indir + parse et.

### HTTP Tabanlı Kaynaklar (ScraperType: Http — HtmlAgilityPack ile)

Bu siteler statik HTML sunar, JavaScript render gerekmez. HtmlAgilityPack yeterlidir.

| # | Kaynak | Gerçek URL | HTML Yapısı | ScraperType | Kategori | Çekilecek Alanlar | Zorluk |
|---|---|---|---|---|---|---|---|
| 6 | **World Bank Debarred Firms** | `https://www.worldbank.org/en/projects-operations/procurement/debarred-firms` | HTML tablo (JS ile yükleniyor aslında — teyitte Selenium gerekebilir) | `Http` → test sonrası `Selenium` olabilir | Corporate | FirmName, Country, IneligibilityPeriod, Grounds, FromDate, ToDate | Orta — tablo yapısı düzgün ama JS kontrol gerek |
| 7 | **Australia DFAT Sanctions** | `https://www.dfat.gov.au/international-relations/security/sanctions/consolidated-list` | HTML tablo + CSV indirme linki | `File` | Individual + Corporate | Name, Type, Address, BirthDate, Nationality, ListingInfo | Kolay — CSV indirme mevcut |

> ⚠️ **DİKKAT:** World Bank sayfası teyitte JS ile veri yüklüyor olabilir. İlk test'te HttpClient ile dene, içerik boş gelirse Selenium'a geç.

### Selenium Tabanlı Kaynaklar (ScraperType: Selenium — JS render zorunlu)

Bu siteler JavaScript ile dinamik içerik yükler. HtmlAgilityPack ile veri alınamaz.

| # | Kaynak | Gerçek URL | Neden Selenium? | ScraperType | Kategori | Çekilecek Alanlar | Zorluk |
|---|---|---|---|---|---|---|---|
| 8 | **Interpol Red Notices** | `https://www.interpol.int/en/How-we-work/Notices/View-Red-Notices` | Arama formu + AJAX sonuçları, tüm içerik JS ile render edilir | `Selenium` | Individual | Name, Nationality, Age, ChargesDetails, IssuingCountry, Photo URL | Yüksek — AJAX pagination, JS render |
| 9 | **MASAK** | `https://masak.hmb.gov.tr/` → "Malvarlıkları Dondurulanlar" | Dinamik arama formu, AJAX ile veri yükleme, filtreleme gerekli | `Selenium` | Individual + Corporate | FullName, EntityType, Country, DecisionNumber, LegalBasis | Yüksek — dinamik form, AJAX, Türkçe encoding |
| 10 | **EU Sanctions Map** | `https://www.sanctionsmap.eu/#/main` | Tamamen SPA (Single Page Application), React/Angular | `Selenium` | Individual + Corporate | Name, EntityType, Programme, ListDate, Measures | Yüksek — SPA, arama API'si var (alternatif: arka plan API'sini bul) |

> ✅ **Teyit:** Interpol ve MASAK kesinlikle Selenium gerektirir — HTML içerikleri JavaScript ile render edilir.
> ⚠️ **MASAK özel not:** Browser Developer Tools → Network sekmesinden arka plandaki API endpoint'ini bulup direkt çağırmak daha sağlıklı olabilir.

### PEP (Siyasi Nüfuz Sahibi) Kaynakları

| # | Kaynak | Gerçek URL | Format | ScraperType | Çekilecek Alanlar | Not |
|---|---|---|---|---|---|---|
| 11 | **OpenSanctions PEPs** | `https://data.opensanctions.org/datasets/latest/peps/entities.ftm.json` | JSON | `Api` | FullName, BirthDate, Position, PoliticalParty, Country, StartDate/EndDate | Non-commercial ücretsiz |
| 12 | **EveryPolitician** | `https://everypolitician.org/` (GitHub data repo) | JSON/CSV | `File` | FullName, BirthDate, Gender, Party, Constituency, Chamber | Açık kaynak, sık güncellenmeyebilir |

### Kaynak Teyit Özet Tablosu

| # | Kaynak | URL Teyit | Erişim Test | ScraperType | Zorluk |
|---|---|---|---|---|---|
| 1 | OFAC SDN | ✅ sanctionslistservice.ofac.treas.gov | ✅ XML indirilebilir | File | Kolay |
| 2 | UN Consolidated | ✅ scsanctions.un.org | ✅ HTML + XML mevcut | Api | Kolay |
| 3 | EU Sanctions | ✅ webgate.ec.europa.eu | ⚠️ Auth gerekebilir | File | Orta |
| 4 | OpenSanctions | ✅ data.opensanctions.org | ✅ JSON indirilebilir | Api | Kolay |
| 5 | UK HM Treasury | ✅ assets.publishing.service.gov.uk | ✅ CSV indirilebilir | File | Kolay |
| 6 | World Bank | ✅ worldbank.org/debarred-firms | ⚠️ JS yüklemeli test gerek | Http/Selenium | Orta |
| 7 | Australia DFAT | ✅ dfat.gov.au | ✅ CSV indirme var | File | Kolay |
| 8 | Interpol | ✅ interpol.int/red-notices | ✅ JS render teyitli | Selenium | Yüksek |
| 9 | MASAK | ✅ masak.hmb.gov.tr | ✅ AJAX teyitli | Selenium | Yüksek |
| 10 | EU Sanctions Map | ✅ sanctionsmap.eu | ✅ SPA teyitli | Selenium | Yüksek |
| 11 | OpenSanctions PEPs | ✅ data.opensanctions.org/peps | ✅ JSON indirilebilir | Api | Kolay |
| 12 | EveryPolitician | ✅ everypolitician.org | ⚠️ Güncellik kontrol et | File | Kolay |

> **SONUÇ:** 12 kaynak, 4 farklı scraper tipi:
> - **File (5):** OFAC, EU, UK, DFAT, EveryPolitician
> - **Api (3):** UN, OpenSanctions, OpenSanctions PEPs
> - **Http (1):** World Bank (teyit sonrası Selenium olabilir)
> - **Selenium (3):** Interpol, MASAK, EU Sanctions Map

---

## ⚙️ Hangfire Detayları

### Neden Hangfire (Quartz değil)?

| Kriter | Hangfire | Quartz |
|---|---|---|
| Dashboard UI | ✅ Built-in, panelden izle | ❌ Yok |
| DB Storage | ✅ PostgreSQL destekler | ✅ Var ama daha zor |
| Runtime cron değiştirme | ✅ `AddOrUpdate` ile kolay | ❌ Restart gerekir |
| Retry mekanizması | ✅ Built-in, configurable | ❌ Manuel yazılmalı |
| Panelden yönetim | ✅ Dashboard'dan durdur/başlat | ❌ Kod gerektirir |

### Hangfire Job Kayıt Mantığı
```csharp
// Her aktif kaynak için ayrı Hangfire recurring job
foreach (var source in activeSources.Where(s => s.HasScraperImpl))
{
    RecurringJob.AddOrUpdate(
        $"harvest-source-{source.Id}",
        () => PublishHarvestCommand(source.Id),
        source.CronExpression ?? Cron.Daily()
    );
}
```

---

## 🔄 Mevcut Yapıyla Entegrasyon — Detaylı Etki Analizi

### SanctionEntry Entity Değişiklikleri

Mevcut `SanctionEntry` entity'sine **yeni alanlar eklenip**, gereksizleşenler **çıkarılıyor** (Normalizasyon):

```diff
 public class SanctionEntry : BaseEntity
 {
     // ... mevcut diğer alanlar aynen kalır ...
-    public string ListSource { get; set; } = null!;      // ÇIKARILIYOR (Normalizasyon)
-    public string? ListSourceUrl { get; set; }           // ÇIKARILIYOR (Yeni ListSource entity'sinde var)
+    public int ListSourceId { get; set; }                // FK — hangi kaynaktan geldi? (Artık zorunlu)
+    public ListSource ListSourceRef { get; set; } = null!;// Navigation property
     
     public ICollection<ScreeningResult> ScreeningResults { get; set; } = ...;
+    public ICollection<HarvestedEntry> HarvestedEntries { get; set; } = ...;
 }
```

### Diğer Entity'lerdeki Tekrarlayan (Redundant) Alanların Temizlenmesi

Bu projede tüm entity'ler `BaseEntity`'den türer ve `BaseEntity` içerisinde `CreatedAt` alanı zaten vardır. Bu nedenle bazı sınıflardaki tarih alanları tamamen gereksizdir ve veritabanı temizliği (Clean Code / Normalizasyon) kapsamında **SİLİNECEKTİR**:

```diff
 public class ScreeningRequest : BaseEntity
 {
     public string SearchQuery { get; set; } = null!;
     public string RequestedBy { get; set; } = null!;
-    public DateTime RequestedAt { get; set; }            // ÇIKARILIYOR (BaseEntity.CreatedAt kullanılacak)
     public DateTime? CompletedAt { get; set; }
     // ...
 }

 public class AuditLog : BaseEntity
 {
     public string Action { get; set; } = null!;
     public string PerformedBy { get; set; } = null!;
-    public DateTime PerformedAt { get; set; }            // ÇIKARILIYOR (BaseEntity.CreatedAt kullanılacak)
     // ...
 }
```

> **Not:** `ScreeningResult` entity'si kontrol edildi; `ReviewedAt` alanı (incelemenin sonradan yapıldığı zamanı tuttuğu için) gereklidir, bir fazlalık yoktur.

### DTO ve Endpoint Etkileri

| Etkilenen Sınıf | Değişiklik | Detay |
|---|---|---|
| `CreateSanctionEntryDto` | `ListSource` ve `Url` siliniyor | Yerine `int ListSourceId` ekleniyor. |
| `SanctionEntryDto` | `ListSource` siliniyor | Yerine `string SourceName` (Navigation'dan) ve `ListSourceId` ekleniyor. |
| `ScreeningRequestDto` | `RequestedAt` siliniyor | Yerine `CreatedAt` mapplenecek. |
| `SanctionEntriesController` | Arama Endpointi Güncelleniyor | `GET /search?source=OFAC` yerine `GET /search?sourceId=1` formatına geçiyor. |
| `SanctionEntryService` | `CreateAsync` mantığı | Artık string kaydetmek yerine verilen `ListSourceId`'nin DB'de varlığını kontrol edecek. |

### Servis Katmanı Temizliği (YAGNI & Düzenleme)

Gereksiz açılmış veya YAGNI (You Aren't Gonna Need It) prensibini ihlal eden servisler silinecektir:

| Servis / Dosya | Durum | Karar |
|---|---|---|
| `IAuditLogService` | İçi tamamen boş ve `internal` kalmış. | 🗑️ **SİLİNECEK.** Audit loglama işlemleri ayrı bir servis üzerinden değil, doğrudan `SaveChanges` interception (veya MediatR event) ile merkezi yapılacaktır. |
| `SanctionEntryService .cs` | Dosya adında boşluk (space) var. | ✏️ **YENİDEN ADLANDIRILACAK.** Linux/Docker ortamlarında sorun çıkmasını engellemek için. |
| `ApiResponse<T>` | Projede var ancak controller'larda kullanılmıyor. | 🗑️ **SİLİNECEK.** (Veya kullanılacaksa standartlaştırılacak, şu an karmaşa yaratıyor). |

### IUnitOfWork Değişiklikleri

Mevcut `IUnitOfWork` interface'ine 3 yeni repository ekleniyor:

```diff
 public interface IUnitOfWork : IDisposable
 {
     ISanctionEntryRepository SanctionEntries { get; }
     IRepository<ScreeningRequest> ScreeningRequests { get; }
     IScreeningResultRepository ScreeningResults { get; }
     IRepository<AuditLog> AuditLogs { get; }
+    IListSourceRepository ListSources { get; }
+    IRepository<ListSourceRun> ListSourceRuns { get; }
+    IHarvestedEntryRepository HarvestedEntries { get; }
     Task<int> SaveChangesAsync();
 }
```

### AppDbContext Değişiklikleri

```diff
 public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
 {
     public DbSet<SanctionEntry> SanctionEntries => Set<SanctionEntry>();
     public DbSet<ScreeningRequest> ScreeningRequests => Set<ScreeningRequest>();
     public DbSet<ScreeningResult> ScreeningResults => Set<ScreeningResult>();
     public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
+    public DbSet<ListSource> ListSources => Set<ListSource>();
+    public DbSet<ListSourceRun> ListSourceRuns => Set<ListSourceRun>();
+    public DbSet<HarvestedEntry> HarvestedEntries => Set<HarvestedEntry>();
 }
```

### DependencyInjection.cs Değişiklikleri

```diff
 // Repository'ler
 services.AddScoped<IUnitOfWork, UnitOfWork>();
 services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
 services.AddScoped<ISanctionEntryRepository, SanctionEntryRepository>();
 services.AddScoped<IScreeningResultRepository, ScreeningResultRepository>();
+services.AddScoped<IListSourceRepository, ListSourceRepository>();
+services.AddScoped<IHarvestedEntryRepository, HarvestedEntryRepository>();
```

### DB İlişki Haritası (Yeni + Eski Tablolar)

```
┌─────────────┐    1:N    ┌──────────────┐    1:N    ┌─────────────────┐
│ ListSources │──────────▶│ListSourceRuns│──────────▶│HarvestedEntries │
└──────┬──────┘           └──────────────┘           └────────┬────────┘
       │ 1:N                                                  │
       │                                                      │ N:1 (opsiyonel)
       ▼                                                      ▼
┌──────────────┐                                     ┌────────────────┐
│SanctionEntries│◀────────────────────────────────────│ SanctionEntryId│
│ (mevcut tablo)│     HarvestedEntry.SanctionEntryId  └────────────────┘
└──────┬───────┘
       │ 1:N (mevcut ilişki — değişmez)
       ▼
┌──────────────────┐
│ScreeningResults  │
│ (mevcut tablo)   │
└──────────────────┘
```

### Mevcut Servis Etkileri

| Mevcut Servis | Değişiklik | Detay |
|---|---|---|
| `SanctionEntryService` | **Az değişiklik** | `CreateAsync` harici kaynak ile çağrıldığında `ListSourceId` set et |
| `ScreeningService` | **Değişmez** | Screening mantığı aynen kalır, kaynak nereden gelirse gelsin |
| `ScreeningResultService` | **Değişmez** | Sadece mevcut SanctionEntry'ler üzerinden çalışır |
| `MatchingEngine` | **Değişmez** | Fuzzy matching algoritması kaynaktan bağımsız |
| `ICacheService` | **Değişmez** | Aynı `sanctions:all` key'i, harvest sonrası invalidate |

---

## 🖥️ Web Panel Sayfaları — Detaylı Tasarım

### Mevcut Navigasyon (Sidebar) Güncellenmesi

Mevcut `_Layout.cshtml` sidebar'ına 2 yeni menü ekleniyor:

```diff
 <ul class="sidebar-nav">
     <li>Dashboard</li>
     <li>Tarama (Screening)</li>
     <li>İncelemeler (Pending)</li>
     <li>Yaptırım Listesi</li>
+    <li class="nav-divider">──── Harvester ────</li>
+    <li><i class="fa-solid fa-globe"></i> Veri Kaynakları</li>
+    <li><i class="fa-solid fa-download"></i> Çekilen Veriler</li>
 </ul>
```

### Sayfa 1: Veri Kaynakları (`/Source/Index`)

**Tablo Yapısı:**

| Sütun | Açıklama | Görsel |
|---|---|---|
| Kaynak Adı | Link → Details sayfası | "OFAC SDN List" |
| URL | Kısaltılmış link | treasury.gov/... |
| Kategori | Badge renkleri | 🟣 PEP / 🔵 Individual / 🟢 Corporate |
| Scraper Tipi | İkon | 🌐 Http / 🖥️ Selenium / 📡 Api / 📁 File |
| Kod Durumu | **Kritik** | 🟢 Hazır / 🔴 Kod Yazılmadı |
| Son Çekme | Relative time | "2 saat önce" |
| Durum | Son çalışma | ✅ Başarılı / ❌ Hatalı / 🔄 Çalışıyor |
| Toplam Kayıt | Sayı | "1,247" |
| Periyot | Cron açıklaması | "Her 6 saatte" |
| Aksiyonlar | Butonlar | [▶ Çek] [✏️ Düzenle] [⏸️ Durdur] |

**Üst Kısım:**
- "Yeni Kaynak Ekle" butonu → Create sayfasına yönlendir
- Filtre: Kategori, Durum, Scraper Tipi
- Arama: Kaynak adı ve URL'de ara

### Sayfa 2: Kaynak Ekleme Formu (`/Source/Create`)

| Alan | Tip | Zorunlu | Açıklama |
|---|---|---|---|
| Kaynak Adı | text input | ✅ | "OFAC SDN List" |
| URL | url input | ✅ | "https://..." |
| Kategori | select/dropdown | ✅ | PEP / Individual / Corporate / Mixed |
| Scraper Tipi | select/dropdown | ✅ | Http / Selenium / Api / File |
| Periyot | cron builder veya preset | ❌ | "Her 6 saatte" / "Günde bir" / Özel cron |
| Timeout (sn) | number input | ❌ | Varsayılan: 120 |
| Retry Sayısı | number input | ❌ | Varsayılan: 3 |
| Not | textarea | ❌ | Serbest metin |

> **DİKKAT:** Form submit edildiğinde `HasScraperImpl = false` olarak kaydedilir.
> Scraper kodu yazılıp deploy edildikten sonra detay sayfasından `HasScraperImpl = true` yapılır.

### Sayfa 3: Kaynak Detay (`/Source/Details/{id}`)

**3 bölümden oluşur:**

**Bölüm A: Kaynak Bilgileri** — Üstte kart
- Ad, URL, Kategori, Scraper Tipi, Periyot, Aktif/Pasif
- Düzenle butonu (modal)
- Scraper Kodu Durumu toggle (HasScraperImpl)

**Bölüm B: Çalışma Geçmişi** — Ortada tablo (ListSourceRuns)
- DataTables ile son 50 run
- Her satır: Başlangıç, Süre, Durum, Yeni/Güncellenen/Atlanan, Tetikleyen
- Hatalı satırlar kırmızı arka plan
- Detay modal: ErrorMessage + StackTrace

**Bölüm C: Kaynak İstatistikleri** — Altta mini dashboard
- Toplam çekilen kayıt, son 7 gün trend, ortalama çalışma süresi, hata oranı

### Sayfa 4: Çekilen Veriler (`/Harvest/Entries`)

**Tablo Yapısı:**

| Sütun | Açıklama |
|---|---|
| Ham İsim | RawFullName |
| Temiz İsim | CleanedFullName |
| Kaynak | ListSource.Name (badge) |
| Kategori | PEP / Individual / Corporate |
| Ülke | Country |
| Hash | İlk 8 karakter |
| İşlendi? | ✅ / ⏳ |
| İşlem Tarihi | ProcessedAt |
| Aksiyon | [👁️ Detay] [✅ Onayla] [❌ Reddet] |

**Detay Modal (👁️):** Tüm alanlar — Aliases, Passports, Addresses, Positions, AdditionalData (JSON formatted)

**Filtreler:**
- Kaynak seç (dropdown)
- Sadece İşlenmemişler (toggle)
- Tarih aralığı

### Dashboard Güncellemeleri (Home/Index)

Mevcut dashboard kartlarına **1 yeni kart** ekleniyor:

```diff
 Kart 1: Toplam Yaptırım Kaydı      (mevcut)
 Kart 2: Bugünkü Taramalar           (mevcut)
 Kart 3: Bekleyen İncelemeler        (mevcut)
 Kart 4: Yüksek Riskli Eşleşmeler   (mevcut)
+Kart 5: Aktif Veri Kaynakları       (yeni — ListSources sayısı)
+Kart 6: Son Harvest Durumu          (yeni — son 24 saat: başarılı/hatalı)
```

---

## 📋 Modül Yapısı (Adım Adım — Detaylı Açıklamalar)

> **Her modülde dikkat:** 06_AGENT_STRATEJILERI.md'deki Red Flag Checklist'i geç.
> Her adımda build al. Entity property adlarını hafızaya güvenme, dosyadan doğrula.

---

### Modül F2.1: Domain + DB Altyapısı (~45 dk)

**Ne yapılıyor:** 3 yeni entity, 3 yeni enum, Fluent API config, migration.

- [ ] **`SourceCategory` enum oluştur** — `Domain/Enums/SourceCategory.cs`
  - PEP=1, Individual=2, Corporate=3, Mixed=4
  - ⚠️ Mevcut `EntityType` enum ile karıştırma! EntityType Person/Organization, SourceCategory PEP/Individual/Corporate

- [ ] **`ScraperType` enum oluştur** — `Domain/Enums/ScraperType.cs`
  - Http=1, Selenium=2, Api=3, File=4

- [ ] **`HarvestStatus` enum oluştur** — `Domain/Enums/HarvestStatus.cs`
  - Running=1, Success=2, Failed=3, PartialSuccess=4
  - ⚠️ Mevcut `ScreeningStatus` ile karıştırma! İsimleri farklı ama yapıları benzer.

- [ ] **`ListSource` entity oluştur** — `Domain/Entities/ListSource.cs`
  - BaseEntity'den türet (Id, CreatedAt, UpdatedAt otomatik gelir)
  - Navigation: `ICollection<ListSourceRun> Runs`, `ICollection<HarvestedEntry> HarvestedEntries`
  - ⚠️ `ListSource` class adı ile `SanctionEntry.ListSource` string property çakışması!
  - → Entity adını değiştirmeye gerek yok, namespace ile ayrışır.
  - → Ancak kodda dikkatli ol: `using WatchListScreening.Domain.Entities;` ile doğru tipi al.

- [ ] **`ListSourceRun` entity oluştur** — `Domain/Entities/ListSourceRun.cs`
  - BaseEntity'den türet
  - FK: `ListSourceId` → ListSource
  - Navigation: `ListSource ListSource`

- [ ] **`HarvestedEntry` entity oluştur** — `Domain/Entities/HarvestedEntry.cs`
  - BaseEntity'den türet
  - FK: `ListSourceId` → ListSource, `ListSourceRunId` → ListSourceRun
  - FK (opsiyonel): `SanctionEntryId` → SanctionEntry
  - ⚠️ `ContentHash` alanı UNIQUE index olmalı — migration'da kontrol et
  - ⚠️ `DateOfBirth` string (varchar), SanctionEntry'deki `DateOfBirth` DateOnly — farklı tipler! Bilerek.

- [ ] **`SanctionEntry` entity'sine FK ekle**
  - `public int? ListSourceId { get; set; }` — nullable çünkü mevcut kayıtlarda yok
  - `public ListSource? ListSourceRef { get; set; }` — navigation
  - ⚠️ Property adı `ListSourceRef` — çünkü `ListSource` string zaten var!

- [ ] **Fluent API konfigürasyonları** — `Infrastructure/Data/Configurations/`
  - `ListSourceConfiguration.cs` — tablo adı "ListSources", index'ler
  - `ListSourceRunConfiguration.cs` — FK ilişki, cascade delete kuralı
  - `HarvestedEntryConfiguration.cs` — ContentHash UNIQUE index **en kritik**
  - ⚠️ Cascade delete dikkat: ListSource silinince Run'lar silinsin mi? → Evet (Cascade)
  - ⚠️ HarvestedEntry silinince SanctionEntry silinMESİN → SetNull

- [ ] **`IUnitOfWork`'e 3 yeni property ekle**

- [ ] **`UnitOfWork .cs` güncelle** (⚠️ dosya adında boşluk var!)
  - 3 yeni repository lazy property ekle

- [ ] **`AppDbContext .cs` güncelle** (⚠️ dosya adında boşluk var!)
  - 3 yeni DbSet ekle

- [ ] **Migration oluştur:**
  ```bash
  dotnet ef migrations add AddHarvesterTables -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API
  ```
  - ⚠️ Migration SQL'ini incele: `ContentHash` UNIQUE mi? FK'ler doğru mu?

- [ ] **Migration uygula:**
  ```bash
  dotnet ef database update -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API
  ```

- [ ] **Doğrulama:**
  ```bash
  docker exec watchlist-postgres psql -U postgres -d watchlist_db -c "\dt"
  ```
  - ListSources, ListSourceRuns, HarvestedEntries tabloları görülmeli

---

### Modül F2.2: Application Katmanı (~30 dk)

**Ne yapılıyor:** Interface'ler, DTO'lar, servis tanımları. Henüz implementasyon yok.

- [ ] **`IListSourceRepository` interface** — `Application/Interfaces/Repositories/`
  - `Task<IEnumerable<ListSource>> GetActiveWithScraperAsync()` — HasScraperImpl=true olanlar
  - `Task<ListSource?> GetByIdWithRunsAsync(int id)` — Include(Runs) ile
  - ⚠️ Include() Application'da YASAK — özel repository metodu yaz, Infrastructure'da Include()

- [ ] **`IHarvestedEntryRepository` interface**
  - `Task<HarvestedEntry?> GetByHashAsync(string contentHash)` — duplicate kontrol
  - `Task<IEnumerable<HarvestedEntry>> GetUnprocessedAsync()` — IsProcessed=false
  - `Task<IEnumerable<HarvestedEntry>> GetByRunIdAsync(int runId)` — run bazlı listeleme

- [ ] **Scraping interface'leri** — `Application/Interfaces/Scraping/`
  - `ISourceScraper` — `Task<List<RawScrapedItem>> ScrapeAsync(string url, string? config, CancellationToken ct)`
  - `IScraperFactory` — `ISourceScraper Create(ScraperType type)`
  - `IDataCleaner` — `CleanedItem Clean(RawScrapedItem raw)`
  - `ICleaningStep` — `string Process(string input)`
  - ⚠️ Bu interface'ler Application katmanında — Selenium, HtmlAgilityPack referansı YASAK
  - ⚠️ `RawScrapedItem`, `CleanedItem` DTO'lar burada tanımlanır

- [ ] **Servis interface'leri** — `Application/Interfaces/Services/`
  - `IListSourceService` — CRUD + GetActiveAsync + GetByIdWithHistoryAsync
  - `IHarvestService` — TriggerAsync, GetRunStatusAsync, ProcessHarvestedAsync

- [ ] **DTO'lar** — `Application/DTOs/`
  - `ListSourceDto`, `CreateListSourceDto`, `UpdateListSourceDto`
  - `ListSourceRunDto`
  - `HarvestedEntryDto`, `HarvestedEntryDetailDto` (aliases, passports vb.)
  - `RawScrapedItem` — scraper'dan gelen ham veri
  - `CleanedItem` — temizlenmiş veri
  - `HarvestCommandMessage`, `HarvestResultMessage` — RabbitMQ mesajları

- [ ] **Doğrulama:** `dotnet build WatchListScreening.sln` — 0 hata

---

### Modül F2.3: Scraper Projesi Oluşturma (~20 dk)

**Ne yapılıyor:** Yeni Worker Service projesi, solution'a ekleme, referanslar, NuGet.

- [ ] **Proje oluştur:**
  ```bash
  dotnet new worker -n WatchListScreening.Scraper -o src/WatchListScreening.Scraper
  ```

- [ ] **Solution'a ekle:**
  ```bash
  dotnet sln WatchListScreening.sln add src/WatchListScreening.Scraper
  ```

- [ ] **Referanslar bağla:**
  ```bash
  dotnet add src/WatchListScreening.Scraper reference src/WatchListScreening.Domain
  dotnet add src/WatchListScreening.Scraper reference src/WatchListScreening.Application
  dotnet add src/WatchListScreening.Scraper reference src/WatchListScreening.Infrastructure
  ```
  - ⚠️ Dependency yönü: Scraper → Application + Infrastructure + Domain (doğru)
  - ⚠️ Application → Scraper referansı YASAK (ters bağımlılık)

- [ ] **NuGet paketleri:**
  ```bash
  dotnet add src/WatchListScreening.Scraper package HtmlAgilityPack
  dotnet add src/WatchListScreening.Scraper package Selenium.WebDriver
  dotnet add src/WatchListScreening.Scraper package Selenium.WebDriver.ChromeDriver
  dotnet add src/WatchListScreening.Scraper package Hangfire
  dotnet add src/WatchListScreening.Scraper package Hangfire.PostgreSql
  dotnet add src/WatchListScreening.Scraper package RabbitMQ.Client
  dotnet add src/WatchListScreening.Scraper package Serilog.AspNetCore
  dotnet add src/WatchListScreening.Scraper package Serilog.Sinks.Console
  ```

- [ ] **Program.cs temel iskelet** — DI, Hangfire, RabbitMQ consumer, Serilog
  - ⚠️ `AddInfrastructure(config)` çağır — DbContext, Redis, Repository'ler gelsin
  - ⚠️ `AddApplicationServices()` çağır — Service'ler gelsin
  - ⚠️ Port: 5020 (API 5256, Web 5010, Scraper 5020)

- [ ] **Doğrulama:** `dotnet build WatchListScreening.sln` — 0 hata

---

### Modül F2.4: Veri Temizleme Pipeline (~30 dk)

**Ne yapılıyor:** İsim normalizasyonu ve duplicate tespiti. Scraper'dan bağımsız test edilebilir.

- [ ] **`ICleaningStep` interface** — `Application/Interfaces/Scraping/`
  ```csharp
  public interface ICleaningStep
  {
      string Process(string input);
      int Order { get; } // Çalışma sırası
  }
  ```

- [ ] **`HtmlEntityDecoder`** — `&amp;` → `&`, `&#39;` → `'`
  - ⚠️ HtmlAgilityPack'in `HtmlEntity.DeEntitize()` kullan — Scraper projesinde
  - Alternatif: `System.Net.WebUtility.HtmlDecode()` — Application'da kullanılabilir

- [ ] **`InvisibleCharacterRemover`** — Zero-width space, control char temizle
  - `Regex.Replace(input, @"\p{C}", "")` — tüm control karakterleri kaldır

- [ ] **`UnicodeNormalizer`** — NFD → NFC, diacritic kaldır
  - ⚠️ Hash için diacritic kaldır ama orijinali `RawFullName`'de koru
  - `string.Normalize(NormalizationForm.FormD)` + `CharUnicodeInfo.GetUnicodeCategory` filtrele

- [ ] **`NameNormalizer`** — Unvan/suffix kaldır, trim, Title Case
  - Unvan listesi: Dr., Mr., Mrs., Ms., Prof., Sheikh, Ayatollah, Jr., Sr., III, IV
  - ⚠️ "Dr. Ali" → "Ali" ama "André" → "André" (unvan değil!)

- [ ] **`NameSplitter`** — FullName → FirstName + LastName
  - Kural: Son kelime = soyad, geri kalan = ad
  - ⚠️ Arap/Türk isimleri: "Ali Hassan Mohammed" → First: "Ali Hassan", Last: "Mohammed"
  - ⚠️ Tek kelime: "Madonna" → First: "", Last: "Madonna"

- [ ] **`DuplicateDetector`** — SHA256 hash oluştur + DB'de kontrol
  - Input: CleanedFullName + ListSourceId + DateOfBirth + NationalId
  - ⚠️ Hash null-safe olmalı — `(DateOfBirth ?? "")` kullan

- [ ] **`CleaningPipeline`** — Tüm step'leri Order'a göre sıralayıp çalıştır

- [ ] **Doğrulama:** Console app veya unit test ile şu inputları test et:
  ```
  "  DR. ALİ   HASSAN  mohammed  (Jr.) " → "Ali Hassan Mohammed"
  "Müller&amp;Son" → "Muller Son" (hash için) / "Müller&Son" (orijinal)
  "Ali\u200BHassan" → "Ali Hassan"
  ```

---

### Modül F2.5: HTTP Scraper — HtmlAgilityPack (~30 dk)

**Ne yapılıyor:** Statik HTML sayfaları için scraper.

- [ ] **`HttpScraper.cs`** — `ISourceScraper` implement et
  - `HttpClient` + `HtmlDocument.LoadHtml()`
  - ⚠️ HttpClient'ı constructor'da al, DI'dan inject et — her çağrıda yeni yaratma
  - ⚠️ User-Agent header set et — bazı siteler bot'ları engeller

- [ ] **ScraperConfig JSON parse** — `System.Text.Json.JsonDocument`
  - `tableSelector`, `nameColumn`, `countryColumn` vb. oku
  - ⚠️ Config boş/null olabilir — varsayılan değerler belirle

- [ ] **Sayfalama (pagination)**
  - URL parametreli: `?page=1`, `?page=2`
  - "Next" butonu selector: `hasNextSelector` config'ten oku
  - ⚠️ Sonsuz döngü koruması: max 100 sayfa limiti

- [ ] **Encoding**
  - ⚠️ MASAK gibi Türkçe siteler `Windows-1254` kullanabilir
  - `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` çağır

- [ ] **Retry** — 3 deneme, her arada 2 saniye bekle

- [ ] **Doğrulama:** World Bank Debarred Firms sayfasını test et (basit HTML tablo)

---

### Modül F2.6: Selenium Scraper (~30 dk)

**Ne yapılıyor:** JavaScript ile render edilen siteler için scraper.

- [ ] **`SeleniumScraper.cs`** — `ISourceScraper` implement et
  - ⚠️ Headless Chrome: `options.AddArgument("--headless")`, `"--no-sandbox"`, `"--disable-gpu"`
  - ⚠️ `IDisposable` implement et — `driver.Quit()` finally'de çağrılmalı
  - ⚠️ Docker'da Chrome: Selenium Grid veya standalone-chrome container gerekebilir

- [ ] **WebDriverWait** — element yüklenene kadar bekle
  - `WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(d => d.FindElement(By.CssSelector(...)))`
  - ⚠️ `config.waitForSelector` varsa o selector'ı bekle

- [ ] **Sayfa kaydırma (infinite scroll)**
  - `driver.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)")`
  - Yeni element yüklenene kadar tekrarla

- [ ] **Screenshot** — hata durumunda debug
  - `driver.GetScreenshot().SaveAsFile($"error_{runId}.png")`
  - Logs klasörüne kaydet

- [ ] **Doğrulama:** Interpol Red Notices sayfasını test et (JS render)

---

### Modül F2.7: Api/File Scraper (~20 dk)

**Ne yapılıyor:** Resmi kaynakların XML/CSV/JSON dosyalarını indirip parse et.

- [ ] **`ApiScraper.cs`** — JSON/XML API
  - HttpClient ile GET, response body'yi parse et
  - ⚠️ OFAC SDN XML çok büyük (~30MB) — StreamReader ile oku, tamamını belleğe alma

- [ ] **`FileScraper.cs`** — CSV/XML dosya indir
  - Temp klasöre indir → parse et → sil
  - ⚠️ UN XML namespace'leri var — `XmlNamespaceManager` kullan

- [ ] **OFAC SDN XML parser** — `<sdnEntry>` elementlerini parse et
  - `<firstName>`, `<lastName>`, `<program>`, `<idList>`, `<akaList>`

- [ ] **Doğrulama:** OFAC SDN XML'ini indir, ilk 10 kaydı parse et

---

### Modül F2.8: ScraperFactory + HarvestWorker (~20 dk)

**Ne yapılıyor:** Factory pattern ile scraper seçimi, ana iş akışı.

- [ ] **`ScraperFactory.cs`** — DI'dan tüm scraper'ları al, ScraperType'a göre seç
  ```csharp
  // Constructor'da Dictionary<ScraperType, ISourceScraper> oluştur
  // Create(type) → dictionary'den dön
  ```
  - ⚠️ Bilinmeyen ScraperType → `NotSupportedException` fırlat

- [ ] **`HarvestWorker.cs`** — RabbitMQ consumer, ana iş mantığı
  - Mesajı al → Factory'den scraper seç → Scrape → Clean → Hash → DB yaz → Cache invalidate → Sonuç at
  - ⚠️ Tüm akış try-catch içinde — hata olursa ListSourceRun.Status = Failed + ErrorMessage
  - ⚠️ CancellationToken propagate et — graceful shutdown

- [ ] **Doğrulama:** RabbitMQ'ya manuel mesaj at, worker'ın çalışıp log yazdığını gör

---

### Modül F2.9: RabbitMQ Entegrasyonu (~20 dk)

**Ne yapılıyor:** API ↔ Scraper arası mesajlaşma.

- [ ] **`HarvestCommandPublisher`** — `Infrastructure/Messaging/`
  - `harvest-commands` exchange + queue oluştur
  - ⚠️ Exchange tipi: direct (point-to-point)
  - ⚠️ Message persistence: true (RabbitMQ restart'ta kaybolmasın)

- [ ] **`HarvestResultConsumer`** — API tarafında sonuç al
  - `harvest-results` queue'dan consume et
  - ListSourceRun tablosunu güncelle
  - ⚠️ Consumer API projesinde çalışır (Background Service olarak)

- [ ] **Doğrulama:** http://localhost:15672 → Queues → harvest-commands, harvest-results görünmeli

---

### Modül F2.10: Hangfire Entegrasyonu (~20 dk)

**Ne yapılıyor:** Periyodik job zamanlayıcı, dashboard.

- [ ] **NuGet:** `Hangfire.PostgreSql` (Scraper projesinde)

- [ ] **Storage konfigürasyonu:**
  ```csharp
  services.AddHangfire(config => config.UsePostgreSqlStorage(connectionString));
  services.AddHangfireServer();
  ```
  - ⚠️ Hangfire kendi tablolarını oluşturur — migration gerekmez
  - ⚠️ Aynı PostgreSQL DB'yi kullan, ayrı DB gereksiz

- [ ] **`ScheduledHarvestJob`** — DB'den aktif kaynakları oku, her biri için mesaj at
  - ⚠️ `HasScraperImpl == false` olanları ATLA — bu kontrolü burada yap

- [ ] **Dashboard:** `app.UseHangfireDashboard("/hangfire")` — port 5020'de
  - ⚠️ Production'da dashboard auth ekle (şimdilik gerek yok)

- [ ] **Runtime cron değiştirme:** `IRecurringJobManager.AddOrUpdate()` ile mevcut job güncelle
  - ⚠️ JobId tutarlı olmalı: `harvest-source-{sourceId}` formatı

- [ ] **Doğrulama:** http://localhost:5020/hangfire → Recurring Jobs'ta kayıtlı job'lar

---

### Modül F2.11: API Endpoint'leri (~20 dk)

**Ne yapılıyor:** Kaynak yönetimi ve harvest tetikleme API'leri.

- [ ] **`SourcesController`** — `api/sources`
  - GET / — tüm kaynaklar
  - GET /{id} — detay + son 10 run
  - POST / — yeni kaynak ekle (HasScraperImpl = false)
  - PUT /{id} — güncelle (cron değişirse Hangfire güncelle)
  - DELETE /{id} — soft delete (IsActive = false, Hangfire job kaldır)
  - PUT /{id}/scraper-status — HasScraperImpl toggle (scraper kodu yazıldı mı?)
  - ⚠️ Naming convention: `SourcesController` (çoğul, mevcut pattern)

- [ ] **`HarvestController`** — `api/harvest`
  - POST /trigger/{sourceId} — manuel tetikle
  - GET /runs/{sourceId} — çalışma geçmişi
  - GET /entries?sourceId=X&processed=false — çekilen veriler
  - POST /entries/{id}/process — manuel onayla (SanctionEntries'e aktar)
  - ⚠️ trigger endpoint HasScraperImpl kontrolü yapsın

- [ ] **Doğrulama:** Swagger UI'dan her endpoint'i test et

---

### Modül F2.12: MVC Panel Sayfaları (~30 dk)

**Ne yapılıyor:** Web paneline kaynak yönetimi ve çekilen veri sayfaları.

- [ ] **`_Layout.cshtml` güncelle** — Sidebar'a 2 yeni menü
  - "Veri Kaynakları" (fa-globe icon)
  - "Çekilen Veriler" (fa-download icon)
  - ⚠️ Mevcut sidebar yapısı ve CSS class'larını kullan — tutarlılık

- [ ] **`SourceController.cs`** — Web MVC controller
  - Index, Create, Details, Edit action'ları
  - ⚠️ API'yi çağır (HttpClient), direkt DB'ye gitme — MVC → API → DB

- [ ] **`HarvestController.cs`** (Web) — Çekilen veriler sayfası
  - ⚠️ API'deki HarvestController ile karıştırma! Namespace farklı: `.Web.Controllers`

- [ ] **View'lar:**
  - `Views/Source/Index.cshtml` — DataTables, mevcut Sanction/Index tarzı
  - `Views/Source/Create.cshtml` — Form, mevcut Sanction/Create tarzı
  - `Views/Source/Details.cshtml` — Kart + DataTables, mevcut Screening/Details tarzı
  - `Views/Harvest/Entries.cshtml` — DataTables, filtreleme
  - ⚠️ Mevcut `site.css`'teki class'ları kullan: `.custom-card`, `.glass-panel`, `.dashboard-stat-card`

- [ ] **Dashboard güncelle** — `Views/Home/Index.cshtml`
  - 2 yeni stat kartı ekle (API'den veri çek)

- [ ] **Doğrulama:** Panelden kaynak ekle → listede gör → detay sayfasına git

---

### Modül F2.13: 10 Gerçek Kaynak ile Entegrasyon Testi (~45 dk)

**Ne yapılıyor:** Tüm pipeline'ı gerçek verilerle uçtan uca test.

- [ ] **10 kaynak URL'si DB'ye ekle** (panelden veya seed data)

- [ ] **Her kaynak için scraper konfigürasyonu (ScraperConfig JSON) yaz**
  - ⚠️ Her sitenin HTML yapısı farklı — CSS selector'lar siteye özel

- [ ] **Tüm pipeline'ı çalıştır:**
  1. Panelden "Şimdi Çek" butonuna tıkla
  2. RabbitMQ'da mesaj oluştu mu? (Management UI kontrol)
  3. Scraper worker çalıştı mı? (loglar kontrol)
  4. HarvestedEntries tablosunda veri var mı?
  5. SanctionEntries tablosuna aktarıldı mı?
  6. Redis cache invalidate edildi mi?

- [ ] **Duplicate testi:** Aynı kaynağı iki kez çek
  - İkinci çalışmada TotalNew = 0, TotalSkipped = N olmalı
  - DB'de duplicate kayıt OLMAMALI

- [ ] **Periyodik test:** Hangfire job'ını bekle, otomatik çalıştığını doğrula

---

## ⚠️ Kritik Kurallar ve Dikkat Noktaları

1. **HasScraperImpl kontrolü** — Kaynak eklendi ama scraper kodu yazılmadıysa job çalışmasın
2. **Hash-first yaklaşım** — Her INSERT'ten önce hash kontrol et, DB'yi şişirme
3. **IDataCleaner pipeline'ı zorunlu** — Ham veri asla direkt DB'ye yazılmasın
4. **Factory Pattern** — Yeni scraper tipi eklemek için sadece yeni sınıf yaz, mevcut kodu değiştirme
5. **RabbitMQ ile async** — Scraping uzun sürer, API thread'i bloklanmasın
6. **AuditLog her adımda** — Her harvest run, her kaynak değişikliği loglanmalı
7. **Dosya adlarında boşluk** — `AppDbContext .cs`, `UnitOfWork .cs` — düzenlerken dikkat
8. **Katman kuralı** — Application'da EF Core, Selenium, HtmlAgilityPack using'i YASAK
9. **Nullable FK** — `SanctionEntry.ListSourceId` nullable olmalı (mevcut kayıtlar için)
10. **Port çakışması** — API:5256, Web:5010, Scraper:5020 — çakışmasın

---

## 🔧 docker-compose.yml Güncellemesi

Mevcut docker-compose'a ekleme gerekmez — PostgreSQL, Redis, RabbitMQ zaten var.
Scraper projesi ayrı terminal'de `dotnet run` ile çalıştırılacak (dev ortamı).
Production'da Scraper için ayrı Dockerfile + docker-compose service eklenir.
