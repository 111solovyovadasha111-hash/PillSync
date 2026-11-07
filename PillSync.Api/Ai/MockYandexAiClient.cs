namespace PillSync.Api.Ai;
using PillSync.Api.Ai.Dto;
public class MockYandexAiClient : IYandexAiClient
{
    public Task<InteractionCheckResult> CheckInteractionsAsync(
        InteractionCheckRequest request, CancellationToken ct = default)
    {
        var names = request.Medications.Select(m => m.Name.Trim().ToLowerInvariant()).ToList();
        var interactions = new List<DrugInteraction>();

        // кофликт лекарств для мока
        if (names.Contains("аспирин") && names.Contains("варфарин"))
            interactions.Add(new DrugInteraction("Аспирин", "Варфарин",
                InteractionSeverity.Major,
                "Повышенный риск кровотечений (тестовые данные).",
                "Не принимать вместе без назначения врача."));

        var schedule = request.Medications
            .Select((m, i) => new IntakeScheduleItem(m.Name, $"{8 + i * 4:00}:00", "после еды"))
            .ToList();

        return Task.FromResult(new InteractionCheckResult(interactions, schedule));
    }
}