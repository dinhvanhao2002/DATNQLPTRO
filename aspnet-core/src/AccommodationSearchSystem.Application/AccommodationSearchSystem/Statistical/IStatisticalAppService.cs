using Abp.Application.Services;
using AccommodationSearchSystem.AccommodationSearchSystem.Statistical.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.Statistical
{
    public interface IStatisticalAppService : IApplicationService
    {
        Task<DashBoardTotalUser> GetTotalUser();
        Task<DashBoardTotalPost> GetTotalPost();
        Task<DashBoardTotalPost> GetTotalPostLike();
        Task<DashBoardTotalBooking> GetTotalBooking();
        Task<List<PostCategoryDto>> GetPostCountByCategory();
        Task<List<PostCountByMonthDto>> GetPostCountByMonth();
        Task<List<ScheduleCountByMonthDto>> GetScheduleCountByMonth();
        Task<UserPostCountDto> GetUserWithMostPosts();
        Task<UserScheduleCountDto> GetUserWithMostSchedules();

        Task<byte[]> GetBpByDateForReport([FromBody]ReportInput input);


    }
}
