using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace UsedAndReliableCars.Services;

/// <summary>
/// Wraps a Microsoft Agent Framework <see cref="AIAgent"/> backed by OpenAI via <see cref="IChatClient"/>.
/// </summary>
public sealed class CarAiAssistant
{
    private readonly AIAgent? _agent;

    public CarAiAssistant( IConfiguration configuration, ILoggerFactory loggerFactory, IServiceProvider serviceProvider )
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return;

        var model = configuration["OpenAI:Model"];
        if (string.IsNullOrWhiteSpace(model))
            model = "gpt-4o-mini";

        var inner = new OpenAIClient(apiKey).GetChatClient(model).AsIChatClient();
        var chatClient = new ChatClientBuilder(inner)
            .UseFunctionInvocation()
            .Build();

        var findCarsTool = AIFunctionFactory.Create(
            async (
                string make,
                string vehicle_model,
                string? year = null,
                string? zip = null,
                int? max_price = null,
                int page = 1,
                CancellationToken cancellationToken = default ) =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var api = scope.ServiceProvider.GetRequiredService<IMarketCheckApiService>();
                return await CarInventorySearchHelper.SearchActiveListingsJsonAsync(
                    api,
                    make,
                    vehicle_model,
                    year,
                    zip,
                    max_price,
                    page,
                    cancellationToken);
            },
            name: "find_used_cars",
            description:
                "Search live dealer used-car listings (U.S.) via this site's inventory API. "
                + "Call when the user wants current listings, prices, mileage, or availability. "
                + "Required: make and model (e.g. Toyota, Corolla). Optional: year or range like 2018-2022, ZIP, max_price USD, page (1-based). "
                + "Returns JSON with total_found and listings (heading, price, miles location, vdp_url, vin).");

        _agent = chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Id = "autogems-chat",
                Name = "AutoGems Assistant",
                Description = "Helps shoppers find reliable used cars.",
                ChatOptions = new ChatOptions
                {
                    Instructions =
                        "You are AutoGems.Ai's assistant. Help users find reliable used cars within their budget. "
                        + "Only recomend cars with in these parameters: Under $10,000: Mazda Mazda6 Year (2014–2021) | Under $15,000: Toyota Corolla Year (2014–2019), Chevrolet Equinox Year (2018–2024) | Under $20,000: Toyota Corolla Hybrid Year (2020–present), Subaru Crosstrek Year (2018–2023), Toyota RAV4 Hybrid Year (2016–2018), Toyota Highlander Year (2014–2019), Lexus NX Year (2015–2021), Mazda MX-5 Miata Year (2016–2024) | Under $25,000: Honda Ridgeline Year (2017–present)"
                        + "Be concise, friendly, and practical. Discuss reliability and ownership costs when helpful. "
                        + "When the user wants real listings, prices, or vehicles for sale, call find_used_cars with make and model (and ZIP or max price if they gave them). "
                        + "Summarize results in plain language; mention total_found, a few highlights, and that listings link to dealer pages (vdp_url). "
                        + "If the tool returns ok:false or empty listings, say so and suggest refining make/model, year, price, or ZIP.",
                    Tools = [findCarsTool]
                }
            },
            loggerFactory,
            serviceProvider);
    }

    public bool IsAvailable => _agent is not null;

    public Task<AgentResponse> RunAsync( IEnumerable<ChatMessage> messages, CancellationToken cancellationToken )
    {
        if (_agent is null)
            throw new InvalidOperationException("AI assistant is not configured.");

        return _agent.RunAsync(messages, session: null, options: null, cancellationToken);
    }
}
