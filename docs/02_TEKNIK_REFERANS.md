# 🔧 Teknik Referans: .NET 8, Patterns ve Araçlar

> Bu doküman, projede kullanacağımız .NET 8 teknolojilerini, design pattern'leri ve araçları açıklar.
> Her konseptin **ne olduğu, neden kullanıldığı ve nasıl kullanılacağı** açıklanır.

---

## 🏛️ Mimari: Clean Architecture

### Nedir?
Katmanlı mimari yaklaşımı. İç katmanlar dış katmanlardan bağımsızdır. Dependency yönü her zaman **dıştan içe** doğrudur.

### Neden Kullanıyoruz?
- Test edilebilirlik (servisler mock'lanabilir)
- Veritabanı değişikliği kolaylığı (PostgreSQL → SQL Server gibi)
- Bakım kolaylığı ve sorumluluk ayrımı

### Katman Yapısı
```
Domain (en iç)         → Entity'ler, Enum'lar, Value Object'ler
    ↑
Application            → Interface'ler, DTO'lar, Service contract'ları
    ↑
Infrastructure         → EF Core, Redis, RabbitMQ implementasyonları
    ↑
Presentation (en dış)  → API Controllers, MVC Controllers, Views
```

### Bağımlılık Kuralı
```
❌ Domain → Infrastructure (YASAK — domain altyapıya bağımlı olamaz)
✅ Infrastructure → Domain (Infrastructure, Domain'i bilir)
✅ API → Application (Controller, servis interface'ini bilir)
✅ Infrastructure → Application (Repository, interface'i implement eder)
```

---

## 📦 Proje Yapısı ve Namespace'ler

```
WatchListScreening.sln
│
├── src/
│   ├── WatchListScreening.Domain/           # Entity, Enum, ValueObject
│   │   ├── Entities/
│   │   │   ├── SanctionEntry.cs
│   │   │   ├── ScreeningRequest.cs
│   │   │   ├── ScreeningResult.cs
│   │   │   └── AuditLog.cs
│   │   ├── Enums/
│   │   │   ├── RiskLevel.cs
│   │   │   ├── ReviewStatus.cs
│   │   │   ├── EntityType.cs
│   │   │   └── ScreeningStatus.cs
│   │   └── Common/
│   │       └── BaseEntity.cs
│   │
│   ├── WatchListScreening.Application/      # Interface, DTO, Service contract
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── ISanctionEntryRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Services/
│   │   │   │   ├── ISanctionEntryService.cs
│   │   │   │   ├── IScreeningService.cs
│   │   │   │   ├── ICacheService.cs
│   │   │   │   └── IAuditLogService.cs
│   │   │   └── Messaging/
│   │   │       └── IMessagePublisher.cs
│   │   ├── Services/                        # Application (Business) Logic
│   │   │   ├── SanctionEntryService.cs
│   │   │   ├── ScreeningService.cs
│   │   │   ├── AuditLogService.cs
│   │   │   └── MatchingEngine.cs            # Fuzzy match algoritması
│   │   ├── DTOs/
│   │   │   ├── SanctionEntryDto.cs
│   │   │   ├── ScreeningRequestDto.cs
│   │   │   ├── ScreeningResultDto.cs
│   │   │   └── DashboardStatsDto.cs
│   │   └── Common/
│   │       ├── PagedResult.cs
│   │       └── ApiResponse.cs
│   │
│   ├── WatchListScreening.Infrastructure/   # Implementasyon
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/              # Fluent API
│   │   │   │   ├── SanctionEntryConfiguration.cs
│   │   │   │   └── ScreeningResultConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── Repository.cs
│   │   │   │   ├── SanctionEntryRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   └── Seed/
│   │   │       └── SeedData.cs
│   │   ├── Caching/
│   │   │   └── RedisCacheService.cs
│   │   ├── Messaging/
│   │   │   ├── RabbitMqPublisher.cs
│   │   │   └── RabbitMqConsumer.cs
│   │   └── BackgroundJobs/
│   │       ├── ListUpdateJob.cs
│   │       └── StaleScreeningCleanupJob.cs
│   │
│   ├── WatchListScreening.API/              # REST API
│   │   ├── Controllers/
│   │   │   ├── SanctionEntriesController.cs
│   │   │   ├── ScreeningController.cs
│   │   │   ├── ResultsController.cs
│   │   │   └── ReportsController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   └── Program.cs
│   │
│   └── WatchListScreening.Web/             # MVC Panel
│       ├── Controllers/
│       │   ├── HomeController.cs            # Dashboard
│       │   ├── SanctionController.cs        # Liste yönetimi
│       │   ├── ScreeningController.cs       # Tarama ekranı
│       │   └── AuditController.cs           # Audit log
│       ├── Views/
│       │   ├── Shared/
│       │   │   ├── _Layout.cshtml
│       │   │   └── _Notifications.cshtml
│       │   ├── Home/
│       │   │   └── Index.cshtml             # Dashboard
│       │   ├── Sanction/
│       │   │   ├── Index.cshtml             # Liste
│       │   │   └── _EditModal.cshtml        # Düzenle modal
│       │   ├── Screening/
│       │   │   ├── Index.cshtml             # Tarama formu
│       │   │   └── Results.cshtml           # Sonuçlar
│       │   └── Audit/
│       │       └── Index.cshtml
│       ├── wwwroot/
│       │   ├── css/
│       │   ├── js/
│       │   └── lib/
│       └── Program.cs
│
├── docker-compose.yml
├── .dockerignore
└── README.md
```

---

## 🔑 Design Patterns

### 1. Repository Pattern
**Ne:** Veri erişim mantığını soyutlayan katman.
**Neden:** Controller veya service'in veritabanı teknolojisini bilmesine gerek yok.

```csharp
// Interface (Application katmanı)
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> Query(); // Filtreleme için
}

// Implementasyon (Infrastructure katmanı)
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;
    
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<T?> GetByIdAsync(int id) 
        => await _dbSet.FindAsync(id);
    
    // ... diğer metodlar
}
```

### 2. Unit of Work Pattern
**Ne:** Birden fazla repository işlemini tek transaction'da yönetir.
**Neden:** İlişkili işlemlerin ya hep ya hiç (atomic) olmasını sağlar.

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<SanctionEntry> SanctionEntries { get; }
    IRepository<ScreeningRequest> ScreeningRequests { get; }
    IRepository<ScreeningResult> ScreeningResults { get; }
    IRepository<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync();
}
```

### 3. Strategy Pattern (Matching Engine)
**Ne:** Farklı eşleştirme algoritmalarını birbirinden bağımsız tanımlar.
**Neden:** Yeni bir eşleştirme yöntemi eklemek mevcut kodu değiştirmez.

```csharp
public interface IMatchStrategy
{
    string Name { get; }
    double CalculateScore(string input, string target);
}

public class ExactMatchStrategy : IMatchStrategy { ... }
public class FuzzyMatchStrategy : IMatchStrategy { ... }  // Levenshtein
public class ContainsMatchStrategy : IMatchStrategy { ... }
```

### 4. Dependency Injection (DI)
**Ne:** Bağımlılıkları dışarıdan enjekte etme.
**Neden:** Gevşek bağlama (loose coupling), test edilebilirlik.

```csharp
// Program.cs'te kayıt
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IScreeningService, ScreeningService>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
```

**Lifetime'lar:**
| Lifetime | Ne Zaman | Örnek |
|---|---|---|
| **Transient** | Her istendiğinde yeni instance | Lightweight, stateless servisler |
| **Scoped** | HTTP request başına bir instance | DbContext, Repository, UnitOfWork |
| **Singleton** | Uygulama ömrü boyunca tek instance | Cache service, RabbitMQ connection |

---

## 🗄️ Entity Framework Core — Temel İşlemler

### Migration Komutları
```bash
# Migration oluştur
dotnet ef migrations add InitialCreate -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# Migration uygula
dotnet ef database update -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# Migration geri al
dotnet ef migrations remove -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API
```

### Fluent API Konfigürasyon
```csharp
public class SanctionEntryConfiguration : IEntityTypeConfiguration<SanctionEntry>
{
    public void Configure(EntityTypeBuilder<SanctionEntry> builder)
    {
        builder.ToTable("SanctionEntries");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(e => e.Country)
            .HasMaxLength(100);
            
        builder.HasIndex(e => e.FullName);      // Sık aranan sütun
        builder.HasIndex(e => e.ListSource);     // Filtreleme için
        builder.HasIndex(e => e.IsActive);       // Aktif kayıtlar filtresi
    }
}
```

### Veritabanına Sütun Ekleme Senaryosu
Diyelim ki `SanctionEntry` tablosuna bir `NationalId` (kimlik numarası) sütunu eklememiz gerekti.

```csharp
// 1. Entity'ye property ekle
public class SanctionEntry : BaseEntity
{
    // ... mevcut property'ler
    
    /// <summary>
    /// Kişinin ulusal kimlik numarası. Bazı yaptırım listelerinde yer alır.
    /// İsim eşleşmesi sonrası doğrulama için kullanılabilir.
    /// Ekleme sebebi: OFAC listesi güncellemesinde bazı kayıtlarda national ID 
    /// bilgisi gelmeye başladı, eşleştirme doğruluğunu artırmak için eklendi.
    /// Tarih: 2024-08-04
    /// </summary>
    public string? NationalId { get; set; }
}

// 2. Fluent API'de configure et
builder.Property(e => e.NationalId)
    .HasMaxLength(50)
    .IsRequired(false);  // Nullable — her kayıtta olmayabilir

builder.HasIndex(e => e.NationalId)
    .HasFilter("[NationalId] IS NOT NULL");  // Sparse index

// 3. Migration oluştur
// dotnet ef migrations add AddNationalIdToSanctionEntry ...

// 4. Migration'ı uygula
// dotnet ef database update ...
```

> [!IMPORTANT]
> **Domain amacını her zaman belirt.** Bir sütun neden eklendi? Hangi iş gereksinimi karşılıyor? Bu bilgi hem kod review'da hem de takım içi iletişimde çok değerli.

---

## 🌐 ASP.NET Core Web API — Temel Yapılar

### Controller Yapısı
```csharp
[ApiController]
[Route("api/[controller]")]
public class ScreeningController : ControllerBase
{
    private readonly IScreeningService _screeningService;
    
    public ScreeningController(IScreeningService screeningService)
    {
        _screeningService = screeningService;
    }
    
    /// <summary>
    /// Yeni bir tarama isteği oluşturur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ScreeningResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateScreening([FromBody] CreateScreeningRequestDto request)
    {
        var result = await _screeningService.ScreenAsync(request);
        return Ok(ApiResponse<ScreeningResultDto>.Success(result));
    }
}
```

### Global Exception Handling
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(
                ApiResponse<object>.Fail("Beklenmeyen bir hata oluştu"));
        }
    }
}
```

### Pagination
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
```

---

## 🟥 Redis — Cache Kullanım Senaryoları

### Temel Kullanım
```csharp
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(30));
    }
    
    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
```

### Cache Senaryoları (Projemizde)
| Senaryo | Cache Key | TTL | Invalidation |
|---|---|---|---|
| Yaptırım listesi | `sanctions:all` | 1 saat | Liste güncellenince sil |
| Tek kayıt | `sanctions:{id}` | 30 dk | Kayıt güncellenince sil |
| Dashboard stats | `dashboard:stats` | 5 dk | Background job ile güncelle |
| Son tarama sonucu | `screening:{hash}` | 15 dk | Otomatik expire |

---

## 🐇 RabbitMQ — Mesaj Kuyruğu

### Temel Kavramlar
```
Producer (üretici)  → Exchange → Queue → Consumer (tüketici)
     API endpoint       ↑         ↑        Background service
                    routing    binding
```

### Projemizde Kullanım
```
Kullanıcı "Toplu Tarama" yapar (CSV yükler)
    → API, her satır için bir mesaj üretir (Producer)
    → Mesajlar kuyruğa gider (Queue: "screening-requests")
    → BackgroundService mesajları tek tek alır (Consumer)
    → Her mesajı işler → veritabanına yazar
    → İşlem durumu güncellenir (Pending → Processing → Completed)
```

### Neden Kuyruk?
- 10.000 kişilik listeyi **senkron** taramak → timeout, UI donması
- Kuyruğa at → arka planda işle → kullanıcı beklemez → sonuç hazır olunca göster

---

## 🕐 Background Services

### IHostedService vs BackgroundService
```
IHostedService        → StartAsync ve StopAsync'i kendin yaz
BackgroundService     → Sadece ExecuteAsync'i override et (daha kolay)
```

### Quartz.NET ile Zamanlanmış İşler
```csharp
// CRON expression örnekleri
"0 */30 * * * ?"    → Her 30 dakikada bir
"0 0 * * * ?"       → Her saat başı
"0 0 2 * * ?"       → Her gece saat 02:00'de
"0 0 0 * * SUN"     → Her pazar gece yarısı
```

---

## 🐳 Docker Cheat Sheet

```bash
# Container'ları başlat
docker-compose up -d

# Logları gör
docker-compose logs -f

# Belirli servisin logları
docker-compose logs -f postgres

# Container'ları durdur
docker-compose down

# Container'ları sil (volume dahil)
docker-compose down -v

# Rebuild
docker-compose up -d --build
```

---

## 📝 Sık Kullanılacak .NET CLI Komutları

```bash
# Solution oluştur
dotnet new sln -n WatchListScreening

# Proje oluştur
dotnet new classlib -n WatchListScreening.Domain -o src/WatchListScreening.Domain
dotnet new classlib -n WatchListScreening.Application -o src/WatchListScreening.Application
dotnet new classlib -n WatchListScreening.Infrastructure -o src/WatchListScreening.Infrastructure
dotnet new webapi -n WatchListScreening.API -o src/WatchListScreening.API
dotnet new mvc -n WatchListScreening.Web -o src/WatchListScreening.Web

# Solution'a ekle
dotnet sln add src/WatchListScreening.Domain
dotnet sln add src/WatchListScreening.Application
dotnet sln add src/WatchListScreening.Infrastructure
dotnet sln add src/WatchListScreening.API
dotnet sln add src/WatchListScreening.Web

# Proje referansı ekle
dotnet add src/WatchListScreening.Application reference src/WatchListScreening.Domain
dotnet add src/WatchListScreening.Infrastructure reference src/WatchListScreening.Application
dotnet add src/WatchListScreening.Infrastructure reference src/WatchListScreening.Domain
dotnet add src/WatchListScreening.API reference src/WatchListScreening.Application
dotnet add src/WatchListScreening.API reference src/WatchListScreening.Infrastructure
dotnet add src/WatchListScreening.Web reference src/WatchListScreening.Application
dotnet add src/WatchListScreening.Web reference src/WatchListScreening.Infrastructure

# NuGet paket ekle (örnekler)
dotnet add src/WatchListScreening.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/WatchListScreening.Infrastructure package StackExchange.Redis
dotnet add src/WatchListScreening.Infrastructure package RabbitMQ.Client
dotnet add src/WatchListScreening.Infrastructure package Quartz
dotnet add src/WatchListScreening.API package Serilog.AspNetCore
dotnet add src/WatchListScreening.API package Swashbuckle.AspNetCore

# Build
dotnet build

# Run
dotnet run --project src/WatchListScreening.API
dotnet run --project src/WatchListScreening.Web
```
