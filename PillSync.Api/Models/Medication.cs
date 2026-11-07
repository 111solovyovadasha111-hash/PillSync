namespace PillSync.Api.Models;

public class Medication
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ActiveIngredient { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string DoseUnit { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}