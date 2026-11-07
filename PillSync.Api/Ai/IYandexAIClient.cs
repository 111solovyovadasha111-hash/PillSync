using PillSync.Api.Ai.Dto;

namespace PillSync.Api.Ai;

public interface IYandexAiClient
{
    Task<InteractionCheckResult> CheckInteractionsAsync(
        InteractionCheckRequest request, CancellationToken ct = default);
}