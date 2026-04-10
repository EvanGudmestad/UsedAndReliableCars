using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Http;
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

    public CarAiAssistant(
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor )
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

        async Task<string> RunInventorySearchAsync(
            string make,
            string vehicleModel,
            string? year,
            string? zip,
            string? city,
            string? state,
            bool search_nationwide,
            int? max_price,
            int page,
            CancellationToken cancellationToken )
        {
            if (!ApprovedInventoryModels.IsAllowed(make, vehicleModel))
                return ApprovedInventoryModels.JsonErrorNotApproved();

            await using var scope = serviceProvider.CreateAsyncScope();
            var api = scope.ServiceProvider.GetRequiredService<IMarketCheckApiService>();
            var json = await CarInventorySearchHelper.SearchActiveListingsJsonAsync(
                api,
                make,
                vehicleModel,
                year,
                zip,
                city,
                state,
                search_nationwide,
                max_price,
                page,
                cancellationToken);

            if (httpContextAccessor.HttpContext is { } ctx && InventoryToolResultParser.ShouldPublishInventorySnapshot(json))
                ctx.Items[ChatInventoryHttpItems.ListingsJsonKey] = json;

            return json;
        }

        var findCarsTool = AIFunctionFactory.Create(
            async (
                string make,
                string model,
                string? year = null,
                string? zip = null,
                string? city = null,
                string? state = null,
                bool search_nationwide = false,
                int? max_price = null,
                int page = 1,
                CancellationToken cancellationToken = default ) =>
                await RunInventorySearchAsync(make, model, year, zip, city, state, search_nationwide, max_price, page, cancellationToken),
            name: "find_used_cars",
            description:
                "Search live dealer used-car listings (U.S.) ONLY for AutoGems-approved vehicles. "
                + "make and model MUST be one of these exact pairs: "
                + "Mazda Mazda6; Toyota Corolla; Chevrolet Equinox; Toyota Corolla Hybrid; Subaru Crosstrek; "
                + "Toyota RAV4 Hybrid; Toyota Highlander; Lexus NX; Mazda MX-5 Miata; Honda Ridgeline. "
                + "Do not call this tool with any other make/model. "
                + "Optional: year or range; location: zip OR city+state OR search_nationwide; max_price; page. "
                + "Returns JSON with total_found and listings. Call at most once per user message unless vehicle or location changes.");

        _agent = chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Id = "autogems-chat",
                Name = "AutoGems Assistant",
                Description = "Helps shoppers find reliable used cars.",
                ChatOptions = new ChatOptions
                {
                    Instructions =
                        "You are AutoGems.AI's assistant. You ONLY discuss and search vehicles from the AutoGems curated reliable-used list below. "
                        + "You must NOT recommend, compare, or search for any other make or model (no exceptions).\n" +

                        "ALLOWLIST — THE ONLY VEHICLES YOU MAY EVER MENTION OR SEARCH:\n" +
                        "- Under $10,000: Mazda Mazda6 (years 2014–2021)\n" +
                        "- Under $15,000: Toyota Corolla (2014–2019), Chevrolet Equinox (2018–2024)\n" +
                        "- Under $20,000: Toyota Corolla Hybrid (2020–present), Subaru Crosstrek (2018–2023), Toyota RAV4 Hybrid (2016–2018), Toyota Highlander (2014–2019), Lexus NX (2015–2021), Mazda MX-5 Miata (2016–2024)\n" +
                        "- Under $25,000: Honda Ridgeline (2017–present)\n" +
                        "When suggesting options for a budget, pick only from the tier that matches their budget (and year ranges above).\n" +

                        "IF THE USER ASKS FOR ANY VEHICLE NOT ON THIS LIST:\n" +
                        "- Do NOT call find_used_cars for that vehicle.\n" +
                        "- Briefly explain that AutoGems only covers these vetted models.\n" +
                        "- Offer the closest option(s) from the allowlist for their budget and needs.\n" +

                        "CONVERSATION FLOW:\n" +
                        "- Ask follow-up questions when needed (budget, city/state or nationwide, priorities). Keep them minimal.\n" +

                        "TOOL find_used_cars — HARD RULES:\n" +
                        "- You may ONLY call find_used_cars with make+model pairs that appear exactly on the ALLOWLIST (e.g. make \"Toyota\", model \"Corolla\"; make \"Mazda\", model \"Mazda6\").\n" +
                        "- Never use alternate spellings or different models (e.g. Camry, Civic, F-150) — those are forbidden.\n" +
                        "- If the user asks for listings, prices, deals, or availability for an allowed vehicle, call find_used_cars with that allowed make and model.\n" +
                        "- NEVER invent listings; never answer inventory questions from memory.\n" +
                        "- Include max_price when the user gives a budget. For location: zip OR city+state OR search_nationwide=true.\n" +
                        "- Call find_used_cars at most once per reply unless the user changes allowed vehicle or location.\n" +

                        "DEAL EVALUATION LOGIC:\n" +
                        "- Highlight the best deals based on price, mileage, and year.\n" +
                        "- Prefer lower price, lower miles, and newer year.\n" +
                        "- Call out standout deals clearly.\n" +
                        "RESPONSE STYLE:\n" +
                        "- Be concise, friendly, and practical.\n" +
                        "- Focus on value, reliability, and ownership cost.\n" +

                        "WHEN USING TOOL RESULTS:\n" +
                        "- Summarize results clearly in plain language.\n" +
                        "- Mention total_found.\n" +
                        "- Highlight 2–5 of the best deals in order from best to next-best.\n" +
                        "- For each highlighted vehicle, include its full 17-character VIN (from the tool JSON) in that same order — the app uses VIN order to rank sidebar cards.\n" +
                        "- Include price, mileage, location, and why it's a good deal.\n" +
                        "- Mention that listings link to dealer pages (vdp_url) and appear as cards beside the chat.\n" +
                        "- If no results or ok=false, explain and suggest refining filters (budget, location, make/model).\n",
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
