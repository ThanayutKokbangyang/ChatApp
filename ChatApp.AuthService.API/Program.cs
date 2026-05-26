using ChatApp.Auth.Service.API.Endpoints;
using ChatApp.AuthService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// =====================================================================
// Auth Service entry point (.NET 9 Minimal Hosting)
// =====================================================================
var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// DESIGN PATTERN: Builder Pattern (DI configuration)
// ---------------------------------------------------------------------
// ใช้ extension method AddAuthInfrastructure() จัดการ DI ทั้งหมดในจุดเดียว
// แทนที่จะ register ทีละบรรทัด ทำให้ Program.cs สั้นและอ่านง่าย
// =====================================================================
builder.Services.AddAuthInfrastructure();

// =====================================================================
// CORS — อนุญาตให้ Blazor WASM frontend เรียก API ผ่าน Nginx ได้
// =====================================================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost", "https://localhost:443")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =====================================================================
// JWT Bearer Authentication
// ---------------------------------------------------------------------
// validate JWT ทุก request ที่ติด [Authorize] หรือ .RequireAuthorization()
// ใช้ secret key เดียวกันทั้ง AuthService และ ChatService
// =====================================================================
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            // ClockSkew = TimeSpan.Zero หมายถึงไม่ tolerate clock difference
            // ของจริงถ้ามีหลาย server อาจตั้งไว้ 30 วินาที
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =====================================================================
// DESIGN PATTERN: Chain of Responsibility (Middleware Pipeline)
// ---------------------------------------------------------------------
// แต่ละ middleware ใน ASP.NET Core เป็น handler ใน chain
// แต่ละตัวตัดสินใจได้ว่าจะ:
//   - ทำงานก่อน (เช่น authentication)
//   - ส่งต่อให้ตัวถัดไป (await _next(context))
//   - ทำงานหลัง (เช่น logging)
//   - short-circuit (เช่น return 401 ทันที)
//
// ลำดับสำคัญมาก! UseAuthentication ต้องอยู่ก่อน UseAuthorization
// =====================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();   // ตรวจ token → set ClaimsPrincipal
app.UseAuthorization();    // ตรวจ [Authorize] / RequireAuthorization

app.MapAuthEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
