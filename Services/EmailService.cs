using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace PeerMarking.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var from = _configuration["EmailSettings:From"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            using (var client = new System.Net.Mail.SmtpClient(smtpServer, port))
            {
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage(from, toEmail, subject, body);
                await client.SendMailAsync(mailMessage);
            }
        }
    }

}
