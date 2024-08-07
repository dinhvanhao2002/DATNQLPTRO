﻿using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.ManageAppointmentSchedules.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts
{
    public interface IManagePostsAppService : IApplicationService
    {
        Task<long> CreateOrEdit(CreateOrEditIPostDto input);
        Task ConfirmPostAD(ConfirmPostByAdminDto input);
        Task<PagedResultDto<GetPostForViewDto>> GetAll(GetPostInputDto input);
        Task<PagedResultDto<GetPostForViewDto>> GetAllForHost(GetPostInputDto input);
        Task DeletePost(EntityDto<long> input);
        Task<GetPostForEditOutput> GetLoyaltyGiftItemForEdit(EntityDto<long> input);
        Task<GetPostForViewDto> GetForEdit(EntityDto<long> input);
        Task<PagedResultDto<GetPostForViewDto>> GetAllForAdmin(GetPostInputDto input);
        Task<PagedResultDto<GetPostForViewDto>> GetAllForHostVIP(GetPostInputDto input);

        // Yêu thích bài đăng
        Task<PostLikeDto> LikePosts(EntityDto<long> input);
    }
}