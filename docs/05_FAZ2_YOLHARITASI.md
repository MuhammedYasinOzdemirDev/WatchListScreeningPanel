# 🚀 Faz 2 — Yol Haritası (Geliştirilmiş Versiyon)

> Faz 1 tamamlandıktan sonra projeyi büyütmek için planlanan geliştirmeler.
> Faz 2 hem teknik derinliği artırır hem de CV'ne ekleyebileceğin yeni yetenekler kazandırır.

---

## 🔄 SQL Server'a Migration

### Amaç
PostgreSQL ile çalışan projeyi SQL Server'a taşımak. Clean Architecture'ın gücünü göstermek — sadece Infrastructure katmanı değişecek.

### Yapılacaklar
- [ ] `Microsoft.EntityFrameworkCore.SqlServer` NuGet paketi ekle
- [ ] Connection string'i SQL Server'a çevir
- [ ] Provider değişikliği: `UseNpgsql()` → `UseSqlServer()`
- [ ] Tip farklılıklarını düzelt:
  - `boolean` → `bit`
  - `timestamp` → `datetime2`
  - `text` → `nvarchar(max)`
  - `serial` → `IDENTITY`
- [ ] Migration'ları yeniden oluştur
- [ ] Seed data'yı tekrar uygula
- [ ] Tüm API ve MVC testlerini tekrar çalıştır
- [ ] Docker Compose'a SQL Server container ekle

### Öğrenme Hedefi
"Mimarim doğruysa, veritabanı değiştirmek Infrastructure'da birkaç satır değiştirmek kadar kolay olmalı."

---

## 📨 MassTransit Entegrasyonu

### Amaç
Raw RabbitMQ.Client yerine MassTransit kullanarak daha production-ready bir mesajlaşma altyapısı kurmak.

### Yapılacaklar
- [ ] MassTransit NuGet paketleri ekle
- [ ] Consumer'ları MassTransit consumer pattern'ına çevir
- [ ] Retry policy konfigürasyonu
- [ ] Dead letter queue (DLQ) konfigürasyonu
- [ ] Outbox pattern implementasyonu (mesaj kaybını önlemek için)
- [ ] Saga pattern farkındalığı (uzun süren iş akışları için)

### Neden MassTransit?
- Retry, circuit breaker, DLQ gibi enterprise pattern'ları built-in
- RabbitMQ, Kafka, Azure Service Bus gibi farklı transport'lara geçiş kolaylığı
- Test edilebilirlik (InMemory transport ile)

---

## 🛡️ Polly ile Resilience

### Amaç
Dış servislere yapılan isteklerde dayanıklılık (resilience) pattern'ları eklemek.

### Yapılacaklar
- [ ] Polly NuGet paketi ekle
- [ ] Retry policy: Dış API çağrılarında 3 deneme, exponential backoff
- [ ] Circuit breaker: Ardışık 5 hata → 30 saniye devre kesici
- [ ] Timeout policy: 10 saniye sonra timeout
- [ ] Policy wrap: Retry + Circuit Breaker + Timeout birleşimi
- [ ] HttpClient + Polly entegrasyonu (typed HttpClient)

---

## 🧪 Test Coverage

### Unit Tests
- [ ] xUnit proje oluştur: `WatchListScreening.Tests`
- [ ] Service layer unit testleri (Moq ile mock'lama)
- [ ] MatchingEngine testleri (çeşitli isim senaryoları)
- [ ] Repository testleri (InMemory DbContext ile)

### Integration Tests
- [ ] API integration testleri (WebApplicationFactory)
- [ ] Veritabanı integration testleri (TestContainers)

### UI Tests (Bonus)
- [ ] Playwright veya Selenium ile temel UI testleri
- [ ] Tarama akışı otomatik test

---

## 📊 Gelişmiş Özellikler

### Gelişmiş Matching Engine
- [ ] Soundex/Metaphone algoritması (fonetik eşleştirme)
- [ ] Transliterasyon desteği (Arapça/Kirilce → Latin)
- [ ] Ağırlıklı skor hesaplama (isim + ülke + doğum tarihi)

### SignalR ile Real-time Bildirimler
- [ ] Toplu tarama tamamlandığında real-time bildirim
- [ ] Yeni yüksek riskli eşleşme bildirim paneli
- [ ] Dashboard canlı güncelleme

### Raporlama
- [ ] PDF rapor oluşturma (tarama sonuçları)
- [ ] Excel export
- [ ] Tarih bazlı istatistik grafikleri

### API Güvenliği
- [ ] JWT Authentication
- [ ] Role-based authorization (Admin, Reviewer, Viewer)
- [ ] API key management
- [ ] Rate limiting

---

## 🗺️ Faz 3+ Vizyonu (Uzun Vadeli)

| Konu | Açıklama |
|---|---|
| Microservices | Screening, List Management, Notification ayrı servisler |
| Kafka | Yüksek hacimli event streaming |
| Elasticsearch | Full-text search, log yönetimi |
| Kubernetes | Container orchestration |
| CI/CD | GitLab CI / GitHub Actions pipeline |
| gRPC | Servisler arası hızlı iletişim |
| ML Model | Daha akıllı false positive tespiti |
