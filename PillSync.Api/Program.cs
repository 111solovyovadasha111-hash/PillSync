using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PillSync.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрируем контроллеры (для будущих наших контроллеров)
builder.Services.AddControllers();

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        // Настройки паролей (чтобы было не слишком сложно для тестов)
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<IdentityDbContext<AppUser>>();

// ВРЕМЕННЫЙ DbContext для Identity (потом Амалия заменит его на свой)
builder.Services.AddDbContext<IdentityDbContext<AppUser>>(options =>
    options.UseInMemoryDatabase("DevDb"));

builder.Services.AddEndpointsApiExplorer();

// 2. Swagger
builder.Services.AddSwaggerGen(options =>
{
    // 1. Описываем схему безопасности (Bearer токен)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставь сюда свой JWT токен (без слова Bearer)."
    });

    // 2. Применяем эту схему ко всем методам (чтобы появились замочки)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// 3. Health-checks (проверка, что сервер жив)
builder.Services.AddHealthChecks();

// 4. JWT (пока с dev-ключом из конфига)
var jwtKey = builder.Configuration["Jwt:Key"] 
             ?? "DEV_ONLY_SECRET_KEY_CHANGE_ME_1234567890_ABCDEF";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PillSync",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PillSync",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// 5. CORS (чтобы Blazor-клиент Светы мог достучаться до API)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// --- Конвейер middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();