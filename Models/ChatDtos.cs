namespace UsedAndReliableCars.Models;

public sealed class ChatRequestDto
{
    public List<ChatTurnDto>? Messages { get; set; }
}

public sealed class ChatTurnDto
{
    public string? Role { get; set; }
    public string? Text { get; set; }
}

public sealed class ChatResponseDto
{
    public string? Reply { get; set; }
    public string? Error { get; set; }

    /// <summary>Listings from the latest inventory tool call in this response (for sidebar cards).</summary>
    public IReadOnlyList<ChatCarListingDto>? Listings { get; set; }
}

public sealed class ChatCarListingDto
{
    public string? Heading { get; set; }
    public int? Year { get; set; }
    public int? Price { get; set; }
    public int? Miles { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Vin { get; set; }
    public string? VdpUrl { get; set; }

    /// <summary>1-based order in the sidebar (best pick first when ranked).</summary>
    public int? PickRank { get; set; }
}
