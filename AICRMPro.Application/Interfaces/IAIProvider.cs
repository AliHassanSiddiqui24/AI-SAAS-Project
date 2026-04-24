namespace AICRMPro.Application.Interfaces;

public interface IAIProvider
{
    Task<AIProviderResponse> GenerateAsync(AIProviderRequest request);
    IAsyncEnumerable<string> GenerateStreamAsync(AIProviderRequest request);
}

public class AIProviderRequest
{
    public string SystemPrompt { get; set; }
    public string UserPrompt { get; set; }
    public string Model { get; set; }
    public int MaxTokens { get; set; } = 1000;
}

public class AIProviderResponse
{
    public string Content { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int LatencyMs { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}
