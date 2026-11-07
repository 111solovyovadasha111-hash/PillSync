namespace PillSync.Api.Models;

public class LabMeasurement
{
    public int Id { get; set; }
    public int AnalysisId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double ReferenceMin { get; set; }
    public double ReferenceMax { get; set; }
}