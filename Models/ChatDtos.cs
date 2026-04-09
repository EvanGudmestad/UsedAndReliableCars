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
}
