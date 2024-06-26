﻿using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.PackagePosts.Dto;
using AccommodationSearchSystem.Authorization;
using AccommodationSearchSystem.Authorization.Users;
using AccommodationSearchSystem.Entity;
using AccommodationSearchSystem.EntityFrameworkCore;
using AccommodationSearchSystem.Interfaces;
using AccommodationSearchSystem.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AccommodationSearchSystem.Authorization.Roles.StaticRoleNames;

namespace AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts
{
    [AbpAuthorize(PermissionNames.Pages_Posts)]
    public class ManagePostsAppService : ApplicationService, IManagePostsAppService
    {
        private readonly IRepository<Post, long> _repositoryPost;
        private readonly IRepository<AppointmentSchedule, long> _repositorySchedule;
        private readonly IRepository<PhotoPost, long> _repositoryPhotoPost;
        private readonly IRepository<User, long> _repositoryUser;
        private readonly IRepository<PackagePost, long> _repositoryPackagePost;
        private readonly IRepository<UserLikePost, long> _repositoryUserLikePost;
        private readonly AccommodationSearchSystemDbContext _dbContext;
        private readonly IPhotoService _photoService;

        public ManagePostsAppService(
            IRepository<Post, long> repositoryPost,
            IRepository<AppointmentSchedule, long> repositorySchedule,
            IRepository<PhotoPost, long> repositoryPhotoPost,
            IRepository<User, long> repositoryUser,
            IRepository<PackagePost, long> repositoryPackagePost,
            IPhotoService photoService,
            IRepository<UserLikePost, long> repositoryUserLikePost,
            AccommodationSearchSystemDbContext dbContext)

        {
            _repositoryPost = repositoryPost;
            _repositorySchedule = repositorySchedule;
            _photoService = photoService;
            _repositoryUser = repositoryUser;
            _dbContext = dbContext;
            _repositoryPhotoPost = repositoryPhotoPost;
            _repositoryPackagePost = repositoryPackagePost;
            _repositoryUserLikePost = repositoryUserLikePost;

        }
        public async Task CreateOrEdit(CreateOrEditIPostDto input)
        {
            if (input.Id == null)
            {
                await Create(input);
            } 
            else
            {
                await Update(input);
            }
        }

        protected virtual async Task Create(CreateOrEditIPostDto input)
        {
            var tenantId = AbpSession.TenantId;
            // Kiểm tra xem bài đăng đã tồn tại hay chưa
            var postCount =  _repositoryPost.GetAll().Where(e => e.Id == input.Id && e.TenantId == tenantId).Count();
            if (postCount >= 1)
            {
                throw new UserFriendlyException(00, L("ThisItemAlreadyExists"));
            } else
            {
                var post = ObjectMapper.Map<Post>(input);
                post.TenantId = tenantId;
                post.ConfirmAdmin = false;
                await _repositoryPost.InsertAsync(post);
            }
        }

        protected virtual async Task Update(CreateOrEditIPostDto input)
        {
            var post = await _repositoryPost.FirstOrDefaultAsync((long)input.Id);
                // Cập nhật các trường dữ liệu của bài đăng từ input
                ObjectMapper.Map(input, post);
         
        }

        public async Task DeletePost(EntityDto<long> input)
        {
            var postId = (long)input.Id;
            var post = await _repositoryPost.FirstOrDefaultAsync(e => e.Id == (long)input.Id);
            post.IsDeleted = true;

            await _repositoryPost.DeleteAsync(post.Id);
        }

        // Đăng lại bài viết
        public async Task RepostPost(CreateOrEditIPostDto input)
        {
            var post = await _repositoryPost.FirstOrDefaultAsync(e => e.Id == input.Id);
            post.RoomStatus = true;
            await _repositoryPost.UpdateAsync(post);
        }

        public async Task<bool> StatusRepostPost(CreateOrEditIPostDto input)
        {
            var tenantId = AbpSession.TenantId;
            var dataCheck = await _repositoryPost.FirstOrDefaultAsync(e => e.RoomStatus == true
                                             && e.Id == input.Id && tenantId == e.TenantId);
            if (dataCheck != null)
            {
                return true;
            }
            return false;
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAll(GetPostInputDto input)
        {
            var tenantId = AbpSession.TenantId;
            var query = from p in _repositoryPost.GetAll()
            .Where(e => tenantId == e.TenantId && e.ConfirmAdmin == true)
            .Where(e => (input.filterText == null || e.Title.Contains(input.filterText))
                                || (e.District.Contains(input.filterText) && e.RoomPrice.Equals(input.filterText) && e.Square.Equals(input.filterText)))
                            //orderby p.Id descending
                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id into uGroup
                        from u in uGroup.DefaultIfEmpty()
                        where u.TenantId == p.TenantId

                        join pk in _repositoryPackagePost.GetAll().AsNoTracking() on p.CreatorUserId equals pk.CreatorUserId into pkGroup
                        from pk in pkGroup.DefaultIfEmpty()
                        where pk == null || (pk.Cancel == false && pk.Confirm == true && pk.PackageType == "Gói VIP")
                        orderby pk.PackageType == null ? 0 : 1 descending, p.Id descending
                        //join s in _repositorySchedule.GetAll().AsNoTracking() on p.Id equals s.PostId into sGroup
                        //from s in sGroup.DefaultIfEmpty()
                        //where s == null || (s.TenantId == tenantId && (s.Confirm == null || s.Confirm == false))

                        //select new { Post = p, PackagePost = pk, Photos = _repositoryPhotoPost.GetAll().AsNoTracking().Where(ph => ph.PostId == p.Id).ToList() };
                        select new { Post = p, User = u, PackagePost = pk, Photos = _repositoryPhotoPost.GetAll().AsNoTracking().Where(ph => ph.PostId == p.Id).ToList() };

            var totalCount = await query.CountAsync();
            var pagedAndFilteredPost = query.PageBy(input);

            var result = await pagedAndFilteredPost.ToListAsync();

            var postDtos = result.Select(item => new GetPostForViewDto
            {
                Id = item.Post.Id,
                PostCode = item.Post.PostCode,
                Title = item.Post.Title,
                ContentPost = item.Post.ContentPost,
                Photo = item.Post.Photo,
                RoomPrice = item.Post.RoomPrice,
                Address = item.Post.Address,
                District = item.Post.District,
                City = item.Post.City,
                Ward= item.Post.Ward,
                Area = item.Post.Area,
                Square = item.Post.Square,
                PriceCategory = item.Post.PriceCategory,
                Wifi = item.Post.Wifi,
                Parking = item.Post.Parking,
                Conditioner = item.Post.Conditioner,
                RoomStatus = item.Post.RoomStatus,
                TenantId = tenantId,
                PackageType = item.PackagePost != null ? item.PackagePost.PackageType : "Gói thường",
                CreateByName = item.User.FullName,
                EmailAddress = item.User.EmailAddress,
                PhoneNumber = item.User.PhoneNumber,
                Photos = item.Photos.Select(photo => new PhotoDto
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId
                }).ToList(),
            }).ToList();

            // Lấy danh sách lượt thích của mỗi bài viết
            var totalLike = await GetTotalLike();
            // Gán tổng số  lượt thích cho mỗi bài viết
            foreach (var item in postDtos)
            {
                var likeInfor = totalLike.FirstOrDefault(l => l.TenantId == tenantId && l.PostId == item.Id);
                item.TotalLike = likeInfor?.Count ?? 0;
            }

            return new PagedResultDto<GetPostForViewDto>(totalCount, postDtos);

        }

        public async Task<GetPostForEditOutput> GetLoyaltyGiftItemForEdit(EntityDto<long> input)
        {
            var tenantId = AbpSession.TenantId;
            var datapost = await _repositoryPost.FirstOrDefaultAsync(input.Id);
            //var datapostView = await _repositoryPost.FirstOrDefaultAsync(input.Id);

            var dataConfirmAdmin = await _repositoryPost.FirstOrDefaultAsync(input.Id);

            var datapostView = from p in _repositoryPost.GetAll()
                       .Where(e => tenantId == e.TenantId && e.Id == input.Id)
                        orderby p.Id descending
                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id
                        where u.TenantId == p.TenantId
                        select new
                        {
                            Post = p,
                            User = u
                        };

            var result = await datapostView.FirstOrDefaultAsync();

            if (result == null)
            {
                throw new UserFriendlyException("Bài đăng không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var post = result.Post;
            var user = result.User;

            var photoData = await _repositoryPhotoPost.GetAllListAsync(e => e.PostId == input.Id);
            var output = new GetPostForEditOutput
            {
                CreateOrEditPost = ObjectMapper.Map<CreateOrEditIPostDto>(datapost),
                //GetPostForView = ObjectMapper.Map<GetPostForViewDto>(datapostView),
                ConfirmPostByAdmins = ObjectMapper.Map<ConfirmPostByAdminDto>(dataConfirmAdmin),
                EmailAddress = user.EmailAddress,
                PhoneNumber = user.PhoneNumber,
                CreateByName = user.FullName,
                // Khởi tạo danh sách hình ảnh
                Photos = new List<PhotoDto>()
            };
            // Nếu có thông tin về hình ảnh
            if (photoData != null)
            {
                output.Photos = photoData.Select(photo => new PhotoDto
                {
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId,
                    Id = photo.Id
                }).ToList();
            }

            return output;
        }

        public async Task<ActionResult<PhotoDto>> AddPhoto(long Id, IFormFile file)
        {
            //var post = await _repositoryPost.FirstOrDefaultAsync(Id);

            var tenantId = AbpSession.TenantId;
            var query = from p in _repositoryPost.GetAll()
                        .Where(e => tenantId == e.TenantId && e.Id == Id)
                        select new
                        {
                            Post = p
                        };

            var posts = await query.FirstOrDefaultAsync();

            if (posts == null)
            {
                throw new UserFriendlyException("Bài đăng không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var post = posts.Post;

            var photoData = await _repositoryPhotoPost.GetAllListAsync(e => e.PostId == Id);

            var output = new GetPostForViewDto
            {
                Id = post.Id,
                PostCode = post.PostCode,
                Title = post.Title,
                ContentPost = post.ContentPost,
                Photo = post.Photo,
                RoomPrice = post.RoomPrice,
                Address = post.Address,
                Area = post.Area,
                Square = post.Square,
                PriceCategory = post.PriceCategory,
                Wifi = post.Wifi,
                Parking = post.Parking,
                Conditioner = post.Conditioner,
                RoomStatus = post.RoomStatus,
                TenantId = tenantId,
                Photos = photoData.Select(photo => new PhotoDto
                {
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId,
                    Id = photo.Id,
                }).ToList()
            };

            var result = await _photoService.AddPhotoAsync(file);

            // Kiểm tra lỗi
            if (result.Error != null)
            {
                throw new UserFriendlyException(result.Error.Message);
            }

            // Khởi tạo danh sách nếu chưa được khởi tạo
            if (post.PhotoPosts == null)
            {
                post.PhotoPosts = new List<PhotoPost>();
            }

            var photo = new PhotoPost
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                PostId = (int)post.Id
            };

            if (post.PhotoPosts != null && post.PhotoPosts.Any())
            {
                photo.IsMain = false;
            }
            else
            {
                photo.IsMain = true;
            }
            post.PhotoPosts.Add(photo);

            // Lưu cấc thay đổi
            return new CreatedAtRouteResult( new { id = post.Id }, ObjectMapper.Map<PhotoDto>(photo));
  
        }

        public async Task<ActionResult> SetMainPhoto(long Id, int photoId)
        {
            var tenantId = AbpSession.TenantId;
            var query = from p in _repositoryPost.GetAll()
                        .Where(e => tenantId == e.TenantId && e.Id == Id)
                        select new
                        {
                            Post = p
                        };

            var posts = await query.FirstOrDefaultAsync();

            if (posts == null)
            {
                throw new UserFriendlyException("Bài đăng không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var post = posts.Post;

            var photoData = await _repositoryPhotoPost.GetAllListAsync(e => e.PostId == Id);

            var output = new GetPostForViewDto
            {
                Id = post.Id,
                PostCode = post.PostCode,
                Title = post.Title,
                ContentPost = post.ContentPost,
                Photo = post.Photo,
                RoomPrice = post.RoomPrice,
                Address = post.Address,
                Area = post.Area,
                Square = post.Square,
                PriceCategory = post.PriceCategory,
                Wifi = post.Wifi,
                Parking = post.Parking,
                Conditioner = post.Conditioner,
                RoomStatus = post.RoomStatus,
                TenantId = tenantId,
                Photos = photoData.Select(photo => new PhotoDto
                {
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId,
                    Id = photo.Id,
                }).ToList()
            };

            var photo = post.PhotoPosts.FirstOrDefault(e => e.Id == photoId);
            if (photo == null)
            {
                throw new UserFriendlyException("Không tìm thấy hình ảnh nào");
            }
            if (photo.IsMain)
            {
                throw new UserFriendlyException("Bức ảnh này đã là ảnh chính");
            }
            // Lấy ảnh chính hiện tại
            var currentMain = post.PhotoPosts.FirstOrDefault(e => e.IsMain);
            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }
            photo.IsMain = true;

            return new CreatedAtRouteResult(new { id = post.Id }, ObjectMapper.Map<PhotoDto>(photo));
        }

        public async Task<ActionResult> DeletePhoto(long Id, int photoId)
        {
            var tenantId = AbpSession.TenantId;
            var query = from p in _repositoryPost.GetAll()
                        .Where(e => tenantId == e.TenantId && e.Id == Id)
                        select new
                        {
                            Post = p
                        };

            var posts = await query.FirstOrDefaultAsync();

            if (posts == null)
            {
                throw new UserFriendlyException("Bài đăng không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var post = posts.Post;

            var photoData = await _repositoryPhotoPost.GetAllListAsync(e => e.PostId == Id);

            var output = new GetPostForViewDto
            {
                Id = post.Id,
                PostCode = post.PostCode,
                Title = post.Title,
                ContentPost = post.ContentPost,
                Photo = post.Photo,
                RoomPrice = post.RoomPrice,
                Address = post.Address,
                Area = post.Area,
                Square = post.Square,
                PriceCategory = post.PriceCategory,
                Wifi = post.Wifi,
                Parking = post.Parking,
                Conditioner = post.Conditioner,
                RoomStatus = post.RoomStatus,
                TenantId = tenantId,
                Photos = photoData.Select(photo => new PhotoDto
                {
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId,
                    Id = photo.Id,
                }).ToList()
            };

            var photo = post.PhotoPosts.FirstOrDefault(e => e.Id == photoId);
            if (photo == null)
            {
                throw new UserFriendlyException("Không tìm thấy hình ảnh nào");
            }
            
            if (photo.IsMain)
            {
                throw new UserFriendlyException("Bạn không thể xóa hình ảnh chính");
            }
            
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            }
             
            post.PhotoPosts.Remove(photo);
            photo.IsDeleted = true;
            photo.DeleterUserId = post.CreatorUserId;
            photo.DeletionTime = DateTime.Now;

            return new CreatedAtRouteResult(new { id = post.Id }, ObjectMapper.Map<PhotoDto>(photo));
        }

        public async Task<GetPostForViewDto> GetForEdit(EntityDto<long> input)
        {
            var tenantId = AbpSession.TenantId;

            var query = from p in _repositoryPost.GetAll()
                        .Where(e => tenantId == e.TenantId && e.Id == input.Id)
                        orderby p.Id descending
                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id
                        where u.TenantId == p.TenantId
                        select new
                        {
                            Post = p,
                            User = u
                        };

            var result = await query.FirstOrDefaultAsync();

            if (result == null)
            {
                throw new UserFriendlyException("Bài đăng không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var post = result.Post;
            var user = result.User;

            var photoData = await _repositoryPhotoPost.GetAllListAsync(e => e.PostId == input.Id);

            var output = new GetPostForViewDto
            {
                Id = post.Id,
                PostCode = post.PostCode,
                Title = post.Title,
                ContentPost = post.ContentPost,
                Photo = post.Photo,
                RoomPrice = post.RoomPrice,
                Address = post.Address,
                District = post.District,
                City = post.City,
                Ward = post.Ward,
                Area = post.Area,
                Square = post.Square,
                PriceCategory = post.PriceCategory,
                Wifi = post.Wifi,
                Parking = post.Parking,
                Conditioner = post.Conditioner,
                RoomStatus = post.RoomStatus,
                TenantId = tenantId,
                EmailAddress = user.EmailAddress,
                PhoneNumber = user.PhoneNumber,
                Photos = photoData.Select(photo => new PhotoDto
                {
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId,
                    Id = photo.Id,
                }).ToList()
            };

            return output;
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAllForHostVIP(GetPostInputDto input)
        {
            var tenantId = AbpSession.TenantId;
            var query = from p in _repositoryPost.GetAll()
            .Where(e => tenantId == e.TenantId && e.ConfirmAdmin == true)
            .Where(e => (input.filterText == null || e.Title.Contains(input.filterText))
                               || (e.District.Contains(input.filterText) && e.RoomPrice.Equals(input.filterText) && e.Square.Equals(input.filterText)))
                        orderby p.Id descending

                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id into uGroup
                        from u in uGroup.DefaultIfEmpty()
                        where u.TenantId == p.TenantId

                        join pk in _repositoryPackagePost.GetAll().AsNoTracking() on p.CreatorUserId equals pk.CreatorUserId into pkGroup
                        from pk in pkGroup.DefaultIfEmpty()
                        where pk.Cancel == false && pk.Confirm == true && pk.PackageType == "Gói VIP pro"

                        select new { Post = p, User = u, PackagePost = pk, Photos = _repositoryPhotoPost.GetAll().AsNoTracking().Where(ph => ph.PostId == p.Id).ToList() };

            var totalCount = await query.CountAsync();
            var pagedAndFilteredPost = query.PageBy(input);

            var result = await pagedAndFilteredPost.ToListAsync();

            var postDtos = result.Select(item => new GetPostForViewDto
            {
                Id = item.Post.Id,
                PostCode = item.Post.PostCode,
                Title = item.Post.Title,
                ContentPost = item.Post.ContentPost,
                Photo = item.Post.Photo,
                RoomPrice = item.Post.RoomPrice,
                Address = item.Post.Address,
                District = item.Post.District,
                City = item.Post.City,
                Ward = item.Post.Ward,
                Area = item.Post.Area,
                Square = item.Post.Square,
                PriceCategory = item.Post.PriceCategory,
                Wifi = item.Post.Wifi,
                Parking = item.Post.Parking,
                Conditioner = item.Post.Conditioner,
                RoomStatus = item.Post.RoomStatus,
                TenantId = tenantId,
                CreateByName = item.User.FullName,
                EmailAddress = item.User.EmailAddress,
                PhoneNumber = item.User.PhoneNumber,
                PackageType = item.PackagePost != null ? item.PackagePost.PackageType : null,
                Photos = item.Photos.Select(photo => new PhotoDto
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId
                }).ToList(),
            }).ToList();

            // Lấy danh sách lượt thích của mỗi bài viết
            var totalLike = await GetTotalLike();
            // Gán tổng số  lượt thích cho mỗi bài viết
            foreach (var item in postDtos)
            {
                var likeInfor = totalLike.FirstOrDefault(l => l.TenantId == tenantId && l.PostId == item.Id);
                item.TotalLike = likeInfor?.Count ?? 0;
            }

            return new PagedResultDto<GetPostForViewDto>(totalCount, postDtos);
        }

        public async Task<List<PostLikeDto>> GetTotalLike()
        {
            var tenantId = AbpSession.TenantId;
            var data = new List<PostLikeDto>();

            // Lấy sô lượng yêu thích theo từng bài đăng
            var postCountLike = await _repositoryUserLikePost.GetAll()
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .GroupBy(p => p.PostId)
                .Select(g => new { TenantId = tenantId, PostId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Chuyển kết quả thành DTO
            data = postCountLike.Select(pc => new PostLikeDto
            {
                TenantId = pc.TenantId,
                PostId = pc.PostId,
                Count = pc.Count
            }).ToList();

            return data;
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAllForHost(GetPostInputDto input)
        {
            var tenantId = AbpSession.TenantId;
            var UserId = AbpSession.UserId;
            var query = from p in _repositoryPost.GetAll()
            .Where(e => tenantId == e.TenantId && UserId == e.CreatorUserId)
            .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                                || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))
                        orderby p.Id descending

                        //join s in _repositorySchedule.GetAll().AsNoTracking() on p.Id equals s.PostId into sGroup
                        //from s in sGroup.DefaultIfEmpty()
                        //where s == null || (s.TenantId == tenantId && (s.Confirm == null || s.Confirm == false))

                        select new { Post = p, Photos = _repositoryPhotoPost.GetAll().AsNoTracking().Where(ph => ph.PostId == p.Id).ToList() };

            var totalCount = await query.CountAsync();
            var pagedAndFilteredPost = query.PageBy(input);

            var result = await pagedAndFilteredPost.ToListAsync();

            var postDtos = result.Select(item => new GetPostForViewDto
            {
                Id = item.Post.Id,
                PostCode = item.Post.PostCode,
                Title = item.Post.Title,
                ContentPost = item.Post.ContentPost,
                Photo = item.Post.Photo,
                RoomPrice = item.Post.RoomPrice,
                Address = item.Post.Address,
                District = item.Post.District,
                City = item.Post.City,
                Ward = item.Post.Ward,
                Area = item.Post.Area,
                Square = item.Post.Square,
                PriceCategory = item.Post.PriceCategory,
                Wifi = item.Post.Wifi,
                Parking = item.Post.Parking,
                Conditioner = item.Post.Conditioner,
                RoomStatus = item.Post.RoomStatus,
                TenantId = tenantId,
                Photos = item.Photos.Select(photo => new PhotoDto
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId
                }).ToList(),
            }).ToList();

            return new PagedResultDto<GetPostForViewDto>(totalCount, postDtos);
        }

        public async Task<PagedResultDto<GetPostForViewDto>> GetAllForAdmin(GetPostInputDto input)
        {
            var tenantId = AbpSession.TenantId;
            var query = from p in _repositoryPost.GetAll()
            .Where(e => tenantId == e.TenantId)
            .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                                || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))
                        orderby p.Id descending
                        join u in _repositoryUser.GetAll() on p.CreatorUserId equals u.Id

                        join pk in _repositoryPackagePost.GetAll() on u.Id equals pk.HostId into pkGroup
                        from pk in pkGroup.DefaultIfEmpty()
                        where pk == null || (pk.IsDeleted == false && pk.Cancel == false && ( pk.PackageType == "Gói VIP pro" || pk.PackageType == "Gói VIP"))
                        //join s in _repositorySchedule.GetAll().AsNoTracking() on p.Id equals s.PostId into sGroup
                        //from s in sGroup.DefaultIfEmpty()
                        //where s == null || (s.TenantId == tenantId && (s.Confirm == null || s.Confirm == false))

                        select new { Post = p, User = u, PackagePost = pk, Photos = _repositoryPhotoPost.GetAll().AsNoTracking().Where(ph => ph.PostId == p.Id).ToList() };

            var totalCount = await query.CountAsync();
            var pagedAndFilteredPost = query.PageBy(input);

            var result = await pagedAndFilteredPost.ToListAsync();

            var postDtos = result.Select(item => new GetPostForViewDto
            {
                Id = item.Post.Id,
                PostCode = item.Post.PostCode,
                Title = item.Post.Title,
                ContentPost = item.Post.ContentPost,
                Photo = item.Post.Photo,
                RoomPrice = item.Post.RoomPrice,
                Address = item.Post.Address,
                District = item.Post.District,
                City = item.Post.City,
                Ward = item.Post.Ward,
                Area = item.Post.Area,
                Square = item.Post.Square,
                PriceCategory = item.Post.PriceCategory,
                Wifi = item.Post.Wifi,
                Parking = item.Post.Parking,
                Conditioner = item.Post.Conditioner,
                RoomStatus = item.Post.RoomStatus,
                TenantId = tenantId,
                CreateByName = item.User.FullName,
                PhoneNumber = item.User.PhoneNumber,
                EmailAddress = item.User.EmailAddress,
                ConfirmAdmin = item.Post.ConfirmAdmin,
                PackageType = item.PackagePost != null ? item.PackagePost.PackageType : "Gói thường",
                Photos = item.Photos.Select(photo => new PhotoDto
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId
                }).ToList(),
            }).ToList();

            return new PagedResultDto<GetPostForViewDto>(totalCount, postDtos);

        }

        public async Task ConfirmPostAD(ConfirmPostByAdminDto input)
        {
            var tenantId = AbpSession.TenantId;
            var UserId = AbpSession.UserId;
            var dataCheck = await _repositoryPost.FirstOrDefaultAsync(e => e.ConfirmAdmin == true
                                    && e.Id == input.Id && tenantId == e.TenantId);
            if (dataCheck != null)
            {
                throw new UserFriendlyException(400, "Bài đăng đã được phê duyệt");
            }
            else
            {
                var post = await _repositoryPost.FirstOrDefaultAsync(e => e.Id == input.Id);
                post.ConfirmAdmin = true;
                await _repositoryPost.UpdateAsync(post);
            }
        }

        public async Task<bool> StatusConfirmAD(ConfirmPostByAdminDto input)
        {
            var tenantId = AbpSession.TenantId;
            var dataCheck = await _repositoryPost.FirstOrDefaultAsync(e => e.ConfirmAdmin == true
                                             && e.Id == input.Id && tenantId == e.TenantId);
            if (dataCheck != null)
            {
                return true;
            }
            return false;
        }


        public Task<PostLikeDto> LikePosts(EntityDto<long> input)
        {
            throw new System.NotImplementedException();
        }
    }
}
