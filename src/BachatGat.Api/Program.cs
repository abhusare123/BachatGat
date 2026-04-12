using System.Text;
using BachatGat.Application;
using BachatGat.Application.Exceptions;
using BachatGat.Infrastructure;
using BachatGat.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (EF Core, SMS, JWT, LoanCalculator)
builder.Services.AddInfrastructure(builder.Configuration);

// Application services
builder.Services.AddApplication();

// Controllers
builder.Services.AddControllers();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or too short (must be ≥ 32 characters). " +
        "Dev: run  dotnet user-secrets set \"Jwt:Key\" \"<64-char-random>\"  in src/BachatGat.Api. " +
        "Prod: set environment variable  Jwt__Key=<value>.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Auto-migrate database on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Global exception handler — maps application exceptions to HTTP status codes
app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    (context.Response.StatusCode, string? message) = ex switch
    {
        NotFoundException     e => (404, e.Message),
        ForbiddenException    e => (403, e.Message),
        ConflictException     e => (409, e.Message),
        BadRequestException   e => (400, e.Message),
        _                       => (500, "An unexpected error occurred")
    };
    context.Response.ContentType = "application/json";
    if (!string.IsNullOrEmpty(message))
        await context.Response.WriteAsJsonAsync(new { Message = message });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bachat Gat API v1"));
}

app.UseCors();

// Skip HTTPS redirect in dev — Angular calls http://localhost:5002
// and redirecting strips the Authorization header, causing 401s.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
