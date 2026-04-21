using BachatGat.Core.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BachatGat.Infrastructure.Services;

public class FirebaseTokenValidator : IFirebaseTokenValidator
{
    private readonly ILogger<FirebaseTokenValidator> _logger;

    public FirebaseTokenValidator(IConfiguration config, ILogger<FirebaseTokenValidator> logger)
    {
        _logger = logger;
        if (FirebaseApp.DefaultInstance == null)
        {
            var serviceAccountPath = config["Firebase:ServiceAccountPath"];
            AppOptions options;
            if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
            {
                options = new AppOptions { Credential = GoogleCredential.FromFile(serviceAccountPath) };
            }
            else
            {
                // Falls back to GOOGLE_APPLICATION_CREDENTIALS env var or GCP default credentials
                options = new AppOptions { Credential = GoogleCredential.GetApplicationDefault() };
            }
            FirebaseApp.Create(options);
        }
    }

    public async Task<FirebaseTokenInfo?> ValidateAsync(string idToken)
    {
        try
        {
            var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            var claims = decoded.Claims;

            string? phoneNumber = claims.TryGetValue("phone_number", out var p) ? p?.ToString() : null;
            string? email = claims.TryGetValue("email", out var e) ? e?.ToString() : null;
            string? name = claims.TryGetValue("name", out var n) ? n?.ToString() : null;
            string signInProvider = "unknown";

            if (claims.TryGetValue("firebase", out var fb) && fb is System.Text.Json.JsonElement fbEl)
            {
                if (fbEl.TryGetProperty("sign_in_provider", out var providerEl))
                    signInProvider = providerEl.GetString() ?? "unknown";
            }

            return new FirebaseTokenInfo(decoded.Uid, phoneNumber, email, name, signInProvider);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Firebase token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
