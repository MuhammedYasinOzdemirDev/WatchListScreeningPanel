namespace WatchListScreening.Domain.Enums;

public enum ReviewStatus
{
    Pending = 1,        // Henüz incelenmedi
    UnderReview = 2,    // İnceleme sürecinde (bir uzman aldı)
    Approved = 3,       // False positive — onaylandı, risk yok
    Confirmed = 4,      // True match — gerçek eşleşme, aksiyon gerekiyor
    Escalated = 5       // Üst birime yükseltildi — karar verilemedi
}
