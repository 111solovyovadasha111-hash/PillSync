using Microsoft.AspNetCore.Mvc;

namespace PillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    [HttpPost("interaction-check")]
    public IActionResult CheckInteractions([FromBody] object request)
    {
        // Это заглушка. Даша позже заменит её на реальный вызов YandexGPT.
        return Ok(new 
        { 
            Status = "Mock (Заглушка)", 
            Message = "Проверка совместимости находится в разработке. Скоро здесь будет ответ от ИИ.",
            Disclaimer = "Информация носит справочный характер и не является медицинской рекомендацией. Проконсультируйтесь с врачом."
        });
    }
}