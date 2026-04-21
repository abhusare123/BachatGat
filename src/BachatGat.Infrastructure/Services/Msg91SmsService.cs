using System.Net.Http.Json;
using BachatGat.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BachatGat.Infrastructure.Services;

public class Msg91SmsService(HttpClient http, IConfiguration config, ILogger<Msg91SmsService> logger) : ISmsService
{
    public async Task SendOtpAsync(string phoneNumber, string otp)
    {
        var authKey    = config["Msg91:AuthKey"]    ?? throw new InvalidOperationException("Msg91:AuthKey is not configured");
        var templateId = config["Msg91:TemplateId"] ?? throw new InvalidOperationException("Msg91:TemplateId is not configured");
        var senderId   = config["Msg91:SenderId"] ?? "BCHATG";

        // Ensure 10-digit Indian numbers are prefixed with country code
        var mobile = phoneNumber.Length == 10 ? "91" + phoneNumber : phoneNumber;

        var payload = new
        {
            template_id = templateId,
            sender      = senderId,
            short_url   = "0",
            mobiles     = mobile,
            VAR1        = otp
        };

        http.DefaultRequestHeaders.Remove("authkey");
        http.DefaultRequestHeaders.Add("authkey", authKey);

        var response = await http.PostAsJsonAsync("https://control.msg91.com/api/v5/flow/", payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            logger.LogError("MSG91 OTP send failed for {Phone}: {Status} — {Body}", phoneNumber, response.StatusCode, body);
        }
        else
        {
            logger.LogInformation("OTP sent via MSG91 to {Phone}", phoneNumber);
        }
    }
}
