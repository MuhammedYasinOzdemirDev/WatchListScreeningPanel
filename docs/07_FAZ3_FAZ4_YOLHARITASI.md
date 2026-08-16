# 🚀 Faz 3 — Teknik Derinlik ve Kalite (Eski Faz 2)

> Faz 1 + Faz 2 tamamlandıktan sonra projeyi production-ready seviyeye çıkarmak için planlanan geliştirmeler.

---

## 🔄 SQL Server'a Migration

### Amaç
PostgreSQL ile çalışan projeyi SQL Server'a taşımak. Clean Architecture'ın gücünü göstermek — sadece Infrastructure katmanı değişecek.

### Yapılacaklar
- [ ] `Microsoft.EntityFrameworkCore.SqlServer` NuGet paketi ekle
- [ ] Connection string'i SQL Server'a çevir
- [ ] Provider değişikliği: `UseNpgsql()` → `UseSqlServer()`
- [ ] Tip farklılıklarını düzelt
- [ ] Migration'ları yeniden oluştur
- [ ] Seed data'yı tekrar uygula
- [ ] Tüm API ve MVC testlerini tekrar çalıştır
- [ ] Docker Compose'a SQL Server container ekle

---

## 📨 MassTransit Entegrasyonu

### Amaç
Raw RabbitMQ.Client yerine MassTransit kullanarak daha production-ready bir mesajlaşma altyapısı kurmak.

### Yapılacaklar
- [ ] MassTransit NuGet paketleri ekle
- [ ] Consumer'ları MassTransit consumer pattern'ına çevir
- [ ] Retry policy konfigürasyonu
- [ ] Dead letter queue (DLQ) konfigürasyonu
- [ ] Outbox pattern implementasyonu

---

## 🛡️ Polly ile Resilience

### Yapılacaklar
- [ ] Retry policy: Dış API çağrılarında 3 deneme, exponential backoff
- [ ] Circuit breaker: Ardışık 5 hata → 30 saniye devre kesici
- [ ] Timeout policy: 10 saniye sonra timeout
- [ ] HttpClient + Polly entegrasyonu

---

## 🧪 Test Coverage

### Unit Tests
- [ ] xUnit proje oluştur: `WatchListScreening.Tests`
- [ ] Service layer unit testleri (Moq ile mock'lama)
- [ ] MatchingEngine testleri
- [ ] Scraper testleri (mock HTML ile)

### Integration Tests
- [ ] API integration testleri (WebApplicationFactory)
- [ ] Veritabanı integration testleri (TestContainers)

---

## 📊 Gelişmiş Özellikler

### Gelişmiş Matching Engine
- [ ] Soundex/Metaphone algoritması
- [ ] Transliterasyon desteği
- [ ] Ağırlıklı skor hesaplama

### SignalR ile Real-time Bildirimler
- [ ] Harvest tamamlandığında real-time bildirim
- [ ] Dashboard canlı güncelleme

### Raporlama
- [ ] PDF rapor oluşturma
- [ ] Excel export

### API Güvenliği
- [ ] JWT Authentication
- [ ] Role-based authorization
- [ ] Rate limiting

---

# 🗺️ Faz 4 — Uzun Vadeli Vizyon (Eski Faz 3+)

| Konu | Açıklama |
|---|---|
| Microservices | Screening, List Management, Harvester ayrı servisler |
| Kafka | Yüksek hacimli event streaming |
| Elasticsearch | Full-text search, log yönetimi |
| Kubernetes | Container orchestration |
| CI/CD | GitLab CI / GitHub Actions pipeline |
| gRPC | Servisler arası hızlı iletişim |
| ML Model | Daha akıllı false positive tespiti |
