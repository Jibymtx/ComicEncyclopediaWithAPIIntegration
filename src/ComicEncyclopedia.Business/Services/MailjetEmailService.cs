using ComicEncyclopedia.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ComicEncyclopedia.Business.Services
{
    public class MailjetEmailService : IMailjetEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MailjetEmailService> _logger;
        private readonly string? _apiKey;
        private readonly string? _apiSecret;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public string ProviderName => "Mailjet";

        public MailjetEmailService(HttpClient httpClient, ILogger<MailjetEmailService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Mailjet:ApiKey"];
            _apiSecret = configuration["Mailjet:ApiSecret"];
            _fromEmail = configuration["Mailjet:FromEmail"] ?? "noreply@comicencyclopedia.local";
            _fromName = configuration["Mailjet:FromName"] ?? "Comic Encyclopedia";
        }


        // send email
        public async Task<EmailResult> SendAsync(EmailRequest request)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret))
            {
                _logger.LogWarning("Mailjet API credentials are not configured. Returning simulated success.");
                return new EmailResult
                {
                    Success = true,
                    Provider = ProviderName,
                    StatusCode = 200,
                    Message = "Simulated send (Mailjet credentials not configured)."
                };
            }

            try
            {
                var payload = new
                {
                    Messages = new[]
                    {
                        new
                        {
                            From = new { Email = _fromEmail, Name = _fromName },
                            To = new[] { new { Email = request.ToEmail, Name = request.ToName } },
                            Subject = request.Subject,
                            TextPart = string.IsNullOrEmpty(request.BodyText) ? StripHtml(request.BodyHtml) : request.BodyText,
                            HTMLPart = string.IsNullOrEmpty(request.BodyHtml) ? request.BodyText : request.BodyHtml
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.mailjet.com/v3.1/send");
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:{_apiSecret}"));
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                return new EmailResult
                {
                    Success = response.IsSuccessStatusCode,
                    Provider = ProviderName,
                    StatusCode = (int)response.StatusCode,
                    Message = response.IsSuccessStatusCode ? "Email sent via Mailjet" : "Mailjet request failed",
                    ErrorDetails = response.IsSuccessStatusCode ? null : body
                };
            }
            catch (Exception ex)
            {
                // return error
                _logger.LogError(ex, "Error sending email via Mailjet to {Email}", request.ToEmail);
                return new EmailResult
                {
                    Success = false,
                    Provider = ProviderName,
                    StatusCode = 0,
                    Message = "Exception sending email via Mailjet",
                    ErrorDetails = ex.Message
                };
            }
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}
