namespace WatchListScreening.Application.Services;

/// <summary>
/// İsim eşleştirme algoritmaları.
/// Strategy Pattern ile her algoritma birbirinden bağımsız.
/// </summary>
public class MatchingEngine
{
    /// <summary>
    /// Bir input string'i, listedeki tüm adlara karşı çalıştırır.
    /// En yüksek skoru döner.
    /// </summary>
    public MatchResult CalculateBestMatch(string input, string target)
    {
        // Case-insensitive karşılaştırma için normalize et
        var normalizedInput = input.Trim().ToLowerInvariant();
        var normalizedTarget = target.Trim().ToLowerInvariant();

        // Her algoritmayı dene, en iyisini seç
        var exactScore = ExactMatch(normalizedInput, normalizedTarget);
        var containsScore = ContainsMatch(normalizedInput, normalizedTarget);
        var fuzzyScore = FuzzyMatch(normalizedInput, normalizedTarget);

        if (exactScore == 100) return new MatchResult(100, Domain.Enums.MatchType.Exact);
        if (containsScore >= 80) return new MatchResult(containsScore, Domain.Enums.MatchType.Contains);
        if (fuzzyScore >= 60) return new MatchResult(fuzzyScore, Domain.Enums.MatchType.Fuzzy);

        return new MatchResult(fuzzyScore, Domain.Enums.MatchType.Fuzzy);
    }

    /// <summary>Tam eşleşme — "John Smith" == "John Smith"</summary>
    private static decimal ExactMatch(string input, string target)
        => input == target ? 100 : 0;

    /// <summary>İçerik eşleşmesi — "John" ⊂ "John Alexander Smith"</summary>
    private static decimal ContainsMatch(string input, string target)
    {
        if (target.Contains(input))
            // Input ne kadar uzunsa, skor o kadar yüksek
            return Math.Round((decimal)input.Length / target.Length * 100, 2);
        return 0;
    }

    /// <summary>
    /// Levenshtein Distance — iki string arasındaki düzenleme mesafesi.
    /// "Jon" → "John" = 1 ekleme = %75 benzerlik
    /// </summary>
    private static decimal FuzzyMatch(string input, string target)
    {
        var distance = LevenshteinDistance(input, target);
        var maxLen = Math.Max(input.Length, target.Length);
        if (maxLen == 0) return 100;
        return Math.Round((1 - (decimal)distance / maxLen) * 100, 2);
    }

    /// <summary>
    /// Levenshtein Distance algoritması.
    /// Bir string'i diğerine çevirmek için gereken minimum işlem sayısı.
    /// İşlemler: ekleme, silme, değiştirme.
    /// </summary>
    private static int LevenshteinDistance(string s, string t)
    {
        var m = s.Length;
        var n = t.Length;
        var d = new int[m + 1, n + 1];

        for (var i = 0; i <= m; i++) d[i, 0] = i;
        for (var j = 0; j <= n; j++) d[0, j] = j;

        for (var j = 1; j <= n; j++)
            for (var i = 1; i <= m; i++)
            {
                var cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

        return d[m, n];
    }
}

/// <summary>Bir eşleştirme işleminin sonucu.</summary>
public record MatchResult(decimal Score, WatchListScreening.Domain.Enums.MatchType MatchType);
