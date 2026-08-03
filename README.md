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

| Faz | Durum | İçerik |
|---|---|---|
| Faz 1 | 🔄 Aktif | PostgreSQL, tüm modüller, Docker |
| Faz 2 | 📋 Planlanmış | SQL Server migration, MassTransit, Polly, Tests |
| Faz 3+ | 💡 Gelecek | Kafka, Microservices, K8s, CI/CD |
