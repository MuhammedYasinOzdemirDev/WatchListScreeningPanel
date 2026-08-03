using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Yeni yaptırım kaydı oluşturmak için API'ye gönderilen veri.
/// Neden ayrı DTO? → Create işleminde Id, CreatedAt gibi alanlar kullanıcıdan gelmez,
/// sistem tarafından otomatik atanır. Bu yüzden SanctionEntryDto'dan ayrıdır.
/// </summary>
public class CreateSanctionEntryDto
{
    /// <summary>
    /// Zorunlu — taramanın yapılacağı birincil alan.
    /// Boş gönderilemez.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Person mi Organization mı?</summary>
    public EntityType EntityType { get; set; }

    /// <summary>Opsiyonel — ülke bilgisi.</summary>
    public string? Country { get; set; }

    /// <summary>
    /// Zorunlu — hangi yaptırım listesinden eklendiği.
    /// Örnek: "OFAC", "UN", "EU", "MASAK"
    /// </summary>
    public string ListSource { get; set; } = string.Empty;

    /// <summary>Opsiyonel — doğum tarihi (kişiler için).</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Opsiyonel — ulusal kimlik numarası.</summary>
    public string? NationalId { get; set; }

    /// <summary>Opsiyonel — kaynak liste URL'si.</summary>
    public string? ListSourceUrl { get; set; }
}
