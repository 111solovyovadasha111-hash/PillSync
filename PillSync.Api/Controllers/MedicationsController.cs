using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillSync.Api.Data;
using PillSync.Api.Models;
using PillSync.Api.Models.Dtos; 

namespace PillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class MedicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicationsController(AppDbContext context)
    {
        _context = context;
    }

    private string? GetCurrentUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }

    // 1. GET: Получить все препараты
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Medication>>> GetAll()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var userMedications = await _context.Medications
            .Where(m => m.UserId == userId)
            .ToListAsync();

        return Ok(userMedications);
    }

    // 2. POST: Создать препарат
    [HttpPost]
    public async Task<ActionResult<Medication>> Create([FromBody] MedicationCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        // Валидация периода приёма
        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            return BadRequest(new { error = "Дата окончания не может быть раньше даты начала." });

        var medication = new Medication
        {
            Name = dto.Name,
            ActiveIngredient = dto.ActiveIngredient,
            Dose = dto.Dose,
            DoseUnit = dto.DoseUnit,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            UserId = userId // Владельца берем из токена!
        };

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        return Ok(medication);
    }
    
    // 3. PUT: Обновить препарат
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MedicationUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        // Ищем препарат И проверяем, что он принадлежит текущему юзеру
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medication == null) return NotFound(new { error = "Препарат не найден." });

        // Валидация периода приёма
        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            return BadRequest(new { error = "Дата окончания не может быть раньше даты начала." });

        // Обновляем поля
        medication.Name = dto.Name;
        medication.ActiveIngredient = dto.ActiveIngredient;
        medication.Dose = dto.Dose;
        medication.DoseUnit = dto.DoseUnit;
        medication.StartDate = dto.StartDate;
        medication.EndDate = dto.EndDate;

        await _context.SaveChangesAsync();
        return NoContent(); // Стандартный ответ 204 (Успешно, без тела ответа)
    }

    // 4. DELETE: Удалить препарат
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        // Ищем препарат И проверяем, что он принадлежит текущему юзеру
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medication == null) return NotFound(new { error = "Препарат не найден." });

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();

        return NoContent(); 
    }
}