using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

/// <summary>
/// API'den dışarıya döndürülen yaptırım kaydı.
/// Entity'nin tamamını değil, sadece ihtiyaç duyulan alanları içerir.
/// Neden DTO? → Entity'yi direkt dönsek EF navigation property'leri,
/// circular reference veya gizli alanlar sızmış olabilir.
/// </summary>
public class SanctionEntryDto
{
    /// <summary>Kaydın benzersiz kimliği.</summary>
    public int Id { get; set; }

    /// <summary>
    /// Taramanın yapıldığı birincil alan.
    /// Yaptırım listesindeki kişi veya kuruluşun tam adı.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Person veya Organization.</summary>
    public EntityType EntityType { get; set; }

    /// <summary>Ülke bilgisi — coğrafi filtreleme için.</summary>
    public string? Country { get; set; }

    /// <summary>
    /// FK — kayıtlı ListSource tablosundaki kaynak ID'si.
    /// Nullable: Faz 2 öncesi eklenen seed kayıtlarda yok.
    /// </summary>
    public int? ListSourceId { get; set; }

    /// <summary>
    /// Hangi listeden geldiği — ListSource.Name navigation'dan.
    /// Örnek: "OFAC SDN List", "UN Consolidated List"
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Kayıt aktif mi?
    /// False ise yaptırım kaldırılmış demektir (soft delete).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Doğum tarihi — aynı isimde farklı kişileri ayırt etmek için.
    /// Nullable çünkü organizasyonlarda doğum tarihi olmaz.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Kaydın sisteme eklendiği tarih (teknik takip).</summary>
    public DateTime CreatedAt { get; set; }
}

