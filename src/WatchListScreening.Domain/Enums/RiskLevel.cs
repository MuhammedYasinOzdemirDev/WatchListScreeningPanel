namespace WatchListScreening.Domain.Enums;

public enum RiskLevel
{
    Low = 1,           // MatchScore < 50 — Düşük risk, muhtemelen false positive
    Medium = 2,        // MatchScore 50-74 — Orta risk, inceleme gerekli
    High = 3,          // MatchScore 75-89 — Yüksek risk, öncelikli inceleme
    Critical = 4       // MatchScore >= 90 — Kritik, olası gerçek eşleşme
}
