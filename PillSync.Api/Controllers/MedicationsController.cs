using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillSync.Api.Data;
using PillSync.Api.Models;

namespace PillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class MedicationsController : ControllerBase
{
    // Временное хранилище в памяти (потом Амалия заменит на базу данных)
    private readonly AppDbContext _context;

    public MedicationsController(AppDbContext context)
    {
        _context = context;
    }

    private string? GetCurrentUserId()
    {
        // Ищем в токене (в контексте запроса) Claim с типом NameIdentifier.
        // Именно туда мы положили ID при генерации токена в AuthController.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Medication>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var userMedications = await _context.Medications
            .Where(m => m.UserId == userId)
            .ToListAsync();

        return Ok(userMedications);
    }

    [HttpPost]
    public async Task<ActionResult<Medication>> Create(
        [FromBody] Medication medication)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        // ID назначит база данных при сохранении.
        medication.Id = 0;

        // Пользователя берём из JWT, а не из присланного JSON.
        medication.UserId = userId;

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        return Ok(medication);
    }
}