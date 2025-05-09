using System.Net;
using System.Net.Mail;

namespace ArelScoreWebUI.Middleware
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        bool IsValidEmail(string email);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HashSet<string> _allowedDomains;

        public EmailService(IConfiguration configuration)
        {
            var domains = configuration.GetSection("AllowedEmailDomains").Get<List<string>>();
            _allowedDomains = new HashSet<string>(domains, StringComparer.OrdinalIgnoreCase);
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient
            {
                Host = _configuration["Smtp:Host"],
                Port = int.Parse(_configuration["Smtp:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _configuration["Smtp:Username"],
                    _configuration["Smtp:Password"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:Username"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return false;

            var localPart = parts[0];
            var domainPart = parts[1];

            // Sadece listedeki domainlerde '+' işareti kontrol edilir
            if (_allowedDomains.Contains(domainPart) && localPart.Contains('+') || localPart.Contains('*') || localPart.Contains('.'))
            {
                return false; // + işareti içeriyorsa reddet
            }

            return true;
        }

    }
}
