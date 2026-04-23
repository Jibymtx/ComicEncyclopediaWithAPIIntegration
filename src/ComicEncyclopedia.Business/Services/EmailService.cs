using ComicEncyclopedia.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ComicEncyclopedia.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly ISendGridEmailService _sendGrid;
        private readonly IMailjetEmailService _mailjet;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            ISendGridEmailService sendGrid,
            IMailjetEmailService mailjet,
            ILogger<EmailService> logger)
        {
            _sendGrid = sendGrid;
            _mailjet = mailjet;
            _logger = logger;
        }

        // send email
        public Task<EmailResult> SendAsync(EmailRequest request, EmailProvider provider)
        {
            return provider switch
            {
                EmailProvider.SendGrid => SendWithSendGridAsync(request),
                EmailProvider.Mailjet => SendWithMailjetAsync(request),
                _ => SendWithSendGridAsync(request)
            };
        }

        public Task<EmailResult> SendWithSendGridAsync(EmailRequest request)
        {
            _logger.LogInformation("Dispatching email to {Email} via SendGrid", request.ToEmail);
            return _sendGrid.SendAsync(request);
        }

        public Task<EmailResult> SendWithMailjetAsync(EmailRequest request)
        {
            _logger.LogInformation("Dispatching email to {Email} via Mailjet", request.ToEmail);
            return _mailjet.SendAsync(request);
        }


        public Task<EmailResult> SendSaveNotificationAsync(string toEmail, string toName, string comicTitle, EmailProvider provider)
        {
            var request = new EmailRequest
            {
                ToEmail = toEmail,
                ToName = toName,
                Subject = $"Comic saved to your list: {comicTitle}",
                BodyHtml = $"<p>Hi {System.Net.WebUtility.HtmlEncode(toName)},</p>" +
                           $"<p>You just added <strong>{System.Net.WebUtility.HtmlEncode(comicTitle)}</strong> to your saved comics list on Comic Encyclopedia.</p>" +
                           "<p>Happy reading!</p>",
                BodyText = $"Hi {toName},\n\nYou just added {comicTitle} to your saved comics list on Comic Encyclopedia.\n\nHappy reading!"
            };

            return SendAsync(request, provider);
        }
    }
}
