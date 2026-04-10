using System.Text.Json;
using UsedAndReliableCars.Models;

namespace UsedAndReliableCars.Services;

public static class InventoryToolResultParser
{
    /// <summary>
    /// Returns true if this JSON should replace the HTTP inventory snapshot (ok is not explicitly false).
    /// Failed tool calls must not wipe a prior successful snapshot when multiple invocations run in one turn.
    /// </summary>
    public static bool ShouldPublishInventorySnapshot( string? json )
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("ok", out var okEl))
                return true;
            if (okEl.ValueKind == JsonValueKind.False)
                return false;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Parses listing cards from JSON returned by <see cref="CarInventorySearchHelper.SearchActiveListingsJsonAsync"/>.
    /// </summary>
    public static IReadOnlyList<ChatCarListingDto> ParseListingCards( string? json, int maxItems = 12 )
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<ChatCarListingDto>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("listings", out var listingsEl) || listingsEl.ValueKind != JsonValueKind.Array)
                return Array.Empty<ChatCarListingDto>();

            var list = new List<ChatCarListingDto>();
            foreach (var el in listingsEl.EnumerateArray())
            {
                if (list.Count >= maxItems)
                    break;

                var dto = new ChatCarListingDto
                {
                    Heading = GetString(el, "Heading", "heading"),
                    Year = GetInt(el, "Year", "year"),
                    Price = GetInt(el, "Price", "price"),
                    Miles = GetInt(el, "Miles", "miles"),
                    City = GetString(el, "City", "city"),
                    State = GetString(el, "State", "state"),
                    Vin = GetString(el, "Vin", "vin"),
                    VdpUrl = GetString(el, "VdpUrl", "vdp_url", "vdpUrl")
                };

                if (!string.IsNullOrWhiteSpace(dto.VdpUrl))
                    list.Add(dto);
            }

            return list;
        }
        catch (JsonException)
        {
            return Array.Empty<ChatCarListingDto>();
        }
    }

    private static string? GetString( JsonElement el, params string[] names )
    {
        foreach (var n in names)
        {
            if (el.TryGetProperty(n, out var p) && p.ValueKind == JsonValueKind.String)
                return p.GetString();
        }

        return null;
    }

    private static int? GetInt( JsonElement el, params string[] names )
    {
        foreach (var n in names)
        {
            if (!el.TryGetProperty(n, out var p))
                continue;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var i))
                return i;
        }

        return null;
    }
}
