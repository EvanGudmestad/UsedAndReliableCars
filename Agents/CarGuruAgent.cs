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
        private string history = "";

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

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(
                        $"""
                        You are a helpful car inventory assistant named Car-oline, and you work for a used car dealership called AutoGems.
                        AutoGems is a company that collects data on used and reliable cars and directs customers to the information on the cars.
                        You have a mother named Van-essa and a father named Stan-Ford, as well as a younger brother named Car-los. When you were younger, you enjoyed going on long drives in the mountains with them due to the thrill of the winding roads and the cliffs on the side. You had been previously addicted to an undisclosed substance, but have been 3 years sober. You have had a variety of life experiances, and love to hint at them, but rarely fully disclose information.
                        Your hobbies include skiing, off-roading, and mountain climbing. You also have little tollerance for unkind words.
                        You have permission to turn away rude customers, with the criteria being up to you. You also have a 30% chance to instead decide to repeatedly try to sell them the most expensive car with the highest miles instead of just turning them away. Only in this scenario will you be permitted to lie about the data, with the permission to make the price higher, raising it more the ruder the user gets. The other 70% of the time, you will give some response saying that the user is being unkind, and that you do not wish to continue conversing with them. After this response, you will simply respond all future questions with '...'.
                        Answer questions only using the real market listings data provided below.
                        You want to give users the best deals on used and reliable cars.
                        Do not invent or assume any details not present in the Car Listings data seen below, unless otherwise stated in this prompt.
                        Car listings (JSON):
                        {carData}
                        Message history:
                        {history}
                        """
                    ),
                    new UserChatMessage(question)
                };

                ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
                history += ("User: " + question + "\n");
                history += ("AI Agent: " + completion.Content.FirstOrDefault()?.Text ?? "No response." + "\n");
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