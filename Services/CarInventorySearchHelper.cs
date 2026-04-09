using System.Text.Json;
using System.Text.RegularExpressions;
using UsedAndReliableCars.Models;

namespace UsedAndReliableCars.Services;

/// <summary>
/// Runs the same style of MarketCheck active inventory search used on the site, returning JSON for tool callers.
/// </summary>
public static class CarInventorySearchHelper
{
    public static async Task<string> SearchActiveListingsJsonAsync(
        IMarketCheckApiService marketCheck,
        string make,
        string model,
        string? year,
        string? zip,
        int? maxPrice,
        int page,
        CancellationToken cancellationToken )
    {
        try
        {
            const int pageSize = 15;
            if (page < 1) page = 1;

            make = (make ?? "").Trim();
            model = (model ?? "").Trim();
            if (make.Length == 0 || model.Length == 0)
                return JsonSerializer.Serialize(new { ok = false, error = "make and model are required." });

            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["make"] = make,
                ["model"] = model
            };

            if (!string.IsNullOrWhiteSpace(year))
            {
                var yearStr = year.Trim();
                if (Regex.IsMatch(yearStr, @"^\d{4}\s*-\s*\d{4}$"))
                    queryParams["year_range"] = yearStr.Replace(" ", "", StringComparison.Ordinal);
                else if (yearStr.Contains("present", StringComparison.OrdinalIgnoreCase))
                {
                    var match = Regex.Match(yearStr, @"(\d{4})");
                    if (match.Success)
                        queryParams["year_range"] = $"{match.Groups[1].Value}-2030";
                }
                else
                    queryParams["year"] = yearStr;
            }

            if (!string.IsNullOrWhiteSpace(zip))
                queryParams["zip"] = zip.Trim();

            if (maxPrice is > 0)
                queryParams["price_range"] = $"0-{maxPrice.Value}";

            queryParams["rows"] = pageSize.ToString();
            queryParams["start"] = ((page - 1) * pageSize).ToString();

            var response = await marketCheck.SearchActiveAsync(queryParams, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return JsonSerializer.Serialize(new
                {
                    ok = false,
                    error = "Inventory search request failed.",
                    status = (int)response.StatusCode
                });
            }

            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<MarketCheckSearchResponse>(raw, options);
            if (parsed?.Listings is null)
                return JsonSerializer.Serialize(new { ok = true, total_found = 0, listings = Array.Empty<object?>() });

            var list = parsed.Listings;
            if (maxPrice is > 0)
                list = list.Where(l => !l.Price.HasValue || l.Price.Value <= maxPrice.Value).ToList();

            var summaries = list.Select(l => new
            {
                l.Heading,
                l.Year,
                l.Make,
                l.Model,
                l.Trim,
                l.Price,
                l.Miles,
                l.City,
                l.State,
                l.Zip,
                l.VdpUrl,
                l.Vin
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                ok = true,
                total_found = parsed.NumFound,
                page,
                page_size = pageSize,
                returned = summaries.Count,
                listings = summaries
            });
        }
        catch (Exception)
        {
            return JsonSerializer.Serialize(new { ok = false, error = "Inventory search failed unexpectedly." });
        }
    }
}
