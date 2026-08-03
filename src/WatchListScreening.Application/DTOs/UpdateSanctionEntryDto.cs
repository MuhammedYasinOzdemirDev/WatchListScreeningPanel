namespace WatchListScreening.Application.DTOs;

/// <summary>
/// Mevcut yaptırım kaydını güncellemek için API'ye gönderilen veri.
/// Neden Create'den ayrı? → Update'de tüm alanlar zorunlu olmayabilir (PATCH mantığı).
/// Ayrıca bazı alanlar (ListSource, EntityType) hiç değiştirilemez iş kuralı gereği.
/// </summary>
public class UpdateSanctionEntryDto
{
    /// <summary>Güncellenen tam ad.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Opsiyonel — ülke güncellemesi.</summary>
    public string? Country { get; set; }

    /// <summary>Opsiyonel — doğum tarihi güncellemesi.</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Opsiyonel — ulusal kimlik numarası güncellemesi.</summary>
    public string? NationalId { get; set; }

    /// <summary>Opsiyonel — kaynak URL güncellemesi.</summary>
    public string? ListSourceUrl { get; set; }

    /// <summary>
    /// Kaydı aktif/pasif yapmak için.
    /// True → aktif, False → soft delete (yaptırım kaldırıldı).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
