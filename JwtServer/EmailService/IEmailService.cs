using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtServer.EmailService
{
    public interface IEmailService
    {
        void SendEmail(string userEmail, string subject, string emailHtmlContent);
    }
}
