# ✅ Faz 1 — Görev Planı (PostgreSQL)

> Projenin Faz 1 görev listesi. Her modül sırayla tamamlanacak.
> Kodu sen yazacaksın — takıldığında yardım alacaksın.

---

## Modül 1: Proje Yapısı ve Veritabanı Kurulumu (⏱️ ~60 dk)

### 1.1 Docker Altyapısı
- [ ] `docker-compose.yml` oluştur:
  - PostgreSQL 16 (port: 5432, user: postgres, pass: postgres123, db: watchlist_db)
  - Redis 7 (port: 6379)
  - RabbitMQ 3 Management (ports: 5672 + 15672, user: guest, pass: guest)
- [ ] `docker-compose up -d` ile servisleri başlat
- [ ] Bağlantıları test et (psql, redis-cli, RabbitMQ management UI)

### 1.2 Solution ve Projeler
- [ ] `WatchListScreening.sln` oluştur
- [ ] 5 proje oluştur (Domain, Application, Infrastructure, API, Web)
- [ ] Proje referanslarını ekle (bağımlılık kuralına uygun)
- [ ] Build et — hata olmadığını doğrula

### 1.3 Domain Katmanı
- [ ] `BaseEntity.cs` oluştur (Id, CreatedAt, UpdatedAt)
- [ ] `SanctionEntry.cs` entity oluştur (tüm property'ler)
- [ ] `ScreeningRequest.cs` entity oluştur
- [ ] `ScreeningResult.cs` entity oluştur
- [ ] `AuditLog.cs` entity oluştur
- [ ] Enum'ları oluştur: EntityType, ScreeningStatus, RiskLevel, ReviewStatus

### 1.4 EF Core Kurulumu
- [ ] NuGet paketleri ekle: Npgsql.EntityFrameworkCore.PostgreSQL, EF Core Tools
- [ ] `AppDbContext.cs` oluştur (DbSet'ler)
- [ ] Fluent API konfigürasyonları yaz (her entity için ayrı dosya)
- [ ] `appsettings.json`'a connection string ekle
- [ ] İlk migration oluştur: `InitialCreate`
- [ ] Migration'ı uygula: `database update`

### 1.5 Seed Data
- [ ] `SeedData.cs` oluştur
- [ ] 10+ örnek yaptırım kaydı ekle (03_VERITABANI_TASARIMI.md'den)
- [ ] Seed migration oluştur ve uygula
- [ ] Veritabanında verileri doğrula

### 📝 Modül 1 Tamamlama Kontrol Listesi
- [ ] Docker servisleri çalışıyor mu?
- [ ] Solution build oluyor mu?
- [ ] Migration başarılı mı?
- [ ] Seed veriler veritabanında görünüyor mu?
- [ ] Git commit: `feat: initial project structure and database setup`

---

## Modül 2: Repository Pattern + Service Layer (⏱️ ~45 dk)

### 2.1 Application Katmanı — Interface'ler
- [ ] `IRepository<T>` generic interface (GetById, GetAll, Add, Update, Delete, Query)
- [ ] `ISanctionEntryRepository` — özelleşmiş repository (SearchByName, GetByListSource)
- [ ] `IUnitOfWork` interface
- [ ] Service interface'leri:
  - `ISanctionEntryService` (CRUD + Search)
  - `IScreeningService` (Screen, GetResults)
  - `IScreeningResultService` (UpdateReviewStatus, GetPendingReviews)
  - `IAuditLogService` (LogAction, GetLogs)
  - `ICacheService` (Get, Set, Remove)
  - `IMessagePublisher` (PublishScreeningRequest)

### 2.2 Application Katmanı — DTO'lar
- [ ] `SanctionEntryDto`, `CreateSanctionEntryDto`, `UpdateSanctionEntryDto`
- [ ] `ScreeningRequestDto`, `CreateScreeningRequestDto`
- [ ] `ScreeningResultDto`, `UpdateReviewDto`
- [ ] `DashboardStatsDto`
- [ ] `PagedResult<T>` generic pagination modeli
- [ ] `ApiResponse<T>` standard response wrapper

### 2.3 Infrastructure Katmanı — Repository Implementasyonları
- [ ] `Repository<T>` generic implementasyon
- [ ] `SanctionEntryRepository` özelleşmiş implementasyon
- [ ] `UnitOfWork` implementasyon

### 2.4 Dependency Injection Kayıtları
- [ ] Infrastructure DI extension metodu: `AddInfrastructureServices()`
- [ ] Repository kayıtları
- [ ] Service kayıtları (şimdilik sadece boş implementasyonlarla)
- [ ] API Program.cs'e DI ekle

### 📝 Modül 2 Tamamlama Kontrol Listesi
- [ ] Build başarılı mı?
- [ ] DI container düzgün çalışıyor mu? (uygulama başlıyor mu?)
- [ ] Interface-implementation eşleşmeleri doğru mu?
- [ ] Git commit: `feat: add repository pattern and service layer`

---

## Modül 3: Web API Endpoints (⏱️ ~60 dk)

### 3.1 SanctionEntries Controller
- [ ] `GET /api/sanctionentries` — Liste (sayfalama + filtreleme)
- [ ] `GET /api/sanctionentries/{id}` — Tek kayıt
- [ ] `POST /api/sanctionentries` — Yeni kayıt
- [ ] `PUT /api/sanctionentries/{id}` — Güncelle
- [ ] `DELETE /api/sanctionentries/{id}` — Soft delete (IsActive = false)
- [ ] `GET /api/sanctionentries/search?query=john&source=OFAC` — Arama

### 3.2 Screening Controller
- [ ] `POST /api/screening` — Tek isim taraması başlat → sonuç döndür
- [ ] `POST /api/screening/bulk` — Toplu tarama başlat (kuyruğa at)
- [ ] `GET /api/screening/{id}` — Tarama sonuçlarını getir
- [ ] `GET /api/screening/history` — Tarama geçmişi (sayfalama)

### 3.3 Results Controller
- [ ] `GET /api/results/pending` — İnceleme bekleyen sonuçlar
- [ ] `PUT /api/results/{id}/review` — İnceleme kararı (Approve/Confirm/Escalate)
- [ ] `GET /api/results/stats` — İstatistikler

### 3.4 SanctionEntryService Implementasyonu
- [ ] CRUD operasyonları
- [ ] Arama fonksiyonu (query parameter ile)
- [ ] Sayfalama mantığı
- [ ] AuditLog entegrasyonu (her işlemde log)

### 3.5 ScreeningService Implementasyonu — Matching Engine
- [ ] `MatchingEngine` sınıfı oluştur
- [ ] Exact match: String.Equals (case-insensitive)
- [ ] Contains match: String.Contains
- [ ] Fuzzy match: Levenshtein distance hesaplama
- [ ] MatchScore hesaplama (0-100)
- [ ] RiskLevel otomatik atama (score'a göre)
- [ ] Sonuçları veritabanına kaydet

### 3.6 API Altyapısı
- [ ] `ExceptionHandlingMiddleware` — global hata yönetimi
- [ ] **Swagger UI kurulumu** — `Swashbuckle.AspNetCore` paketi + UI arayüzü (OpenAPI JSON spec var, UI eksik)
- [ ] Swagger/OpenAPI konfigürasyonu (XML comments ile endpoint açıklamaları)
- [ ] Serilog konfigürasyonu
- [ ] CORS ayarları (MVC panelden erişim için)

### 📝 Modül 3 Tamamlama Kontrol Listesi
- [ ] Swagger UI'dan tüm endpoint'ler görünüyor mu?
- [ ] POST ile tarama yapılabiliyor mu?
- [ ] Matching engine doğru skor hesaplıyor mu?
- [ ] Exception middleware hataları düzgün yakalıyor mu?
- [ ] Git commit: `feat: add REST API with screening engine`

---

## Modül 4: MVC Panel — Razor + jQuery (⏱️ ~90 dk)

### 4.1 Layout ve Tema
- [ ] `_Layout.cshtml` — sidebar menü + header + content area
- [ ] Bootstrap 5 CDN ekleme
- [ ] Genel CSS düzenlemeleri (temiz ve profesyonel görünüm)
- [ ] jQuery ve Toastr CDN ekleme

### 4.2 Dashboard Sayfası (Home/Index)
- [ ] Özet kartlar: Toplam yaptırım kaydı, Bugünkü tarama sayısı, Bekleyen incelemeler, Yüksek risk eşleşmeleri
- [ ] jQuery AJAX ile API'den veri çekme
- [ ] Auto-refresh (30 saniyede bir)

### 4.3 Yaptırım Listesi Yönetimi (Sanction/Index)
- [ ] DataTable ile liste görüntüleme (sayfalama, arama, sıralama)
- [ ] jQuery AJAX ile API'ye CRUD istekleri
- [ ] Modal ile yeni kayıt ekleme / düzenleme
- [ ] Silme onay dialogu
- [ ] Toastr ile başarı/hata bildirimleri

### 4.4 Tarama Ekranı (Screening/Index)
- [ ] Form: İsim girişi + Entity Type seçimi
- [ ] "Tara" butonu → AJAX POST to API → Sonuçları göster
- [ ] Sonuç kartları: Her eşleşme için MatchScore, RiskLevel badge, kaynak bilgisi
- [ ] Renk kodlaması: Low=yeşil, Medium=sarı, High=turuncu, Critical=kırmızı

### 4.5 Sonuç İnceleme Ekranı (Screening/Results)
- [ ] Bekleyen incelemeleri listele
- [ ] Her sonuç için detay modal: eşleşen kayıt bilgileri, skor, geçmiş taramalar
- [ ] İnceleme aksiyonları: Approve / Confirm / Escalate butonları
- [ ] Review note girişi
- [ ] İnceleme sonrası liste güncellenmesi

### 4.6 Audit Log Sayfası (Audit/Index)
- [ ] Tüm işlem geçmişi tablosu
- [ ] Filtreleme: Tarih aralığı, işlem türü, kullanıcı
- [ ] Sayfalama

### 📝 Modül 4 Tamamlama Kontrol Listesi
- [ ] Dashboard verileri API'den doğru gelıyor mu?
- [ ] CRUD işlemleri çalışıyor mu?
- [ ] Tarama yapılabiliyor mu ve sonuçlar gösteriliyor mu?
- [ ] İnceleme aksiyonları veritabanını güncelliyor mu?
- [ ] Git commit: `feat: add MVC admin panel with jQuery`

---

## Modül 5: Redis Caching (⏱️ ~30 dk)

### 5.1 Redis Altyapısı
- [ ] StackExchange.Redis NuGet paketi ekle
- [ ] `ICacheService` interface (Application katmanı — zaten var)
- [ ] `RedisCacheService` implementasyonu (Infrastructure katmanı)
- [ ] DI kaydı: Singleton

### 5.2 Cache Senaryolarını Uygula
- [ ] Yaptırım listesi sorguları → cache'e al (TTL: 60 dk)
- [ ] Aynı isimle tekrar tarama → cache'ten dön (TTL: 15 dk)
- [ ] Dashboard istatistikleri → cache (TTL: 5 dk)
- [ ] Cache invalidation: Yaptırım kaydı CRUD işlemlerinden sonra

### 5.3 Service Katmanında Cache Entegrasyonu
- [ ] `SanctionEntryService`'te cache ekleme
- [ ] `ScreeningService`'te cache kontrol
- [ ] Dashboard service'te cache

### 📝 Modül 5 Tamamlama Kontrol Listesi
- [ ] Redis'e bağlanıyor mu?
- [ ] İlk sorgu DB'den, ikinci sorgu cache'ten mi geliyor? (loglardan kontrol)
- [ ] Cache invalidation çalışıyor mu?
- [ ] Git commit: `feat: add Redis caching layer`

---

## Modül 6: RabbitMQ ile Asenkron İşleme (⏱️ ~45 dk)

### 6.1 RabbitMQ Altyapısı
- [ ] RabbitMQ.Client NuGet paketi ekle
- [ ] `IMessagePublisher` interface (Application — zaten var)
- [ ] `RabbitMqPublisher` implementasyon (queue adı: "screening-requests")
- [ ] `RabbitMqConsumer` — BackgroundService olarak

### 6.2 Bulk Screening Akışı
- [ ] API endpoint: `POST /api/screening/bulk` — liste al, her item için mesaj publish et
- [ ] Message modeli: `ScreeningRequestMessage` (SearchQuery, SearchType, RequestedBy, RequestId)
- [ ] Consumer: Mesajı al → ScreeningService.ScreenAsync çağır → sonucu kaydet
- [ ] Status update: Pending → Processing → Completed/Failed

### 6.3 MVC Panel Entegrasyonu
- [ ] Toplu tarama başlatma butonu (basit bir textarea'ya isimleri alt alta yaz)
- [ ] İşlem durumu gösterimi (polling ile status kontrolü)
- [ ] Toastr ile "Toplu tarama başlatıldı" bildirimi

### 6.4 Hata Yönetimi
- [ ] Consumer'da try-catch + loglama
- [ ] Basit retry mekanizması (3 deneme, sonra fail)
- [ ] Failed mesajlar için loglama

### 📝 Modül 6 Tamamlama Kontrol Listesi
- [ ] RabbitMQ Management UI'da kuyruk görünüyor mu? (localhost:15672)
- [ ] Mesaj publish ediliyor mu?
- [ ] Consumer mesajları alıp işliyor mu?
- [ ] Hata durumunda retry çalışıyor mu?
- [ ] Git commit: `feat: add RabbitMQ bulk screening`

---

## Modül 7: Background Services (⏱️ ~30 dk)

### 7.1 Yaptırım Listesi Güncelleme Job'ı
- [ ] `ListUpdateJob` — BackgroundService veya Quartz.NET job
- [ ] Simüle: Her 5 dakikada bir "dış kaynaktan" rastgele yeni kayıtlar ekle
- [ ] Cache invalidation sonrası
- [ ] AuditLog kaydı: "System: List updated with X new entries"

### 7.2 Eski Tarama Temizliği
- [ ] `StaleScreeningCleanupJob` — 30 günden eski completed taramaları işaretle/sil
- [ ] Cron: Günde bir (development'ta test için her 2 dakikada bir)

### 7.3 Dashboard İstatistik Hesaplama
- [ ] `DashboardStatsJob` — Redis'e güncel istatistikleri push'la
- [ ] Her 5 dakikada bir çalışsın

### 📝 Modül 7 Tamamlama Kontrol Listesi
- [ ] Job'lar zamanında çalışıyor mu? (loglardan kontrol)
- [ ] Graceful shutdown çalışıyor mu? (Ctrl+C ile uygulama düzgün kapanıyor mu?)
- [ ] Git commit: `feat: add background jobs`

---

## Modül 8: Docker Compose ile Son Dokunuşlar (⏱️ ~20 dk)

### 8.1 Uygulama Dockerize
- [ ] API projesi için `Dockerfile`
- [ ] Web projesi için `Dockerfile`
- [ ] `docker-compose.yml`'e API ve Web servislerini ekle
- [ ] Environment variables ile config yönetimi

### 8.2 Tüm Sistemi Test Et
- [ ] `docker-compose up -d` ile tüm servisleri başlat
- [ ] API Swagger UI erişimi
- [ ] MVC Panel erişimi
- [ ] Tam akış testi: Tarama yap → Sonuçları gör → İncele → Onayla

### 📝 Modül 8 Tamamlama Kontrol Listesi
- [ ] Tüm container'lar healthy mi?
- [ ] API ve Web uygulamaları erişilebilir mi?
- [ ] End-to-end akış çalışıyor mu?
- [ ] Git commit: `feat: dockerize full application`

---

## 🏁 Faz 1 Tamamlama Kriterleri

Faz 1 başarıyla tamamlandığında:
- [ ] Bir isim taraması yapılabiliyor (API + UI)
- [ ] Yaptırım listesi yönetilebiliyor (CRUD)
- [ ] Tarama sonuçları incelenebiliyor (Approve/Confirm/Escalate)
- [ ] Dashboard istatistikleri görüntülenebiliyor
- [ ] Toplu tarama RabbitMQ üzerinden çalışıyor
- [ ] Redis cache devrede
- [ ] Background job'lar çalışıyor
- [ ] Audit log tutuluyor
- [ ] Tüm sistem Docker Compose ile ayağa kalkıyor
