# 🌍 Domain Bilgisi: AML, Yaptırım Tarama ve Finansal Uyum

> Bu doküman, Sanction Scanner'ın çalıştığı domain'i anlamak için gerekli temel kavramları açıklar.
> Yarınki görüşmede bu kavramları bilmek, teknik becerinin ötesinde domain farkındalığı gösterir.

---

## 📖 Temel Kavramlar

### AML (Anti-Money Laundering — Kara Para Aklama Önleme)
Kara para aklama, yasadışı yollarla elde edilen paranın "temiz" para gibi gösterilmesi işlemidir.
AML, bunu önlemek için uygulanan yasal düzenlemeler, prosedürler ve teknolojilerin genel adıdır.

**Neden önemli?**
- Bankalara ve fintech'lere yasal zorunluluk (MASAK, FATF, FinCEN gibi otoriteler)
- Uyumsuzluk → ağır cezalar (milyonlarca dolar)
- Sanction Scanner bu süreci **otomatikleştiren** bir SaaS platformu

### KYC (Know Your Customer — Müşterini Tanı)
Bir müşteriyle iş ilişkisi kurmadan önce kimliğini doğrulama ve risk değerlendirmesi yapma.

```
Yeni müşteri → Kimlik doğrulama → Yaptırım listesi kontrolü → Risk skoru → Onay/Red
```

### KYB (Know Your Business — İşletmeni Tanı)
KYC'nin kurumsal versiyonu. Şirketlerin ortaklık yapısı, beneficiary owners (gerçek faydalanıcılar) kontrol edilir.

---

## 🔍 Yaptırım Listesi Tarama (Sanctions Screening)

### Yaptırım Listesi Nedir?
Devletler ve uluslararası kuruluşlar tarafından yayınlanan, ticaret/finansal işlem yapılmaması gereken kişi ve kuruluş listeleri.

**Başlıca listeler:**
| Liste | Kaynak | Açıklama |
|---|---|---|
| OFAC SDN | ABD Hazine Bakanlığı | En kritik ABD yaptırım listesi |
| EU Sanctions | Avrupa Birliği | AB yaptırımları |
| UN Consolidated | Birleşmiş Milletler | Küresel yaptırımlar |
| MASAK | Türkiye | Türkiye'nin ulusal listesi |
| HMT | İngiltere | UK yaptırımları |

### PEP (Politically Exposed Person — Siyasi Nüfuz Sahibi Kişi)
Üst düzey siyasi pozisyonda olan veya yakın ilişkisi bulunan kişiler. Daha yüksek risk taşırlar.
Örnekler: Başbakanlar, bakanlar, yüksek rütbeli subaylar, büyükelçiler ve bunların yakın aile üyeleri.

### Adverse Media (Olumsuz Medya)
Bir kişi veya kuruluş hakkında yolsuzluk, dolandırıcılık, terör finansmanı gibi konularda çıkan olumsuz haberler.

---

## ⚙️ Tarama Süreci (Screening Flow)

```
┌──────────────────────────────────────────────────────────────────┐
│                     TARAMA SÜRECİ                                │
│                                                                   │
│  1. INPUT                                                         │
│     Müşteri bilgisi gelir (isim, doğum tarihi, ülke, vb.)       │
│                    ↓                                              │
│  2. SCREENING (Tarama)                                            │
│     Yaptırım listelerine karşı eşleştirme yapılır               │
│     - Exact match (tam eşleşme)                                  │
│     - Fuzzy match (bulanık eşleşme — yazım hataları, varyasyonlar)│
│     - Phonetic match (fonetik eşleşme — ses benzerliği)          │
│                    ↓                                              │
│  3. SCORING (Puanlama)                                            │
│     Her eşleşmeye bir MatchScore verilir (0-100)                 │
│     Threshold üstü → "Potential Match"                           │
│                    ↓                                              │
│  4. REVIEW (İnceleme)                                             │
│     Compliance uzmanı sonucu inceler:                            │
│     → False Positive (yanlış alarm) → Onay                      │
│     → True Match (gerçek eşleşme) → Aksiyona geç               │
│     → Escalate (yükselt) → Üst birime ilet                     │
│                    ↓                                              │
│  5. ACTION (Aksiyon)                                              │
│     İşlem engelleme, hesap dondurma, SAR raporu oluşturma        │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🎯 False Positive Problemi

AML taramasının **en büyük sorunu** false positive (yanlış alarm) oranının yüksek olmasıdır.

**Örnek:**
- Yaptırım listesinde: "Ali Hassan" (terör finansmanı şüphelisi)
- Müşteri: "Ali Hasan" (sıradan bir vatandaş)
- Fuzzy match skoru: 92% → Eşleşme!
- Ama bu bir **false positive** — iki farklı kişi

**Neden Sanction Scanner önemli?**
AI ve gelişmiş algoritmalar ile false positive oranını düşürür → operasyonel maliyet azalır.

> [!TIP]
> **Yarınki görüşmede bu kavramı bilmek çok değerli.** "False positive oranını düşürmek neden kritik?" sorusuna cevap verebilmek, domain farkındalığını gösterir.

---

## 📊 Risk Seviyeleri

| Risk Seviyesi | Açıklama | Aksiyon |
|---|---|---|
| **Low** | Düşük eşleşme skoru, bilinen güvenli kaynak | Otomatik onay |
| **Medium** | Orta eşleşme, ek kontrol gerekebilir | Manuel inceleme |
| **High** | Yüksek eşleşme skoru | Acil inceleme + üst bildirim |
| **Critical** | Tam eşleşme veya çoklu liste eşleşmesi | İşlem durdurma + yasal bildirim |

---

## 🏢 Sanction Scanner'ın Ürünleri

### 1. AML Screening API
- REST API ile müşteri bilgisi gönder → yaptırım listelerine karşı tara → sonuç al
- Gerçek zamanlı (real-time) tarama
- 3000+ yaptırım listesi, PEP listesi, düzenleyici liste

### 2. Transaction Monitoring
- Finansal işlemleri izle → şüpheli kalıpları tespit et
- Kural tabanlı (rule-based) ve ML tabanlı (machine learning) tespit
- Gerçek zamanlı uyarılar

### 3. Adverse Media Screening
- Medya kaynaklarını otomatik tara
- NLP ile olumsuz haberleri kategorize et

### 4. KYC/KYB Onboarding
- Yeni müşteri kabul sürecini otomatikleştir
- Risk değerlendirmesi + belge doğrulama

---

## 🔗 Projemiz ile Sanction Scanner Bağlantısı

| Projemizdeki Modül | Sanction Scanner'daki Karşılık |
|---|---|
| `SanctionEntry` entity | Sanctions/PEP list veritabanı |
| Screening API | AML Screening API |
| MatchScore hesaplama | Fuzzy matching engine |
| Review sistemi | Compliance dashboard |
| Bulk screening (RabbitMQ) | Toplu tarama API |
| Background list update | Periyodik liste güncelleme |
| Redis cache | Sık sorgulanan liste cache'i |
| Audit log | Regulatory compliance log |

---

## 📚 Faydalı Terimler Sözlüğü

| Terim | Açıklama |
|---|---|
| **SAR** | Suspicious Activity Report — Şüpheli işlem raporu |
| **STR** | Suspicious Transaction Report — Şüpheli işlem bildirimi |
| **FATF** | Financial Action Task Force — Uluslararası AML düzenleyicisi |
| **MASAK** | Mali Suçları Araştırma Kurulu — Türkiye'nin AML otoritesi |
| **CDD** | Customer Due Diligence — Müşteri durum tespiti |
| **EDD** | Enhanced Due Diligence — Artırılmış durum tespiti (yüksek riskli müşteriler) |
| **Beneficial Owner** | Gerçek faydalanıcı — Şirketin arkasındaki gerçek kişi |
| **Watchlist** | İzleme listesi — Yaptırım ve PEP listelerinin genel adı |
| **Threshold** | Eşik değer — Eşleşme skorunun "match" sayılması için gereken minimum |
| **Onboarding** | Müşteri kabul süreci |
| **RegTech** | Regulatory Technology — Düzenleyici uyum teknolojisi |
