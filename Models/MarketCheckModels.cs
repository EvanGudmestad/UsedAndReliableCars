using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace UsedAndReliableCars.Models
{
    public class MarketCheckSearchResponse
    {
        [JsonPropertyName("num_found")]
        public int NumFound { get; set; }

        [JsonPropertyName("listings")]
        public List<CarListing> Listings { get; set; } = new();
    }

    public class CarListing
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("vin")]
        public string? Vin { get; set; }

        [JsonPropertyName("heading")]
        public string? Heading { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("miles")]
        public int? Miles { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("make")]
        public string? Make { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("trim")]
        public string? Trim { get; set; }

        [JsonPropertyName("exterior_color")]
        public string? ExteriorColor { get; set; }

        [JsonPropertyName("interior_color")]
        public string? InteriorColor { get; set; }

        [JsonPropertyName("seller_name")]
        public string? SellerName { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("zip")]
        public string? Zip { get; set; }

        [JsonPropertyName("vdp_url")]
        public string? VdpUrl { get; set; }

        [JsonPropertyName("media")]
        public MediaInfo? Media { get; set; }

        /// <summary>
        /// First real photo URL (cached preferred, raw fallback).
        /// Returns null if no photos or only dealer placeholder images exist.
        /// </summary>
        [JsonIgnore]
        public string? FirstPhoto =>
            Media?.PhotoLinksCached?.FirstOrDefault(IsRealPhoto)
            ?? Media?.PhotoLinks?.FirstOrDefault(IsRealPhoto);

        private static bool IsRealPhoto(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            string[] placeholderPatterns =
            [
                "nophoto", "no_photo", "no-photo",
                "noimage", "no_image", "no-image",
                "newarrivalphoto", "notavailable", "not-available",
                "vehicle-image-notavailable", "coming-soon", "comingsoon",
                "defaultcar", "default_car", "placeholder",
                "stockphoto", "stock_photo", "stock-photo",
            ];
            var lower = url.ToLowerInvariant();
            return !Array.Exists(placeholderPatterns, p => lower.Contains(p));
        }
    }

    public class MediaInfo
    {
        [JsonPropertyName("photo_links_cached")]
        public List<string>? PhotoLinksCached { get; set; }

        [JsonPropertyName("photo_links")]
        public List<string>? PhotoLinks { get; set; }
    }

    public class VinHistoryRecord
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("miles")]
        public int? Miles { get; set; }

        [JsonPropertyName("heading")]
        public string? Heading { get; set; }

        [JsonPropertyName("seller_name")]
        public string? SellerName { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("first_seen_at")]
        public long? FirstSeenAt { get; set; }

        [JsonPropertyName("last_seen_at")]
        public long? LastSeenAt { get; set; }

        [JsonIgnore]
        public string? FirstSeenDate => FirstSeenAt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(FirstSeenAt.Value).LocalDateTime.ToShortDateString()
            : null;
    }

    public class CarSearchResultViewModel
    {
        public List<CarListing> Listings { get; set; } = new();
        public int TotalFound { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> PriceTrendByVin { get; set; } = new();

        /// <summary>1-based current page.</summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>Number of results per page.</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>Total number of pages (0 if no results).</summary>
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalFound / PageSize);

        /// <summary>Search params for building pagination links (e.g. selectedCar, year, make, location).</summary>
        public string? SelectedCar { get; set; }
        public string? Year { get; set; }
        public string? Make { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool Nationwide { get; set; }
    }

    public class PriceHistoryViewModel
    {
        public string? Vin { get; set; }
        public string? VehicleTitle { get; set; }
        public string? ErrorMessage { get; set; }
        public List<VinHistoryRecord> Records { get; set; } = new();
    }
}
