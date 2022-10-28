using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtServer
{
    public static class Statics
    {
        public static string ActiveAccountLink { get; set; }
        public static string RecoverPasswordEndpoint { get; set; }

        public static string SmtpHost { get; set; }
        public static string SmtpLogin { get; set; }
        public static string SmtpPassword { get; set; }
        public static int SmtpPort { get; set; }
    }

}
