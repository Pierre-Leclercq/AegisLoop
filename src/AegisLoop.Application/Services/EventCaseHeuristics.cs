using System.Globalization;
using System.Text;
using System.Text.Json;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Services;

/// <summary>
/// Heuristiques V1 documentées, déterministes et sans ML/NLP avancé.
/// Regroupement EventCase : deux observations sont proches si elles ont au moins
/// 2 mots significatifs communs, une similarité Jaccard >= 0.20 et au plus 72h d'écart.
/// Les observations trop courtes, sans recouvrement lexical clair ou hors fenêtre temporelle
/// ne sont pas regroupées.
/// </summary>
public static class EventCaseHeuristics
{
    public const int TimeWindowHours = 72;
    public const double TextSimilarityThreshold = 0.20;
    public const int MinimumCommonTerms = 2;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "from", "with", "this", "that", "into", "over", "after", "before", "about",
        "une", "des", "les", "dans", "avec", "pour", "sur", "par", "aux", "du", "de", "la", "le", "un",
        "near", "says", "said", "new", "update", "breaking", "report", "reports", "selon", "apres", "avant"
    };

    public static IReadOnlySet<string> SignificantTerms(Observation observation)
    {
        var text = $"{observation.Title} {observation.Content} {observation.ClaimText}";
        var normalized = RemoveDiacritics(text).ToLowerInvariant();
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = new StringBuilder();

        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch))
            {
                current.Append(ch);
                continue;
            }

            AddCurrent(current, terms);
        }

        AddCurrent(current, terms);
        return terms;
    }

    public static bool AreClose(Observation left, Observation right)
    {
        if (Math.Abs((left.ObservedAt - right.ObservedAt).TotalHours) > TimeWindowHours)
        {
            return false;
        }

        var leftTerms = SignificantTerms(left);
        var rightTerms = SignificantTerms(right);
        var commonTerms = leftTerms.Intersect(rightTerms, StringComparer.OrdinalIgnoreCase).Count();
        if (commonTerms < MinimumCommonTerms)
        {
            return false;
        }

        return Jaccard(leftTerms, rightTerms) >= TextSimilarityThreshold;
    }

    public static double Jaccard(IReadOnlySet<string> left, IReadOnlySet<string> right)
    {
        if (left.Count == 0 || right.Count == 0)
        {
            return 0.0;
        }

        var intersection = left.Intersect(right, StringComparer.OrdinalIgnoreCase).Count();
        var union = left.Union(right, StringComparer.OrdinalIgnoreCase).Count();
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    public static string BuildEventTitle(IReadOnlyList<Observation> observations)
    {
        var representative = observations
            .OrderByDescending(o => SignificantTerms(o).Count)
            .ThenBy(o => o.ObservedAt)
            .First();
        return representative.Title.Length <= 180 ? representative.Title : representative.Title[..180];
    }

    public static EventCategory InferCategory(IEnumerable<Observation> observations)
    {
        var terms = observations.SelectMany(SignificantTerms).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (terms.Overlaps(new[] { "conflict", "war", "attack", "strike", "combat", "combats", "crisis", "crise", "incident" })) return EventCategory.Conflict;
        if (terms.Overlaps(new[] { "flood", "earthquake", "fire", "disaster", "seisme", "inondation", "catastrophe" })) return EventCategory.Disaster;
        if (terms.Overlaps(new[] { "election", "government", "minister", "diplomatic", "political", "diplomatique", "ministre" })) return EventCategory.Political;
        if (terms.Overlaps(new[] { "market", "trade", "economic", "economy", "commerce", "marche" })) return EventCategory.Economic;
        if (terms.Overlaps(new[] { "climate", "pollution", "environment", "environnement" })) return EventCategory.Environmental;
        return EventCategory.Other;
    }

    public static string? MetadataValue(Observation observation, string key)
    {
        if (string.IsNullOrWhiteSpace(observation.MetadataJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(observation.MetadataJson);
            return document.RootElement.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void AddCurrent(StringBuilder current, HashSet<string> terms)
    {
        if (current.Length == 0)
        {
            return;
        }

        var term = current.ToString();
        current.Clear();
        if (term.Length >= 4 && !StopWords.Contains(term))
        {
            terms.Add(term);
        }
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}