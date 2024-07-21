using Abp.Domain.Repositories;
using AccommodationSearchSystem.Authorization.Users;
using AccommodationSearchSystem.Email.Dto;
using AccommodationSearchSystem.EntityFrameworkCore;
using AccommodationSearchSystem.Helpers;
using AccommodationSearchSystem.Services;
using AccommodationSearchSystem.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static IdentityServer4.Models.IdentityResources;

namespace AccommodationSearchSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : AccommodationSearchSystemControllerBase
    {
        private readonly AccommodationSearchSystemDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRepository<User, long> _repositoryUser;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(
            AccommodationSearchSystemDbContext context,
            IConfiguration configuration,
            IRepository<User, long> repositoryUser,
            IEmailService emailService,
            IPasswordHasher<User> passwordHasher
        )
        {
            _context = context;
            _configuration = configuration;
            _repositoryUser = repositoryUser;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("send-reset-email/{email}")]
        public async Task<IActionResult> SendResetEmail(string email)
        {
            var user = await _repositoryUser.FirstOrDefaultAsync(e => e.EmailAddress == email);
            // lấy user , check email có trong db không
            if (user == null)
            {
                //return NotFound(new
                //{
                //    StatusCode = 404,
                //    Message = "Email doesn't exist"
                //});
                throw new Exception("Email không tồn tại!");
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var emailToken = Convert.ToBase64String(tokenBytes);
            // tạo emailToken

            user.PasswordResetCode = emailToken;
            // lưu mã token vào password mới 
            // nên tạo thêm cái ResetPasswordExpriry = Datime.Now.AddMinutes(15);

            var from = _configuration["EmailSettings:From"];
            var emailModel = new EmailModel(email, "Reset Password", EmailBody.EmailStringBody(email, emailToken));
            _emailService.SendEmail(emailModel);
            //_context.Entry(user).State = EntityState.Modified;
            //await _context.SaveChangesAsync();
            await _repositoryUser.UpdateAsync(user);
            await CurrentUnitOfWork.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Email sent!"
            });
        }

        [HttpPost("reset-passwordToken")]
        public async Task<IActionResult> ResetPasswordToken(ResetPasswordTokenDto resetPasswordDto)
        {
            var user = await _repositoryUser.FirstOrDefaultAsync(e => e.EmailAddress == resetPasswordDto.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Email doesn't exist"
                });
            }

            var tokenCode = user.PasswordResetCode;
            // gọi lại mã thống báo bằng PasswordResetCode
            if (tokenCode != resetPasswordDto.EmailToken)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Reset Token"
                });
            }

            user.Password = _passwordHasher.HashPassword(user, resetPasswordDto.NewPassword);
            //user.PasswordResetCode = null; // Clear the reset code after successful reset


           // _context.Entry(user).State = EntityState.Modified;
            await CurrentUnitOfWork.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Password reset successful!"
            });
        }

        [HttpPost("send-contact-email")]
        public async Task<IActionResult> SendContactEmail([FromBody] ContactFormModel contactForm)
        {
            if (contactForm == null || !ModelState.IsValid)
            {
                return BadRequest(new { StatusCode = 400, Message = "Invalid form data" });
            }

            var emailModel = new EmailModel(
                _configuration["EmailSettings:From"],
                contactForm.Subject,
                $"Name: {contactForm.Name}<br>Email: {contactForm.Email}<br>Message: {contactForm.Message}"
            );
             _emailService.SendEmail(emailModel);
            return Ok(new { StatusCode = 200, Message = "Email sent!" });
        }
    }
}
