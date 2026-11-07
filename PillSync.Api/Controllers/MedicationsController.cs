using Microsoft.AspNetCore.Mvc;
using PillSync.Api.Models;

namespace PillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicationsController : ControllerBase
{
    // Временное хранилище в памяти (потом Амалия заменит на базу данных)
    private static readonly List<Medication> _medications = new();

    // Временный метод для получения ID пользователя (пока логина нет, возвращает 1)
    private int GetCurrentUserId()
    {
        // Когда сделаем логин, здесь будет: int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return 1; 
    }

    [HttpGet]
    public ActionResult<IEnumerable<Medication>> GetAll()
    {
        var userId = GetCurrentUserId();
        // Показываем только препараты текущего пользователя
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