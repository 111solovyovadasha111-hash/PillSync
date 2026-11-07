namespace PillSync.Api.Models;

public class Analysis
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}