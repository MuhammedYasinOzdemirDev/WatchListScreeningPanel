# WatchList Screening Panel 🛡️

**AML (Anti-Money Laundering) Yaptırım Tarama Platformu**

Kişi ve kuruluşları uluslararası yaptırım listelerine karşı tarayan, sonuçları yöneten ve riskli eşleşmeleri izleyen bir compliance aracı.

---

## 🏗️ Mimari

- **Clean Architecture** — Domain → Application → Infrastructure → Presentation
- **ASP.NET Core Web API** — REST API
- **ASP.NET Core MVC** — Admin Panel (Razor + jQuery)
- **Entity Framework Core** — ORM (PostgreSQL)
- **Redis** — Distributed Cache
- **RabbitMQ** — Message Queue (Bulk Screening)
- **Serilog** — Structured Logging
- **Docker Compose** — Container Orchestration

## 📂 Proje Yapısı

```
WatchListScreening/
├── docs/                          # Proje dokümanları
│   ├── 00_PRENSIP_VE_TALIMATLAR.md
│   ├── 01_DOMAIN_BILGISI.md
│   ├── 02_TEKNIK_REFERANS.md
│   ├── 03_VERITABANI_TASARIMI.md
│   ├── 04_FAZ1_GOREVLER.md
│   ├── 05_FAZ2_YOLHARITASI.md
│   └── 06_KOMUT_REFERANSI.md
├── src/
│   ├── WatchListScreening.Domain/
│   ├── WatchListScreening.Application/
│   ├── WatchListScreening.Infrastructure/
│   ├── WatchListScreening.API/
│   └── WatchListScreening.Web/
├── docker-compose.yml
└── WatchListScreening.sln
```

## 🚀 Hızlı Başlangıç

```bash
# 1. Docker servislerini başlat
docker-compose up -d

# 2. Migration uygula
dotnet ef database update -p src/WatchListScreening.Infrastructure -s src/WatchListScreening.API

# 3. API'yi çalıştır
dotnet run --project src/WatchListScreening.API

# 4. Web paneli çalıştır
dotnet run --project src/WatchListScreening.Web --urls "http://localhost:5010"
```

## 📋 Fazlar

## 📋 Fazlar

| Faz | Durum | İçerik |
|---|---|---|
| Faz 1 | 🔄 Tamamlandı | PostgreSQL, tüm modüller, Docker, Temel Mimariler |
| Faz 2 | 🔄 Tamamlandı | ListHarvester (Scraper) Motoru, Hangfire, MassTransit, RabbitMQ, UI Paneli, Uçtan Uca Test |
| Faz 3+ | 💡 Gelecek | ElasticSearch (Tarama Motoru), K8s, CI/CD |

---

## 🗺️ Faz 2: Mimari & Veri Toplama (Scraper) Akışı

Sistem Faz 2 itibarıyla asenkron mikroservis mimarisine geçmiş, **Hangfire** ve **RabbitMQ** ile desteklenmiş sağlam bir **Veri Toplama (Scraper)** motoru kazanmıştır. "En ufak adımına kadar" detaylandırılmış sistem akışı aşağıda hem **Zaman Çizelgesi (Sequence)** hem de **Mimari Şema (Flowchart)** olarak sunulmuştur.

### 1. Sistem İşleyişi (Sequence Diagram - Adım Adım İlişkiler)
Aşağıdaki diyagram, bir yöneticinin kaynak eklemesinden verinin temizlenip kaydedilmesine kadar geçen tüm aşamaları (basit ve karmaşık tüm adımlarıyla) zaman sırasına göre gösterir:

```mermaid
sequenceDiagram
    autonumber
    actor Admin as Kullanıcı (Admin)
    participant Web as WatchList Web (MVC)
    participant API as WatchList API
    participant DB as PostgreSQL (DB)
    participant HF as Hangfire Scheduler
    participant MQ as RabbitMQ
    participant Worker as Scraper Worker
    participant Ext as Hedef Site (MASAK vb.)

    Admin->>Web: Kaynak formunu (Kategori, Cron vb.) doldurur ve kaydeder.
    Web->>API: HTTP POST /api/Sources
    API->>DB: Yeni ListSource (Kaynak) kaydını oluşturur.
    API->>HF: Kayıttaki Cron değerini okuyup Periyodik Job (Görev) tanımlar.
    API-->>Web: Kayıt Başarılı (201 Created) döner.
    Web-->>Admin: Ekranda "Başarılı" bildirimi gösterilir.

    Note over HF, MQ: Zamanı geldiğinde veya Panelden "Şimdi Tarat" denildiğinde:
    HF->>MQ: Kuyruğa (harvest-commands) 'HarvestCommandDto' mesajını fırlatır.
    
    MQ->>Worker: Worker (Consumer) mesajı yakalar ve uyanır.
    Worker->>Ext: HttpClient ile hedefe gider (Sayfayı / XML / JSON indirir).
    Ext-->>Worker: Ham (Kirli) veriyi geri döner.
    
    Note over Worker, Worker: 6 Aşamalı Pipeline Temizliği (Veri İşleme)
    Worker->>Worker: 1. HtmlDecoder: HTML taglarını soyar.
    Worker->>Worker: 2. UnicodeNormalizer: Türkçe/Arapça harfleri standartlaştırır.
    Worker->>Worker: 3. NameNormalizer: Fazla boşlukları, büyük/küçük harf uyumsuzluğunu giderir.
    Worker->>Worker: 4. CategoryClassifier: Kelimelerden Birey mi Kurum mu olduğunu anlar.
    Worker->>Worker: 5. HashGenerator: Verinin özgün (unique) hash'ini alır.
    Worker->>Worker: 6. NameParser: İsim, Göbek Adı ve Soyadı akıllıca ayırır.

    Worker->>DB: Temizlenen binlerce kaydı UnitOfWork ile DB'ye (SanctionEntries) basar.
    
    Worker->>MQ: Kuyruğa (harvest-results) Başarı veya Hata Event'i fırlatır.
    MQ->>API: API içindeki Consumer bu Event'i yakalar.
    API->>DB: Görevin Sonucunu, Hatasını ve Süresini 'ListSourceRuns' tablosuna kaydeder.
```

### 2. Mimari Bileşen Şeması (Flowchart)
Sistemin parçalarının birbiriyle fiziksel (altyapısal) ilişkilerini gösteren detaylı harita:

```mermaid
flowchart LR
    %% Kullanıcı ve Arayüz
    User((Kullanici)) -->|Istek Yapar| Web[MVC Web Paneli]
    Web -->|HTTP| API[WatchList REST API]
    
    %% API İşlemleri
    subgraph CoreBackend [API ve Planlama Katmani]
        API -->|Yaz/Oku| DB_Sources[(DB: ListSources)]
        API -->|Gorev Kaydet| Hangfire[Hangfire Zamanlayici]
        Consumer[HarvestResultConsumer] -->|Log Guncelle| DB_Runs[(DB: ListSourceRuns)]
    end

    %% Mesajlaşma
    Hangfire -->|Zamani Gelince Mesaj| RMQ_CMD[(RabbitMQ Commands)]
    RMQ_RES[(RabbitMQ Results)] -->|Sonuc Bildirimi| Consumer

    %% Worker İşlemleri
    subgraph DataScraping [Scraper ve Veri Hatti]
        RMQ_CMD --> Worker[Scraper Worker Daemon]
        Worker -->|1. Indir| Internet((Dis Kaynaklar))
        
        Worker -->|2. Filtrele| Pipeline[6 Asamali Veri Temizleme Pipeline]
        Pipeline -->|3. Kaydet| DB_Entries[(DB: SanctionEntries)]
    end

    Worker -->|Islem Bittiginde| RMQ_RES
```
