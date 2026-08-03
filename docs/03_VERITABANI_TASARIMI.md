# 🗄️ Veritabanı Tasarımı — Faz 1 (PostgreSQL)

> Bu doküman, projenin veritabanı şemasını, her tablonun amacını, her sütunun neden var olduğunu ve domain bağlamını açıklar.
> Faz 2'de bu şema SQL Server'a taşınacaktır.

---

## 📐 ER Diyagramı

```
┌─────────────────────┐       ┌─────────────────────────┐
│   SanctionEntries   │       │    ScreeningRequests     │
├─────────────────────┤       ├─────────────────────────┤
│ Id (PK)             │       │ Id (PK)                 │
│ FullName            │◄──┐   │ SearchQuery             │
│ FirstName           │   │   │ SearchType              │
│ LastName            │   │   │ RequestedBy             │
│ EntityType          │   │   │ RequestedAt             │
│ Country             │   │   │ CompletedAt             │
│ DateOfBirth         │   │   │ Status                  │
│ NationalId          │   │   │ TotalMatches            │
│ ListSource          │   │   │ IsBulk                  │
│ ListSourceUrl       │   │   │ Notes                   │
│ Aliases             │   │   │ CreatedAt               │
│ AdditionalInfo      │   │   │ UpdatedAt               │
│ IsActive            │   │   └────────────┬────────────┘
│ AddedAt             │   │                │
│ DeactivatedAt       │   │                │ 1:N
│ CreatedAt           │   │                │
│ UpdatedAt           │   │   ┌────────────┴────────────┐
└─────────────────────┘   │   │    ScreeningResults      │
                          │   ├─────────────────────────┤
                          │   │ Id (PK)                 │
                          └───│ SanctionEntryId (FK)    │
                              │ ScreeningRequestId (FK) │
                              │ MatchScore              │
                              │ MatchType               │
                              │ RiskLevel               │
                              │ ReviewStatus            │
                              │ ReviewedBy              │
                              │ ReviewedAt              │
                              │ ReviewNotes             │
                              │ CreatedAt               │
                              │ UpdatedAt               │
                              └─────────────────────────┘

┌─────────────────────┐
│     AuditLogs       │
├─────────────────────┤
│ Id (PK)             │
│ Action              │
│ EntityType          │
│ EntityId            │
│ PerformedBy         │
│ PerformedAt         │
│ OldValues           │
│ NewValues           │
│ IpAddress           │
│ Details             │
└─────────────────────┘
```

---

## 📋 Tablo Detayları

### 1. `SanctionEntries` — Yaptırım Listesi Kayıtları

**Amacı:** Çeşitli uluslararası yaptırım listelerinden alınan kişi ve kuruluş bilgilerini saklar. Bu tablo, tarama (screening) işleminin "karşılaştırma kaynağıdır."

| Sütun | Tip | Zorunlu | Açıklama | Domain Amacı |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | Otomatik artan birincil anahtar | Tekil kayıt tanımlayıcı |
| `FullName` | varchar(500) | ✅ | Kişi/kuruluşun tam adı | Birincil eşleştirme alanı — tarama buna karşı yapılır |
| `FirstName` | varchar(250) | ❌ | Kişinin adı | Parçalı eşleştirme için (isim+soyisim ayrı aranabilir) |
| `LastName` | varchar(250) | ❌ | Kişinin soyadı | Parçalı eşleştirme için |
| `EntityType` | smallint (enum) | ✅ | Kayıt tipi: Person / Organization | Kişi mi kuruluş mu? Tarama türü buna göre ayrışır |
| `Country` | varchar(100) | ❌ | Ülke bilgisi | Coğrafi filtreleme — bazı taramalar ülkeye özel yapılır |
| `DateOfBirth` | date | ❌ | Doğum tarihi | İsim eşleşmesi sonrası doğrulama — aynı isimde farklı kişileri ayırır |
| `NationalId` | varchar(50) | ❌ | Ulusal kimlik numarası | Kesin doğrulama — isim benzerliği yetmediğinde kimlik numarasıyla teyit |
| `ListSource` | varchar(200) | ✅ | Hangi yaptırım listesinden geldiği (OFAC, EU, UN, MASAK) | Müşteri hangi listelere karşı taranacağını seçebilir |
| `ListSourceUrl` | varchar(1000) | ❌ | Kaynak URL | Denetim sırasında kaynağa geri dönülebilmesi için |
| `Aliases` | text | ❌ | Takma adlar (JSON array olarak) | Bir kişinin birden fazla adı olabilir: gerçek ad, takma ad, transliterasyon |
| `AdditionalInfo` | text | ❌ | Ek bilgiler (JSON) | Pasaport no, adres, ilişkili kişiler gibi yapısal olmayan veriler |
| `IsActive` | boolean | ✅ | Kayıt aktif mi? | Yaptırım kaldırılabilir — soft delete, eski kayıtlar silinmez |
| `AddedAt` | timestamp | ✅ | Yaptırım listesine eklendiği tarih | Düzenleyici denetim için: "ne zaman listeye girdi?" |
| `DeactivatedAt` | timestamp | ❌ | Yaptırımın kaldırıldığı tarih | Yaptırım kaldırıldıysa ne zaman kaldırıldı? |
| `CreatedAt` | timestamp | ✅ | DB'ye eklendiği tarih | Teknik takip |
| `UpdatedAt` | timestamp | ❌ | Son güncellenme tarihi | Teknik takip |

**Index'ler:**
- `IX_SanctionEntries_FullName` — En kritik index, tüm taramalar bu sütun üzerinden yapılır
- `IX_SanctionEntries_ListSource` — Liste bazlı filtreleme
- `IX_SanctionEntries_IsActive` — Sadece aktif kayıtları sorgulamak için
- `IX_SanctionEntries_Country` — Ülke bazlı filtreleme

> [!NOTE]
> **Neden `Aliases` JSON olarak saklanıyor?**
> Bir kişinin takma ad sayısı belirsizdir ve ilişkisel bir tablo yerine JSON array ile saklamak bu seviyede yeterlidir. Production'da ayrı bir `SanctionAliases` tablosu daha uygun olabilir.

---

### 2. `ScreeningRequests` — Tarama İstekleri

**Amacı:** Kullanıcıların yaptığı her tarama isteğini kaydeder. "Kim, ne zaman, neyi aradı?" sorusunun cevabıdır.

| Sütun | Tip | Zorunlu | Açıklama | Domain Amacı |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | Otomatik artan birincil anahtar | Tekil tarama tanımlayıcı |
| `SearchQuery` | varchar(500) | ✅ | Aranan isim/kuruluş adı | Kullanıcının tarama kutusuna yazdığı metin |
| `SearchType` | smallint (enum) | ✅ | Individual / Organization | Kişi mi kuruluş mu aranıyor? |
| `RequestedBy` | varchar(200) | ✅ | İsteği yapan kullanıcı | Denetim izi (audit trail) — kim tarama yaptı? |
| `RequestedAt` | timestamp | ✅ | İstek zamanı | Denetim izi — ne zaman yapıldı? |
| `CompletedAt` | timestamp | ❌ | Tamamlanma zamanı | Performans ölçümü + async işlemlerde bitiş takibi |
| `Status` | smallint (enum) | ✅ | Pending / Processing / Completed / Failed | İşlemin mevcut durumu — özellikle bulk taramalar için |
| `TotalMatches` | int | ❌ | Bulunan toplam eşleşme sayısı | Hızlı özet — detaya girmeden kaç sonuç bulundu? |
| `IsBulk` | boolean | ✅ | Toplu tarama mı? | Tek isim taraması vs CSV'den gelen toplu tarama ayrımı |
| `Notes` | text | ❌ | Kullanıcı notu | İsteğe bağlı açıklama — "onboarding için kontrol" gibi |
| `CreatedAt` | timestamp | ✅ | Kayıt oluşturulma tarihi | Teknik takip |
| `UpdatedAt` | timestamp | ❌ | Son güncelleme tarihi | Teknik takip |

**Index'ler:**
- `IX_ScreeningRequests_Status` — Bekleyen işlemleri sorgulamak için
- `IX_ScreeningRequests_RequestedAt` — Tarih bazlı raporlama
- `IX_ScreeningRequests_RequestedBy` — Kullanıcı bazlı geçmiş

---

### 3. `ScreeningResults` — Tarama Sonuçları

**Amacı:** Bir tarama isteğinin ürettiği eşleşme sonuçlarını saklar. Bir tarama isteği birden fazla sonuç üretebilir.

| Sütun | Tip | Zorunlu | Açıklama | Domain Amacı |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | Birincil anahtar | Tekil sonuç tanımlayıcı |
| `ScreeningRequestId` | int (FK) | ✅ | Hangi tarama isteğine ait | İstek-sonuç ilişkisi |
| `SanctionEntryId` | int (FK) | ✅ | Hangi yaptırım kaydıyla eşleşti | Eşleşen kayda referans |
| `MatchScore` | decimal(5,2) | ✅ | Eşleşme skoru (0.00 - 100.00) | **En kritik alan** — ne kadar benzer? 95 = çok benzer, 40 = belirsiz |
| `MatchType` | varchar(50) | ✅ | Exact / Fuzzy / Contains / Phonetic | Hangi algoritma ile eşleşti? |
| `RiskLevel` | smallint (enum) | ✅ | Low / Medium / High / Critical | Otomatik risk değerlendirmesi — MatchScore'a göre hesaplanır |
| `ReviewStatus` | smallint (enum) | ✅ | Pending / UnderReview / Approved / Confirmed / Escalated | Compliance uzmanının değerlendirme durumu |
| `ReviewedBy` | varchar(200) | ❌ | İnceleyen kişi | Kim inceledi? |
| `ReviewedAt` | timestamp | ❌ | İncelenme zamanı | Ne zaman incelendi? (SLA takibi) |
| `ReviewNotes` | text | ❌ | İnceleme notu | "False positive — farklı kişi, doğum tarihi uymuyor" gibi |
| `CreatedAt` | timestamp | ✅ | Oluşturulma tarihi | Teknik takip |
| `UpdatedAt` | timestamp | ❌ | Son güncelleme tarihi | Teknik takip |

**Index'ler:**
- `IX_ScreeningResults_ScreeningRequestId` — İsteğe göre sonuçları getir
- `IX_ScreeningResults_ReviewStatus` — Bekleyen incelemeleri listele
- `IX_ScreeningResults_RiskLevel` — Risk bazlı filtreleme
- `IX_ScreeningResults_SanctionEntryId` — Bir yaptırım kaydının kaç kez eşleştiğini bul

> [!IMPORTANT]
> **MatchScore neden decimal(5,2)?**
> 100.00'e kadar iki ondalık hassasiyette skor saklarız. Integer yeterli olabilir ama hassas sıralama ve threshold karşılaştırmaları için ondalıklı değer daha esnektir.

---

### 4. `AuditLogs` — Denetim İzleri

**Amacı:** Sistemde yapılan tüm kritik işlemleri kaydeder. Düzenleyici denetimler (regulatory audit) sırasında "kim, ne zaman, ne yaptı?" sorusuna cevap verir.

| Sütun | Tip | Zorunlu | Açıklama | Domain Amacı |
|---|---|---|---|---|
| `Id` | int (PK) | ✅ | Birincil anahtar | Tekil log tanımlayıcı |
| `Action` | varchar(100) | ✅ | Yapılan işlem: Create, Update, Delete, Screen, Review, Export... | Ne yapıldı? |
| `EntityType` | varchar(100) | ✅ | İşlem yapılan entity tipi: SanctionEntry, ScreeningRequest, ScreeningResult | Hangi tablo üzerinde? |
| `EntityId` | int | ❌ | İlgili kaydın ID'si | Hangi kayıt? |
| `PerformedBy` | varchar(200) | ✅ | İşlemi yapan kullanıcı | Kim yaptı? |
| `PerformedAt` | timestamp | ✅ | İşlem zamanı | Ne zaman yapıldı? |
| `OldValues` | text (JSON) | ❌ | Değişiklik öncesi değerler | Değişiklik takibi — eski hali ne idi? |
| `NewValues` | text (JSON) | ❌ | Değişiklik sonrası değerler | Yeni hali ne? |
| `IpAddress` | varchar(50) | ❌ | İsteğin geldiği IP | Güvenlik denetimi |
| `Details` | text | ❌ | Ek açıklama | Serbest metin — "Bulk import from OFAC list" gibi |

**Index'ler:**
- `IX_AuditLogs_PerformedAt` — Tarih bazlı sorgulama
- `IX_AuditLogs_EntityType_EntityId` — Belirli bir kaydın geçmişini bul
- `IX_AuditLogs_Action` — İşlem türüne göre filtreleme

> [!TIP]
> **AML düzenlemelerinde audit trail zorunludur.** Her tarama, her inceleme kararı, her liste güncellemesi kayıt altında olmalıdır. Bu tablo sadece "güzel olsun" diye değil, yasal zorunluluktur.

---

## 🔢 Enum Tanımları

### EntityType
```csharp
public enum EntityType
{
    Person = 1,        // Gerçek kişi
    Organization = 2   // Kuruluş, şirket, örgüt
}
```

### ScreeningStatus
```csharp
public enum ScreeningStatus
{
    Pending = 1,       // Kuyrukta bekliyor (özellikle bulk taramalar)
    Processing = 2,    // İşleniyor
    Completed = 3,     // Tamamlandı
    Failed = 4         // Hata oluştu
}
```

### RiskLevel
```csharp
public enum RiskLevel
{
    Low = 1,           // MatchScore < 50 — Düşük risk, muhtemelen false positive
    Medium = 2,        // MatchScore 50-74 — Orta risk, inceleme gerekli
    High = 3,          // MatchScore 75-89 — Yüksek risk, öncelikli inceleme
    Critical = 4       // MatchScore >= 90 — Kritik, olası gerçek eşleşme
}
```

### ReviewStatus
```csharp
public enum ReviewStatus
{
    Pending = 1,        // Henüz incelenmedi
    UnderReview = 2,    // İnceleme sürecinde (bir uzman aldı)
    Approved = 3,       // False positive — onaylandı, risk yok
    Confirmed = 4,      // True match — gerçek eşleşme, aksiyon gerekiyor
    Escalated = 5       // Üst birime yükseltildi — karar verilemedi
}
```

---

## 🌱 Seed Data (Örnek Veriler)

İlk migration ile birlikte sisteme eklenecek örnek yaptırım verileri:

```
| FullName                    | EntityType   | Country      | ListSource | DateOfBirth |
|-----------------------------|-------------|-------------- |------------|-------------|
| John Alexander Smith        | Person       | United States | OFAC       | 1975-03-15  |
| Ali Hassan Mohammed         | Person       | Syria         | UN         | 1980-07-22  |
| Petrolex Trading Corp       | Organization | Russia        | EU         | NULL        |
| Mehmet Yılmaz               | Person       | Turkey        | MASAK      | 1968-11-03  |
| Golden Bridge Holdings Ltd  | Organization | Iran          | OFAC       | NULL        |
| Fatima Al-Rashid            | Person       | Iraq          | UN         | 1992-04-18  |
| Nord Stream Finance GmbH   | Organization | Germany       | EU         | NULL        |
| Carlos Rodriguez Vega       | Person       | Venezuela     | OFAC       | 1971-09-30  |
| Bright Star Logistics       | Organization | North Korea   | UN         | NULL        |
| Ayşe Demir                  | Person       | Turkey        | MASAK      | 1985-06-14  |
```

> [!NOTE]
> **Bunlar tamamen kurgusal verilerdir.** Gerçek yaptırım listelerindeki isimler kullanılmamıştır. Production ortamda bu veriler resmi kaynaklardan (OFAC, UN, EU) API ile çekilir.

---

## 🔄 Faz 2 Veritabanı Değişiklikleri (Önizleme)

Faz 2'de PostgreSQL → SQL Server'a geçişte dikkat edilecekler:
- `boolean` → `bit`
- `timestamp` → `datetime2`
- `text` → `nvarchar(max)`
- `serial` → `IDENTITY`
- Connection string değişikliği
- Provider değişikliği: `Npgsql` → `SqlServer`
- Migration'lar yeniden oluşturulacak
