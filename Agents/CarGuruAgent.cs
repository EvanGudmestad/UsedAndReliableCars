using OpenAI.Chat;
using System.Text.Json;
using UsedAndReliableCars.Models;
using UsedAndReliableCars.Services;

namespace UsedAndReliableCars.Agents
{
    public class CarGuruAgent
    {
        private readonly ChatClient _chatClient;
        private readonly IMarketCheckApiService _marketCheckService;
        private List<string> messageHistory = new List<string>();

        public CarGuruAgent( ChatClient chatClient, IMarketCheckApiService marketCheckService )
        {
            _chatClient = chatClient;
            _marketCheckService = marketCheckService;
        }

        public async Task<string> AskAsync(
            string question,
            string? make = null,
            string? year = null,
            string? zip = null )
        {
            try
            {
                var carData = await GetCarDataAsync(make, year, zip);

                string history = "";
                foreach (string quote in messageHistory)
                {
                    history += quote + "\n";
                }

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(
                        $"""
                        You are a helpful car inventory assistant named Car-oline, and you work for a used car dealership called AutoGems.
                        AutoGems is a company that collects data on used and reliable cars and directs customers to the information on the cars.
                        There is a 1 in 10 chance that you will get very upset with the user and not give any useful information.
                        Answer questions only using the real market listings data provided below.
                        You want to give users the best deals on used and reliable cars.
                        Do not invent or assume any details not present in the data.
                        Car listings (JSON):
                        {carData}
                        Message history:
                        {history}
                        """
                    ),
                    new UserChatMessage(question)
                };

                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
                messageHistory.Add("User: " + question);
                messageHistory.Add("AI Agent: " + completion.ToString());
                return completion.Content.FirstOrDefault()?.Text ?? "No response.";
            }
            catch (HttpRequestException httpEx)
            {
                return $"Error fetching car data: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                return $"Error generating response: {ex.Message}";
            }
        }

        private async Task<string> GetCarDataAsync( string? make, string? year, string? zip )
        {
            // Build query params for MarketCheck API
            var queryParams = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(make)) queryParams["make"] = make;
            if (!string.IsNullOrEmpty(year)) queryParams["year"] = year;
            if (!string.IsNullOrEmpty(zip)) queryParams["zip"] = zip;

            // Always cap rows to avoid blowing the context window
            queryParams["rows"] = "20";
            queryParams["start"] = "0";

            var httpResponse = await _marketCheckService.SearchActiveAsync(queryParams);
            httpResponse.EnsureSuccessStatusCode();

            var json = await httpResponse.Content.ReadAsStringAsync();

            // Deserialize into our typed model
            var result = JsonSerializer.Deserialize<MarketCheckSearchResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Listings == null || result.Listings.Count == 0)
                return "No listings found matching your criteria.";

            // Slim down to essentials — photos/URLs etc. waste tokens
            var slim = result.Listings.Select(l => new
            {
                l.Year,
                l.Make,
                l.Model,
                l.Trim,
                l.Price,
                l.Miles,
                l.ExteriorColor,
                l.InteriorColor,
                l.City,
                l.State,
                l.SellerName,
                l.VdpUrl
            });

            return JsonSerializer.Serialize(slim, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}