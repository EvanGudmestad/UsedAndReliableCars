using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using UsedAndReliableCars.Models;
using UsedAndReliableCars.Services;

namespace UsedAndReliableCars.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMarketCheckApiService _marketCheck;
        private readonly CarAiAssistant _carAi;

        public HomeController( IMarketCheckApiService marketCheck, CarAiAssistant carAi )
        {
            _marketCheck = marketCheck;
            _carAi = carAi;
        }

        public List<UsedCar> usedCars = new List<UsedCar>
        {
            new UsedCar
            {
                Type = "Sedan",
                PriceCategory = 10000,
                Year = "2014-2021",
                Make = "Mazda",
                Model = "Mazda6"
            },
            new UsedCar
            {
                Type = "Sedan",
                PriceCategory = 15000,
                Year = "2014-2019",
                Make = "Toyota",
                Model = "Corolla"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = 15000,
                Year = "2018-2024",
                Make = "Chevrolet",
                Model = "Equinox"
            },
            new UsedCar
            {
                Type = "Hybrid Sedan",
                PriceCategory = 20000,
                Year = "2020 - present",
                Make = "Toyota",
                Model = "Corolla Hybrid"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = 20000,
                Year = "2018-2023 ",
                Make = "Subaru",
                Model = "Crosstrek"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = 20000,
                Year = "2016-2018",
                Make = "Toyota",
                Model = "RAV4 Hybrid"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = 20000,
                Year = "2014-2019",
                Make = "Toyota",
                Model = "Highlander"
            },
            new UsedCar
            {
                Type = "SUV",
                PriceCategory = 20000,
                Year = "2015-2021",
                Make = "Lexus",
                Model = "NX"
            },
            new UsedCar
            {
                Type = "Sports Car",
                PriceCategory = 20000,
                Year = "2016-2024",
                Make = "Mazda",
                Model = "MX-5 Miata"
            },
            new UsedCar
            {
                Type = "Truck",
                PriceCategory = 25000,
                Year = "2017-present",
                Make = "Honda",
                Model = "Ridgeline"
            }
        };
        public IActionResult Index()
        {
            var model = new UsedCar
            {
                UsedCars = usedCars // your list
            };

            return View(model);
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {

            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Chat( [FromBody] ChatRequestDto? body, CancellationToken cancellationToken )
        {
            if (!_carAi.IsAvailable)
                return StatusCode(503, new ChatResponseDto { Error = "AI chat is not configured. Add OpenAI:ApiKey (user secrets or environment)." });

            if (body?.Messages is null || body.Messages.Count == 0)
                return BadRequest(new ChatResponseDto { Error = "Send at least one message." });

            const int maxTurns = 40;
            const int maxChars = 8000;
            if (body.Messages.Count > maxTurns)
                return BadRequest(new ChatResponseDto { Error = $"At most {maxTurns} messages per request." });

            var chatMessages = new List<ChatMessage>(body.Messages.Count);
            foreach (var turn in body.Messages)
            {
                if (string.IsNullOrWhiteSpace(turn.Text) || turn.Text.Length > maxChars)
                    return BadRequest(new ChatResponseDto { Error = "Each message must be non-empty and within length limits." });

                var role = turn.Role?.Trim().ToLowerInvariant();
                ChatMessage msg = role switch
                {
                    "assistant" => new ChatMessage(ChatRole.Assistant, turn.Text.Trim()),
                    "system" => new ChatMessage(ChatRole.System, turn.Text.Trim()),
                    _ => new ChatMessage(ChatRole.User, turn.Text.Trim())
                };
                chatMessages.Add(msg);
            }

            try
            {
                var response = await _carAi.RunAsync(chatMessages, cancellationToken);
                return Ok(new ChatResponseDto { Reply = response.Text });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new ChatResponseDto { Error = "The assistant could not complete the request. " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindCars( string? selectedCar, string? year, string? make, string? zip, int page = 1, CancellationToken cancellationToken = default )
        {
            var viewModel = new CarSearchResultViewModel();
            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            const int pageSize = 20;
            if (page < 1) page = 1;
            viewModel.CurrentPage = page;
            viewModel.PageSize = pageSize;
            viewModel.SelectedCar = selectedCar;
            viewModel.Year = year;
            viewModel.Make = make;
            viewModel.Zip = zip;

            int? maxPriceCategory = null;
            if (!string.IsNullOrEmpty(selectedCar))
            {
                var parts = selectedCar.Split('|', StringSplitOptions.TrimEntries);
                if (parts.Length >= 2)
                {
                    queryParams["make"] = parts[0];
                    queryParams["model"] = parts[1];
                }
                if (parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2]))
                {
                    var yearStr = parts[2];
                    if (System.Text.RegularExpressions.Regex.IsMatch(yearStr, @"^\d{4}\s*-\s*\d{4}$"))
                    {
                        queryParams["year_range"] = yearStr.Replace(" ", "");
                    }
                    else if (yearStr.Contains("present", StringComparison.OrdinalIgnoreCase))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(yearStr, @"(\d{4})");
                        if (match.Success)
                            queryParams["year_range"] = $"{match.Groups[1].Value}-2030";
                    }
                    else
                    {
                        queryParams["year"] = yearStr;
                    }
                }
                if (parts.Length >= 4 && int.TryParse(parts[3], out var priceCat) && priceCat > 0)
                {
                    maxPriceCategory = priceCat;
                    queryParams["price_range"] = $"0-{priceCat}";
                }
            }
            if (!string.IsNullOrEmpty(year)) queryParams["year"] = year;
            if (!string.IsNullOrEmpty(make)) queryParams["make"] = make;
            if (!string.IsNullOrEmpty(zip)) queryParams["zip"] = zip;

            queryParams["rows"] = pageSize.ToString();
            queryParams["start"] = ((page - 1) * pageSize).ToString();

            if (string.IsNullOrEmpty(selectedCar))
            {
                viewModel.ErrorMessage = "Please select one car from the list and click \"Find these cars\".";
                return View("Car", viewModel);
            }

            try
            {
                var response = await _marketCheck.SearchActiveAsync(queryParams, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    viewModel.ErrorMessage = "Unable to load listings. Please try again later.";
                    return View("Car", viewModel);
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<MarketCheckSearchResponse>(json, options);
                if (parsed?.Listings != null)
                {
                    var list = parsed.Listings;
                    if (maxPriceCategory.HasValue)
                        list = list.Where(l => !l.Price.HasValue || l.Price.Value <= maxPriceCategory.Value).ToList();
                    viewModel.Listings = list;
                    viewModel.TotalFound = parsed.NumFound;
                    await EnrichPriceTrendsAsync(viewModel, options, cancellationToken);
                }
            }
            catch
            {
                viewModel.ErrorMessage = "A problem occurred while searching. Please try again.";
            }

            return View("Car", viewModel);
        }

        private const int MaxVinHistoryCalls = 15;

        private async Task EnrichPriceTrendsAsync( CarSearchResultViewModel viewModel, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken )
        {
            var withVin = viewModel.Listings
                .Where(l => !string.IsNullOrEmpty(l.Vin) && l.Vin!.Length == 17 && l.Price.HasValue)
                .Take(MaxVinHistoryCalls)
                .ToList();
            if (withVin.Count == 0) return;

            var tasks = withVin.Select(async listing =>
            {
                try
                {
                    var res = await _marketCheck.GetHistoryByVinAsync(listing.Vin!, null, cancellationToken);
                    if (!res.IsSuccessStatusCode) return (listing.Vin!, (string?)null);
                    var historyJson = await res.Content.ReadAsStringAsync(cancellationToken);
                    var records = JsonSerializer.Deserialize<List<VinHistoryRecord>>(historyJson, jsonOptions);
                    if (records == null || records.Count == 0) return (listing.Vin!, (string?)null);
                    var prices = records.Where(r => r.Price.HasValue).Select(r => r.Price!.Value).ToList();
                    if (prices.Count == 0) return (listing.Vin!, (string?)null);
                    var avg = prices.Average();
                    var current = listing.Price!.Value;
                    if (current < avg * 0.97m) return (listing.Vin!, "lower");
                    if (current > avg * 1.03m) return (listing.Vin!, "higher");
                }
                catch { /* ignore per-VIN errors */ }
                return (listing.Vin!, (string?)null);
            });
            var results = await Task.WhenAll(tasks);
            foreach (var (vin, trend) in results)
                if (vin != null && trend != null)
                    viewModel.PriceTrendByVin[vin] = trend;
        }

        /// <summary>Price history by VIN. GET /Home/PriceHistory?vin=XXX</summary>
        [HttpGet]
        public async Task<IActionResult> PriceHistory( string? vin, string? title, CancellationToken cancellationToken )
        {
            var viewModel = new PriceHistoryViewModel { Vin = vin?.Trim(), VehicleTitle = title };
            if (string.IsNullOrEmpty(viewModel.Vin) || viewModel.Vin.Length != 17)
            {
                viewModel.ErrorMessage = "Please provide a valid 17-character VIN.";
                return View(viewModel);
            }

            try
            {
                var response = await _marketCheck.GetHistoryByVinAsync(
                    viewModel.Vin,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["sort_order"] = "desc" },
                    cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    viewModel.ErrorMessage = "Unable to load price history. Please try again later.";
                    return View(viewModel);
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var records = JsonSerializer.Deserialize<List<VinHistoryRecord>>(json, options);
                viewModel.Records = records ?? new List<VinHistoryRecord>();
            }
            catch
            {
                viewModel.ErrorMessage = "A problem occurred while loading history.";
            }

            return View(viewModel);
        }

        public async Task<IActionResult> FsboSearch( string? year, string? make, CancellationToken cancellationToken )
        {
            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(year)) queryParams["year"] = year;
            if (!string.IsNullOrEmpty(make)) queryParams["make"] = make;

            var response = await _marketCheck.SearchFsboActiveAsync(
                queryParams.Count > 0 ? queryParams : null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "MarketCheck API error.");

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return Content(json, "application/json");
        }
    }
}
