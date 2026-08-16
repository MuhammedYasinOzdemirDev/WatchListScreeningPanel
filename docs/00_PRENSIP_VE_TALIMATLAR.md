# 📜 Prensip ve Talimatlar Dokümanı

> Bu doküman, projeyi nasıl geliştireceğimizi, çalışma metodumuzu ve birbirimize karşı sorumluluklarımızı tanımlar.

---

## 🎯 Amaç

Bu proje bir **iş başvurusu sınavına hazırlık** değil, bir **pekiştirme ve yeniden hızlanma çalışmasıdır.**
2+ haftadır kod yazmayan bir geliştiricinin, bildiklerini hatırlaması ve güvenini tazelemesi için tasarlanmıştır.

---

## 🤝 Çalışma Prensiplerimiz

### 1. Kodu Sen Yazarsın
- Her satırı sen yazacaksın. Copy-paste değil, anlayarak yazma.
- Takıldığında önce 2-3 dakika kendin düşün. Sonra sor.
- Sormak zayıflık değil, sormamak zayıflıktır.

### 2. Ben Sana Öğretici Dille Anlatırım
- Her modüle başlamadan önce "neden bunu yapıyoruz" açıklamasını veririm.
- Sınav gibi sorarım ama sınav değilmiş gibi açıklarım.
- Cevabı bilmiyorsan sorun yok — birlikte çözeriz.

### 3. Sınav Modu vs Öğretici Mod
| Durum | Yaklaşım |
|---|---|
| Konsepti anlattığımda | "Bu nedir, neden var?" sorusuyla başlarım |
| Kod yazarken | "Şimdi şunu yap" derken ipucu veririm |
| Takıldığında | Önce ipucu, sonra açıklama, en son kod |
| Bitirdiğinde | "Bunu neden böyle yaptık?" diye review yaparım |

### 4. Zaman Yönetimi
- Planlanmış süreler tahminidir, kesin değil.
- Bir modülde fazla zaman harcadıysan sorun değil — öğrenme anlamına gelir.
- Ama bir yerde 30+ dakika takılıyorsan, bana sor — zaman kaybetme.
- Gerekirse plan uzar, bu normal.

### 5. Hata Yapma Politikası
- Hata yapmak **beklenen ve istenen** bir şey.
- Derleme hatası? Güzel — hata mesajını oku ve anla.
- Runtime hatası? Daha da güzel — debug pratik yaparsın.
- Mantık hatası? En değerli öğrenme anı.

---

## 🏗️ Proje Geliştirme Prensipleri

### Kod Yazım Kuralları
- **SOLID prensipleri** — özellikle Single Responsibility ve Dependency Inversion
- **Clean Code** — anlamlı isimlendirme, kısa metodlar, tek sorumluluk
- **DRY (Don't Repeat Yourself)** — tekrar eden kodu soyutla
- **KISS (Keep It Simple, Stupid)** — gereksiz karmaşıklıktan kaçın, overengineering yapma

### Commit Prensipleri
- Her modül tamamlandığında commit at
- Commit mesajları anlamlı olsun: `feat: add screening service with fuzzy matching`
- Küçük ve sık commitler > büyük ve nadir commitler

### Dosya ve Klasör İsimlendirme
- Solution adı: `WatchListScreening`
- Namespace'ler proje katmanını yansıtsın
- Dosya başına tek sınıf (istisnalar: küçük DTO'lar, enum'lar)

---

## 🎓 Öğrenme Hedefleri (Bu Projeyle Ne Kazanacaksın)

### Teknik Hedefler
- [ ] .NET 8 proje yapısını sıfırdan kurabilmek
- [ ] EF Core ile code-first veritabanı yönetimi
- [ ] RESTful API tasarlayıp implement edebilmek
- [ ] ASP.NET MVC ile admin paneli geliştirebilmek
- [ ] Redis cache kullanabilmek
- [ ] RabbitMQ ile asenkron mesajlaşma kurabilmek
- [ ] Background service yazabilmek
- [ ] Docker Compose ile sistemi ayağa kaldırabilmek

### Domain Hedefleri
- [ ] AML/KYC kavramlarını anlayabilmek
- [ ] Yaptırım listesi tarama mantığını kavramak
- [ ] False positive yönetimini bilmek
- [ ] Risk skorlama mantığını kavramak

### Soft Hedefler
- [ ] Kod yazma hızını geri kazanmak
- [ ] Problem çözme reflekslerini tazelemek
- [ ] Teknik terimleri akıcı kullanabilmek (yarınki görüşme için)

---

## 📌 Faz Yapısı

| Faz | İçerik | Durum |
|---|---|---|
| **Faz 1** | PostgreSQL + tüm modüller + Docker | 🔄 Aktif |
| **Faz 2** | Otomatik Liste Toplama (List Harvester): Web scraping, Hangfire, RabbitMQ | 📋 Planlanmış |
| **Faz 3** | SQL Server migration, MassTransit, Polly, test coverage | 📋 Planlanmış |
| **Faz 4+** | Kafka, microservice ayrımı, Kubernetes, CI/CD | 💡 Gelecek |

---

## ⚠️ Önemli Hatırlatmalar

> [!WARNING]
> **Mükemmeliyetçilik tuzağına düşme.** Amaç production-ready bir sistem değil, pratik yapıp hatırlamak.

> [!TIP]
> **Her modülü bitirdiğinde kendine sor:** "Yarın biri bana bunu sorsa anlatabilir miyim?"

> [!IMPORTANT]
> **Sanction Scanner bağlamını unutma.** Bu sadece teknik bir proje değil — yarın gittiğin şirketin core işini anlaman gerekiyor.
