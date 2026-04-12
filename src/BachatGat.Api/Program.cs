using System.Security.Cryptography.X509Certificates;
using System.Text;
using BachatGat.Application;
using BachatGat.Application.Exceptions;
using BachatGat.Infrastructure;
using BachatGat.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Bootstrap logger captures any startup errors before full config loads
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BachatGat API");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog — full config read from appsettings
    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Production HTTPS certificate — loaded from config / env vars.
    // Set ONE of these pairs before deploying:
    //   PEM :  Kestrel__Certificate__Path=/etc/ssl/cert.pem  +  Kestrel__Certificate__KeyPath=/etc/ssl/key.pem
    //   PFX :  Kestrel__Certificate__Path=/etc/ssl/cert.pfx  +  Kestrel__Certificate__Password=<secret>
    if (!builder.Environment.IsDevelopment())
    {
        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            var cfg = builder.Configuration;
            var certPath = cfg["Kestrel:Certificate:Path"];
            var keyPath  = cfg["Kestrel:Certificate:KeyPath"];
            var certPass = cfg["Kestrel:Certificate:Password"];

            if (string.IsNullOrWhiteSpace(certPath))
                throw new InvalidOperationException(
                    "Kestrel:Certificate:Path is not set. " +
                    "Provide a PEM cert+key or a PFX file via environment variables.");

            kestrel.ConfigureHttpsDefaults(https =>
            {
                https.ServerCertificate = string.IsNullOrWhiteSpace(keyPath)
                    ? new X509Certificate2(certPath, certPass)           // PFX
                    : X509Certificate2.CreateFromPemFile(certPath, keyPath); // PEM
            });
        });
    }

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

    // CORS — origins come from config so dev and prod can each declare their own
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:4200"];

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
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

    if (!app.Environment.IsDevelopment())
    {
        // HSTS tells browsers to always use HTTPS for this origin (max-age 1 year)
        app.UseHsts();
        // Redirect any stray HTTP requests to HTTPS
        app.UseHttpsRedirection();
    }
    // Dev: skip redirect — Angular calls http://localhost:5002 and redirecting
    // strips the Authorization header, causing 401s.

    // Structured HTTP request/response logging (replaces default ASP.NET access log)
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "BachatGat API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
