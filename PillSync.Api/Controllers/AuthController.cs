using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PillSync.Api.Models;
using PillSync.Api.Models.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    // Внедряем UserManager (для работы с пользователями) и конфигурацию (для ключа JWT)
    public AuthController(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email и пароль обязательны.");

        var user = new AppUser { UserName = dto.Email, Email = dto.Email };
        
        // Identity сам хеширует пароль!
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            // Если пароль слишком короткий или почта занята, вернем ошибки
            return BadRequest(result.Errors);
        }

        return Ok(new { Message = "Пользователь успешно зарегистрирован!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Ищем пользователя по email
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Неверный email или пароль.");

        // Проверяем пароль (Identity сравнит введенный пароль с хешем в базе)
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return Unauthorized("Неверный email или пароль.");

        // Генерируем JWT-токен
        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(AppUser user)
    {
        // Берем настройки из appsettings.json (те самые, что мы добавляли вчера)
        var jwtKey = _configuration["Jwt:Key"] ?? throw new Exception("JWT Key не найден");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "PillSync";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "PillSync";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // В токен кладем ID пользователя (ClaimTypes.NameIdentifier)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), // Вот тут ID, который мы будем брать из JWT!
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Токен живет 7 дней
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}