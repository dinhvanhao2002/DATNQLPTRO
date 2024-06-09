using Abp.Domain.Repositories;
using AccommodationSearchSystem.Authorization.Users;
using AccommodationSearchSystem.Email.Dto;
using AccommodationSearchSystem.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IdentityServer4.Models.IdentityResources;

namespace AccommodationSearchSystem.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<User, long> _repositoryUser;

        public EmailService(
            IConfiguration configuration,
            IRepository<User, long> repositoryUser
        )
        {
            _configuration = configuration;
            _repositoryUser = repositoryUser;
        }
        public void SendEmail(EmailModel emailModel)
        {
            var emailMessage = new MimeMessage();
            var from = _configuration["EmailSettings:From"];
            emailMessage.From.Add(new MailboxAddress("RENT24H", from));
            emailMessage.To.Add(new MailboxAddress(emailModel.To, emailModel.To));
            emailMessage.Subject = emailModel.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailModel.Content
            };
            using( var client = new SmtpClient())
            {
                //try
                //{
                //    client.Connect(_configuration["EmailSettings:SmtpServer"], 587, true);
                //    client.Authenticate(_configuration["EmailSettings:From"], _configuration["EmailSettings:Password"]);
                //    client.Send(emailMessage);

                //}catch(Exception ex)
                //{
                //    throw (ex);
                //}
                //finally
                //{
                //    client.Disconnect(true);
                //    client.Dispose();
                //}
                try
                {
                    var smtpServer = _configuration["EmailSettings:SmtpService"];
                    var port = int.Parse(_configuration["EmailSettings:Port"]);
                    var username = _configuration["EmailSettings:Username"];
                    var password = _configuration["EmailSettings:Password"];

                    client.Connect(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(username, password);
                    client.Send(emailMessage);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }

        }
    }
    
}
