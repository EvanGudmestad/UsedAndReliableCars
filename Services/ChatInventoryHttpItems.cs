namespace UsedAndReliableCars.Services;

/// <summary>
/// Key used to pass the latest inventory JSON from AI tool execution to the chat HTTP response.
/// </summary>
public static class ChatInventoryHttpItems
{
    public const string ListingsJsonKey = "__AiInventoryListingsJson";
}
