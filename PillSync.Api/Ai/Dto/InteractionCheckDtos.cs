using System.Text.Json.Serialization;

namespace PillSync.Api.Ai.Dto;
public record MedicationInput(
    string Name,
    string? ActiveIngredient,
    decimal? Dose,
    string? DoseUnit);

// сервер берет userId из JWT
public record InteractionCheckRequest(IReadOnlyList<MedicationInput> Medications);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InteractionSeverity { None, Minor, Moderate, Major }

public record DrugInteraction(
    string DrugA,
    string DrugB,
    InteractionSeverity Severity,
    string Description,
    string Recommendation);

public record IntakeScheduleItem(
    string Medication,
    string TimeOfDay,      // "08:00"
    string Instruction);   // "после еды"

public record InteractionCheckResult(
    IReadOnlyList<DrugInteraction> Interactions,
    IReadOnlyList<IntakeScheduleItem> Schedule)
{
    public const string DisclaimerText =
        "Информация носит справочный характер и не является медицинской рекомендацией. Проконсультируйтесь с врачом.";

    public string Disclaimer { get; init; } = DisclaimerText;
}
