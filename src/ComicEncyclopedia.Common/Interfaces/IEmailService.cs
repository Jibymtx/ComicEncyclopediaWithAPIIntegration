namespace ComicEncyclopedia.Common.Interfaces
{
    public enum EmailProvider
    {
        SendGrid,
        Mailjet
    }

    public class EmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public string BodyText { get; set; } = string.Empty;
    }

    public class EmailResult
    {
        public bool Success { get; set; }
        public string Provider { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? ErrorDetails { get; set; }
    }

    public interface IEmailProviderService
    {
        string ProviderName { get; }
        Task<EmailResult> SendAsync(EmailRequest request);
    }

    public interface ISendGridEmailService : IEmailProviderService { }

    public interface IMailjetEmailService : IEmailProviderService { }


    public interface IEmailService
    {
        // send email
        Task<EmailResult> SendAsync(EmailRequest request, EmailProvider provider);
        Task<EmailResult> SendWithSendGridAsync(EmailRequest request);
        Task<EmailResult> SendWithMailjetAsync(EmailRequest request);
        Task<EmailResult> SendSaveNotificationAsync(string toEmail, string toName, string comicTitle, EmailProvider provider);
    }
}
