using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PillSync.Api.Ai;
using PillSync.Api.Data;
using PillSync.Api.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Контроллеры
builder.Services.AddControllers();

// 2. Identity
builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставь сюда свой JWT токен (без слова Bearer)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });
});

// 4. Health
builder.Services.AddHealthChecks();

// 5. JWT
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

// 6. CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// 7. Дашин код (AI)
builder.Services.Configure<YandexAiOptions>(
    builder.Configuration.GetSection(YandexAiOptions.SectionName));

var ai = builder.Configuration.GetSection(YandexAiOptions.SectionName).Get<YandexAiOptions>() ?? new();

if (ai.UseMock)
{
    builder.Services.AddScoped<IYandexAiClient, MockYandexAiClient>();
}
else
{
    builder.Services.AddHttpClient<IYandexAiClient, YandexFoundationModelsClient>((sp, http) =>
    {
        var o = sp.GetRequiredService<IOptions<YandexAiOptions>>().Value;
        http.BaseAddress = new Uri(o.BaseUrl);
        http.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Api-Key", o.ApiKey);
        http.DefaultRequestHeaders.Add("x-folder-id", o.FolderId);
    });
}

var app = builder.Build();

// Конвейер
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