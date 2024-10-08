﻿using Abp.Authorization.Users;
using AccommodationSearchSystem.AccommodationSearchSystem.ManageAppointmentSchedules.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.PackagePosts.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.UserComment.Dto;
using AccommodationSearchSystem.Entity;
using AccommodationSearchSystem.Sessions.Dto;
using AccommodationSearchSystem.VnPayment.Dto;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.Helpers
{
    public class CustomDtoMapper : Profile
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<Post, CreateOrEditIPostDto>().ReverseMap();
            configuration.CreateMap<Post, GetPostForViewDto>().ReverseMap();
            configuration.CreateMap<Post, ConfirmPostByAdminDto>().ReverseMap();
            configuration.CreateMap<Post, PhotoDto>().ReverseMap();

            configuration.CreateMap<Accommodate, CreateOrEditIPostDto>().ReverseMap();

            configuration.CreateMap<AppointmentSchedule, CreateOrEditSchedulesDto>().ReverseMap();
            configuration.CreateMap<AppointmentSchedule, ConfirmSchedulesDto>().ReverseMap();
            configuration.CreateMap<AppointmentSchedule, GetAllSchedulesDto>().ReverseMap();
            configuration.CreateMap<AppointmentSchedule, CancelSchedulesDto>().ReverseMap();

            configuration.CreateMap<PhotoPost, PhotoDto>().ReverseMap();
            configuration.CreateMap<PhotoPost, GetPostForViewDto>().ReverseMap();

            configuration.CreateMap<PackagePost, ConfirmPackageDto>().ReverseMap();
            configuration.CreateMap<PackagePost, CancelPostDto>().ReverseMap();
            configuration.CreateMap<PackagePost, PackagePostDto>().ReverseMap();
            configuration.CreateMap<PackagePost, GetPackageViewDto>().ReverseMap();

            configuration.CreateMap<UserLikePost, PostLikeDto>().ReverseMap();
            configuration.CreateMap<UserLikePost, GetPostForLikeDto>().ReverseMap();

            configuration.CreateMap<UserComments, UserCommentDto>().ReverseMap();
            configuration.CreateMap<UserComments, UserCommentViewDto>().ReverseMap();

            configuration.CreateMap<VnPayments, VnPaymentRequestDto>().ReverseMap();
            configuration.CreateMap<Notification, NotificationDto>().ReverseMap();
            configuration.CreateMap<NotificationScheduleNew, NotificationScheduleNewDto>().ReverseMap();







            //configuration.CreateMap<UserRole, UserRoleDto>().ReverseMap();

        }
    }
}
