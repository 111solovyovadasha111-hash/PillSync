using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillSync.Api.Ai;
using PillSync.Api.Ai.Dto;
using PillSync.Api.Data;

namespace PillSync.Api.Controllers;

[ApiController]
[Authorize] 
[Route("api/ai")]
public class AiController(
    AppDbContext db,
    IYandexAiClient ai,
    ILogger<AiController> log) : ControllerBase
{
    private const int MaxMedications = 20;

    [HttpPost("interaction-check")]
    [ProducesResponseType(typeof(InteractionCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckInteractions(
        [FromBody] InteractionCheckByIdsRequest request, CancellationToken ct)
    {
        var ids = request.MedicationIds?.Distinct().ToList() ?? [];

        if (ids.Count == 0)
            return BadRequest(new { error = "Передайте хотя бы один препарат." });
        if (ids.Count > MaxMedications)
            return BadRequest(new { error = $"Не больше {MaxMedications} препаратов за запрос." });

        // UserId только из JWT, никогда из тела запроса
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        // Фильтр по владельцу прямо в запросе
        var meds = await db.Medications
            .Where(m => m.UserId == userId && ids.Contains(m.Id))
            .AsNoTracking()
            .ToListAsync(ct);

        // Чужой и несуществующий ID неотличимы: один и тот же ответ
        if (meds.Count != ids.Count)
            return NotFound(new { error = "Один или несколько препаратов не найдены." });

        var aiRequest = new InteractionCheckRequest(
            meds.Select(m => new MedicationInput(m.Name, m.ActiveIngredient, m.Dose, m.DoseUnit)).ToList());

        try
        {
            var result = await ai.CheckInteractionsAsync(aiRequest, ct);
            return Ok(result);
        }
        catch (AiUnavailableException ex)
        {
            log.LogWarning(ex, "AI check failed");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Сервис проверки временно недоступен. Попробуйте позже." });
        }
    }


    // тест на нейронку
    [HttpPost("test-ai-direct")]
    [AllowAnonymous] 
    public async Task<IActionResult> TestAiDirect(CancellationToken ct)
    {
        // Создаем тестовый запрос напрямую, без базы данных
        var aiRequest = new InteractionCheckRequest(
        [
            new MedicationInput("Эликвис", "Апиксабан", "5", "мг"),
            new MedicationInput("Кардиомагнил", "Ацетилсалициловая кислота + Магния гидроксид", "75", "мг"),
            new MedicationInput("Клопидогрел", "Клопидогрел", "75", "мг"),
            new MedicationInput("Кеторол экспресс", "Кеторолак", "10", "мг")
        ]);

        try
        {
            // Напрямую дергаем YandexGPT через ваш клиент
            var result = await ai.CheckInteractionsAsync(aiRequest, ct);
            return Ok(result);
        }
        catch (AiUnavailableException ex)
        {
            log.LogWarning(ex, "AI check failed");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }
}