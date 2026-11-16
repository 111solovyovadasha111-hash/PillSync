using System.ComponentModel.DataAnnotations;

namespace PillSync.Api.Models.Dtos;

public class MedicationCreateDto
{
    [Required(ErrorMessage = "Название препарата обязательно")]
    public string Name { get; set; } = string.Empty;

    public string ActiveIngredient { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дозировка обязательна")]
    public string Dose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Единица измерения обязательна")]
    public string DoseUnit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата начала приема обязательна")]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

public class MedicationUpdateDto
{
    [Required(ErrorMessage = "Название препарата обязательно")]
    public string Name { get; set; } = string.Empty;

    public string ActiveIngredient { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дозировка обязательна")]
    public string Dose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Единица измерения обязательна")]
    public string DoseUnit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата начала приема обязательна")]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}