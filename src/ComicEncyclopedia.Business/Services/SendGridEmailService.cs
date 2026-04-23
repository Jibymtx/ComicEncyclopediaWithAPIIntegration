using ComicEncyclopedia.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ComicEncyclopedia.Business.Services
{
    public class SendGridEmailService : ISendGridEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string? _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public string ProviderName => "SendGrid";

        public SendGridEmailService(HttpClient httpClient, ILogger<SendGridEmailService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@comicencyclopedia.local";
            _fromName = configuration["SendGrid:FromName"] ?? "Comic Encyclopedia";
        }

        // send email
        public async Task<EmailResult> SendAsync(EmailRequest request)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("SendGrid API key is not configured. Returning simulated success.");
                return new EmailResult
                {
                    Success = true,
                    Provider = ProviderName,
                    StatusCode = 202,
                    Message = "Simulated send (SendGrid API key not configured)."
                };
            }

            try
            {
                var payload = new
                {
                    personalizations = new[]
                    {
                        new
                        {
                            to = new[] { new { email = request.ToEmail, name = request.ToName } },
                            subject = request.Subject
                        }
                    },
                    from = new { email = _fromEmail, name = _fromName },
                    content = new[]
                    {
                        new { type = "text/plain", value = string.IsNullOrEmpty(request.BodyText) ? StripHtml(request.BodyHtml) : request.BodyText },
                        new { type = "text/html", value = string.IsNullOrEmpty(request.BodyHtml) ? request.BodyText : request.BodyHtml }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                return new EmailResult
                {
                    Success = response.IsSuccessStatusCode,
                    Provider = ProviderName,
                    StatusCode = (int)response.StatusCode,
                    Message = response.IsSuccessStatusCode ? "Email sent via SendGrid" : "SendGrid request failed",
                    ErrorDetails = response.IsSuccessStatusCode ? null : body
                };
            }
            catch (Exception ex)
            {
                // return error
                _logger.LogError(ex, "Error sending email via SendGrid to {Email}", request.ToEmail);
                return new EmailResult
                {
                    Success = false,
                    Provider = ProviderName,
                    StatusCode = 0,
                    Message = "Exception sending email via SendGrid",
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
