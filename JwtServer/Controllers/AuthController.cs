using DAL.Repositories.IRepositories;
using JwtServer.Dto;
using JwtServer.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Models;
using Models.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public class AuthController : ControllerBase
    {
        private readonly IUserAccountRepository _userRepo;
        private readonly IUserProfileRepository _profileRepo;
        private readonly IEmailService _emailService;

        public AuthController(IUserAccountRepository userRepo, IUserProfileRepository profileRepo, IEmailService emailService)
        {
            _userRepo = userRepo;
            _profileRepo = profileRepo;
            _emailService = emailService;
        }


        /// <summary>
        /// Authenticating user.
        /// </summary>
        /// <param name="userDetails">Must contain user name and password.</param>
        /// <returns>Http Ok if success, http bad request if fail.</returns>
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] LoginDetails loginDetails)
        {
            try
            {
                var userWithTokenExpirationDate = _userRepo.AuthenticateUser(loginDetails);

                if (userWithTokenExpirationDate == null) return BadRequest(new { message = "Invalid user details" });

                return Ok(userWithTokenExpirationDate);
            }
            catch(Exception e)
            {
                return BadRequest(new { message = "Invalid user details" });
            }

        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginDetails loginDetails)
        {
            if (string.IsNullOrEmpty(loginDetails.Email)
                || !loginDetails.Email.Contains('@')
                || !loginDetails.Email.Contains('.')
                || string.IsNullOrEmpty(loginDetails.Password))
                return BadRequest(new { message = "Wrong credentials" });

            UserAccount accountToCreate = new UserAccount()
            {
                Email = loginDetails.Email,
                Password = loginDetails.Password
            };

            var newAccount = _userRepo.RegisterUser(accountToCreate);

            if (newAccount == null || newAccount.Email.Length <= 0) 
                return BadRequest(new { message = "Coudn't create account" });

            UserProfile newProfile = new UserProfile()
            {
                UserAccount = newAccount,
                UserAccountId = newAccount.Id,
                HashTags = new List<HashTag>()
            };

            if(!_profileRepo.CreateProfile(newProfile) || !_userRepo.AddProfileToAccount(newProfile, newAccount.Id)) 
                return BadRequest(new { message = "Coudn't create profile" });

            string emailBody = $"<h1>Welcome to Hobbist !</h1> <p>To confirm your account please click the following link {Statics.ActiveAccountLink}?email={newAccount.Email}&activationId={newAccount.ActivationId}</p>";
            _emailService.SendEmail(newAccount.Email, "Aktywacja konta.", emailBody);

            return Ok();
        }

        [HttpPost("ActivateAccountConfirm")]
        public IActionResult ActivateAccountConfirm(string userEmail, Guid activationId)
        {
            var userAcc = _userRepo.GetUserByEmail(userEmail);

            if (userAcc == null || userAcc.ActivationId != activationId) return BadRequest();

            userAcc.isBlocked = false;
            userAcc.ActivationId = Guid.Empty;

            return _userRepo.Save() ? Ok("ok") : BadRequest("fail");
        }

        [HttpPost("RecoverPassword")]
        public IActionResult RecoverPassword(string userEmail)
        {
            var userAcc = _userRepo.GetUserByEmail(userEmail);

            if (userAcc == null) return BadRequest();

            userAcc.ActivationId = Guid.NewGuid();

            if (!_userRepo.Save()) return BadRequest();

            string emailBody = $"<h1>Hello Hobbist !</h1> <p>To recover your password please click the following link {Statics.RecoverPasswordEndpoint}?email={userAcc.Email}&activationId={userAcc.ActivationId}</p>";
            _emailService.SendEmail(userAcc.Email, "Odzyskiwanie hasłą", emailBody);

            return Ok();
        }

        [HttpPost("RecoverPasswordConfirm")]
        public IActionResult RecoverPasswordConfirm([FromBody] RecoverPasswordConfirmationDto request)
        {
            if (request.Password != request.ConfirmPassword) return BadRequest();

            var userAcc = _userRepo.GetUserByEmail(request.UserEmail);

            if (userAcc == null || userAcc.ActivationId != request.ActivationId) return BadRequest();

            userAcc.ActivationId = Guid.Empty;
            userAcc.Password = request.Password;

            return _userRepo.Save() ? Ok() : BadRequest();
        }


        [Authorize]
        [HttpPost("ChangeUserEmail")]
        public IActionResult ChangeUserEmail([FromBody] LoginDetailsAndNewEmail loginDetailsAndUsername)
        {
            var userWithTokenExpirationDate = _userRepo.AuthenticateUser(loginDetailsAndUsername.LoginDetails);
            if (userWithTokenExpirationDate == null || string.IsNullOrEmpty(loginDetailsAndUsername.NewEmail)) return BadRequest(new { message = "Invalid parameters" });

            return _userRepo.ChangeEmail(loginDetailsAndUsername.LoginDetails, loginDetailsAndUsername.NewEmail) ? Ok() : BadRequest();
        }
    }
}
