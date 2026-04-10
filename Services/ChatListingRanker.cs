using System.Text.RegularExpressions;
using UsedAndReliableCars.Models;

namespace UsedAndReliableCars.Services;

/// <summary>
/// Orders inventory cards to mirror the assistant's narrative: VINs mentioned in reply order first, then heuristic.
/// </summary>
public static class ChatListingRanker
{
    private static readonly Regex VinRegex = new(@"\b[A-HJ-NPR-Z0-9]{17}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static List<ChatCarListingDto> RankForSidebar( string? assistantText, IReadOnlyList<ChatCarListingDto> parsed )
    {
        var list = parsed.ToList();
        if (list.Count == 0)
            return list;

        if (string.IsNullOrWhiteSpace(assistantText))
        {
            ApplyHeuristicAndRanks(list);
            return list;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var vinOrder = new List<string>();
        foreach (Match m in VinRegex.Matches(assistantText))
        {
            var v = m.Value.ToUpperInvariant();
            if (seen.Add(v))
                vinOrder.Add(v);
        }

        if (vinOrder.Count == 0)
        {
            ApplyHeuristicAndRanks(list);
            return list;
        }

        var byVin = list
            .Where(l => !string.IsNullOrWhiteSpace(l.Vin))
            .GroupBy(l => l.Vin!.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var ranked = new List<ChatCarListingDto>();
        foreach (var vin in vinOrder)
        {
            if (byVin.TryGetValue(vin, out var row))
            {
                ranked.Add(row);
                byVin.Remove(vin);
            }
        }

        foreach (var row in list)
        {
            if (!ranked.Contains(row))
                ranked.Add(row);
        }

        for (var i = 0; i < ranked.Count; i++)
            ranked[i].PickRank = i + 1;

        return ranked;
    }

    private static void ApplyHeuristicAndRanks( List<ChatCarListingDto> list )
    {
        var ordered = list
            .OrderBy(l => l.Price ?? int.MaxValue)
            .ThenBy(l => l.Miles ?? int.MaxValue)
            .ThenByDescending(l => l.Year ?? 0)
            .ToList();

        list.Clear();
        list.AddRange(ordered);
        for (var i = 0; i < list.Count; i++)
            list[i].PickRank = i + 1;
    }
}
