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
    /// Opsiyonel — hangi kayıtlı kaynaktan ekleneceği.
    /// Faz 2 ile zorunlu hale gelecek, şu an nullable.
    /// </summary>
    public int? ListSourceId { get; set; }

    /// <summary>Opsiyonel — doğum tarihi (kişiler için).</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Opsiyonel — ulusal kimlik numarası.</summary>
    public string? NationalId { get; set; }
}
