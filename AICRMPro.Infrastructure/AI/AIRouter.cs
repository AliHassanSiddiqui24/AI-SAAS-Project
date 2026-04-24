using AICRMPro.Application.Interfaces;
using AICRMPro.Domain.Entities;
using AICRMPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AICRMPro.Infrastructure.AI;

public class AIRouter
{
    private readonly GroqProvider _groqProvider;
    private readonly OpenAIProvider _openAIProvider;
    private readonly AppDbContext _dbContext;
    private readonly ICurrentTenant _currentTenant;

    public AIRouter(GroqProvider groqProvider, OpenAIProvider openAIProvider, AppDbContext dbContext, ICurrentTenant currentTenant)
    {
        _groqProvider = groqProvider;
        _openAIProvider = openAIProvider;
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<AIProviderResponse> GenerateAsync(AIProviderRequest request, AIFeature feature, Guid userId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        AIProviderResponse response = null!;
        AIProvider usedProvider = AIProvider.Groq;

        try
        {
            // Try Groq first
            response = await _groqProvider.GenerateAsync(request);
            if (response.IsSuccess)
            {
                usedProvider = AIProvider.Groq;
            }
            else
            {
                // Try OpenAI as fallback
                response = await _openAIProvider.GenerateAsync(request);
                if (response.IsSuccess)
                {
                    usedProvider = AIProvider.OpenAI;
                }
                else
                {
                    // Both failed
                    throw new AIServiceUnavailableException("All AI providers failed");
                }
            }

            // Log successful request
            await LogAIRequestAsync(request, response, feature, userId, usedProvider, true);
            return response;
        }
        catch (AIServiceUnavailableException)
        {
            // Log both provider failures
            await LogAIRequestAsync(request, response, feature, userId, AIProvider.Groq, false);
            await LogAIRequestAsync(request, response, feature, userId, AIProvider.OpenAI, false);
            throw;
        }
        catch (Exception ex)
        {
            // Log the failed attempt
            await LogAIRequestAsync(request, response, feature, userId, usedProvider, false, ex.Message);
            throw new AIServiceUnavailableException($"AI service error: {ex.Message}", ex);
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(AIProviderRequest request, AIFeature feature, Guid userId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        AIProvider usedProvider = AIProvider.Groq;
        bool success = false;
        string errorMessage = string.Empty;

        try
        {
            // Try Groq first
            await foreach (var chunk in _groqProvider.GenerateStreamAsync(request))
            {
                if (chunk.StartsWith("Error:") || chunk.Contains("failed"))
                {
                    // Groq failed, try OpenAI
                    break;
                }
                success = true;
                yield return chunk;
            }

            if (!success)
            {
                usedProvider = AIProvider.OpenAI;
                await foreach (var chunk in _openAIProvider.GenerateStreamAsync(request))
                {
                    if (chunk.StartsWith("Error:") || chunk.Contains("failed"))
                    {
                        errorMessage = chunk;
                        throw new AIServiceUnavailableException("All AI providers failed");
                    }
                    success = true;
                    yield return chunk;
                }
            }

            if (!success)
            {
                throw new AIServiceUnavailableException("All AI providers failed");
            }
        }
        catch (AIServiceUnavailableException)
        {
            // Log both provider failures
            await LogAIRequestAsync(request, null, feature, userId, AIProvider.Groq, false, errorMessage);
            await LogAIRequestAsync(request, null, feature, userId, AIProvider.OpenAI, false, errorMessage);
            throw;
        }
        catch (Exception ex)
        {
            // Log the failed attempt
            await LogAIRequestAsync(request, null, feature, userId, usedProvider, false, ex.Message);
            throw new AIServiceUnavailableException($"AI streaming error: {ex.Message}", ex);
        }
    }

    private async Task LogAIRequestAsync(AIProviderRequest request, AIProviderResponse? response, 
        AIFeature feature, Guid userId, AIProvider provider, bool success, string? errorMessage = null)
    {
        try
        {
            var aiRequest = new AIRequest
            {
                Id = Guid.NewGuid(),
                TenantId = _currentTenant.TenantId,
                UserId = userId,
                Feature = feature,
                Provider = provider,
                Model = request.Model ?? (provider == AIProvider.Groq ? "llama-3.1-8b-instant" : "gpt-4o-mini"),
                PromptTokens = response?.PromptTokens ?? 0,
                CompletionTokens = response?.CompletionTokens ?? 0,
                TotalTokens = (response?.PromptTokens ?? 0) + (response?.CompletionTokens ?? 0),
                CostUSD = CalculateCostEstimate(response?.PromptTokens ?? 0, response?.CompletionTokens ?? 0),
                LatencyMs = response?.LatencyMs ?? 0,
                IsSuccess = success,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AIRequests.Add(aiRequest);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't throw to avoid disrupting the main flow
            // In production, you'd want proper logging here
            Console.WriteLine($"Failed to log AI request: {ex.Message}");
        }
    }

    private decimal CalculateCostEstimate(int promptTokens, int completionTokens)
    {
        // Cost estimate: $0.0001 per 1000 tokens (approximation for logging)
        var totalTokens = promptTokens + completionTokens;
        return (decimal)totalTokens / 1000 * 0.0001m;
    }
}
