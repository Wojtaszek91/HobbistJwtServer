using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtServer.Dto
{
    public class RecoverPasswordConfirmationDto
    {
        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public Guid ActivationId { get; set; }

    }
}
