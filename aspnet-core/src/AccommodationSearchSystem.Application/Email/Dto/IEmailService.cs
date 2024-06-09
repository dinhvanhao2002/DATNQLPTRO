using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.Email.Dto
{
    public interface IEmailService : IApplicationService
    {
        void SendEmail(EmailModel emailModel);
    }
}
