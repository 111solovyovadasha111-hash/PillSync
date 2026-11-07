using Microsoft.AspNetCore.Mvc;
using PillSync.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace PillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class MedicationsController : ControllerBase
{
    // Временное хранилище в памяти (потом Амалия заменит на базу данных)
    private static readonly List<Medication> _medications = new();
    
    private string? GetCurrentUserId()
    {
        // Ищем в токене (в контексте запроса) Claim с типом NameIdentifier.
        // Именно туда мы положили ID при генерации токена в AuthController.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Medication>> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(); // Если токена нет
        var userMedications = _medications.Where(m => m.UserId == userId).ToList();
        return Ok(userMedications);
    }

    [HttpPost]
    public ActionResult<Medication> Create([FromBody] Medication medication)
    {
        // ВАЖНО: ID пользователя берем с сервера, а не из присланного JSON!
        medication.UserId = GetCurrentUserId();
        
        // Генерируем простой ID
        medication.Id = _medications.Count > 0 ? _medications.Max(m => m.Id) + 1 : 1;
        
        _medications.Add(medication);
        
        // Возвращаем созданный объект
        return Ok(medication);
    }
}