using Abp.Application.Services;
using AccommodationSearchSystem.Users.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.Users
{
    public interface IProfileAppService : IApplicationService
    {
        Task UpdateProfilePicture(UpdateProfilePictureInput input);
    }
}
