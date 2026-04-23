using ComicEncyclopedia.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComicEncyclopedia.Web.Controllers.Api
{
    [ApiController]
    [Route("api/email")]
    public class EmailApiController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailApiController> _logger;

        public EmailApiController(IEmailService emailService, ILogger<EmailApiController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public class SendEmailRequest
        {
            public string ToEmail { get; set; } = string.Empty;
            public string ToName { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string BodyHtml { get; set; } = string.Empty;
            public string BodyText { get; set; } = string.Empty;
            public string Provider { get; set; } = "SendGrid";
            public string? ComicTitle { get; set; }
        }

        // send email
        [HttpPost("send")]
        public async Task<ActionResult> Send([FromBody] SendEmailRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return BadRequest(new { error = "toEmail is required" });
            }

            if (!Enum.TryParse<EmailProvider>(request.Provider, true, out var provider))
            {
                return BadRequest(new { error = $"Unknown provider '{request.Provider}'. Use 'SendGrid' or 'Mailjet'." });
            }

            EmailResult result;

            if (!string.IsNullOrWhiteSpace(request.ComicTitle) &&
                string.IsNullOrWhiteSpace(request.Subject) &&
                string.IsNullOrWhiteSpace(request.BodyHtml) &&
                string.IsNullOrWhiteSpace(request.BodyText))
            {
                result = await _emailService.SendSaveNotificationAsync(
                    request.ToEmail,
                    string.IsNullOrWhiteSpace(request.ToName) ? request.ToEmail : request.ToName,
                    request.ComicTitle!,
                    provider);
            }
            else
            {
                var emailRequest = new EmailRequest
                {
                    ToEmail = request.ToEmail,
                    ToName = string.IsNullOrWhiteSpace(request.ToName) ? request.ToEmail : request.ToName,
                    Subject = string.IsNullOrWhiteSpace(request.Subject) ? "Notification from Comic Encyclopedia" : request.Subject,
                    BodyHtml = request.BodyHtml,
                    BodyText = request.BodyText
                };
                result = await _emailService.SendAsync(emailRequest, provider);
            }

            if (!result.Success)
            {
                // return error
                _logger.LogWarning("Email send failed via {Provider}: {Details}", result.Provider, result.ErrorDetails);
                return StatusCode(result.StatusCode == 0 ? 500 : result.StatusCode, result);
            }

            return Ok(result);
        }


        [HttpPost("send/sendgrid")]
        public async Task<ActionResult> SendViaSendGrid([FromBody] SendEmailRequest request)
        {
            request.Provider = nameof(EmailProvider.SendGrid);
            return await Send(request);
        }

        [HttpPost("send/mailjet")]
        public async Task<ActionResult> SendViaMailjet([FromBody] SendEmailRequest request)
        {
            request.Provider = nameof(EmailProvider.Mailjet);
            return await Send(request);
        }
    }
}
