using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JwtServer.EmailService
{
    public class EmailService : IEmailService
    {
        private SmtpClient _smtpClient { get; set; }
        public EmailService()
        {
            _smtpClient = new SmtpClient(Statics.SmtpHost)
            {
                Port = Statics.SmtpPort,
                Credentials = new NetworkCredential(Statics.SmtpLogin, Statics.SmtpPassword),
                EnableSsl = true,
            };
        }

        public void SendEmail(string userEmail, string subject, string emailHtmlContent)
        {
            var mailMessage = GetMailMessageWithBodyAndSubject(subject, emailHtmlContent);

            mailMessage.To.Add(userEmail);

            _smtpClient.Send(mailMessage);
        }

        private MailMessage GetMailMessageWithBodyAndSubject(string subject, string body)
            => new MailMessage
            {
                From = new MailAddress(Statics.SmtpLogin),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
    }
}
