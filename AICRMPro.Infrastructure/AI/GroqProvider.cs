using System.Text.Json;
using AICRMPro.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AICRMPro.Infrastructure.AI;

public class GroqProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Groq:ApiKey"] ?? throw new ArgumentNullException("Groq:ApiKey not configured");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var requestBody = new
            {
                model = request.Model ?? "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "system", content = request.SystemPrompt },
                    new { role = "user", content = request.UserPrompt }
                },
                max_tokens = request.MaxTokens,
                temperature = 0.7,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BaseUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new InvalidOperationException("Groq API rate limit exceeded. Please try again later.");
                }

                return new AIProviderResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Groq API error: {response.StatusCode} - {responseString}",
                    LatencyMs = (int)stopwatch.ElapsedMilliseconds
                };
            }

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;
            
            var choice = root.GetProperty("choices")[0];
            var message = choice.GetProperty("message");
            var contentText = message.GetProperty("content").GetString();
            
            var usage = root.GetProperty("usage");
            var promptTokens = usage.GetProperty("prompt_tokens").GetInt32();
            var completionTokens = usage.GetProperty("completion_tokens").GetInt32();

            return new AIProviderResponse
            {
                Content = contentText ?? string.Empty,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                LatencyMs = (int)stopwatch.ElapsedMilliseconds,
                IsSuccess = true
            };
        }
        catch (HttpRequestException ex)
        {
            return new AIProviderResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Groq HTTP request failed: {ex.Message}",
                LatencyMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new AIProviderResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Groq provider error: {ex.Message}",
                LatencyMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(AIProviderRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var requestBody = new
            {
                model = request.Model ?? "llama-3.1-8b-instant",
                messages = new[]
                {
                    new { role = "system", content = request.SystemPrompt },
                    new { role = "user", content = request.UserPrompt }
                },
                max_tokens = request.MaxTokens,
                temperature = 0.7,
                stream = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BaseUrl, content, HttpCompletionOption.ResponseHeadersRead);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    throw new InvalidOperationException("Groq API rate limit exceeded. Please try again later.");
                }

                yield return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6);
                    if (data == "[DONE]") yield break;

                    try
                    {
                        using var doc = JsonDocument.Parse(data);
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var choice = choices[0];
                            if (choice.TryGetProperty("delta", out var delta) && delta.TryGetProperty("content", out var contentElement))
                            {
                                var contentText = contentElement.GetString();
                                if (!string.IsNullOrEmpty(contentText))
                                {
                                    yield return contentText;
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip malformed JSON
                        continue;
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            yield return $"HTTP request failed: {ex.Message}";
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            yield return $"Groq streaming error: {ex.Message}";
        }
    }
}
