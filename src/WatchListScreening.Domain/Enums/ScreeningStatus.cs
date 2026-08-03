namespace WatchListScreening.Domain.Enums;

public enum ScreeningStatus
{
    Pending = 1,       // Kuyrukta bekliyor (özellikle bulk taramalar)
    Processing = 2,    // İşleniyor
    Completed = 3,     // Tamamlandı
    Failed = 4         // Hata oluştu
}
