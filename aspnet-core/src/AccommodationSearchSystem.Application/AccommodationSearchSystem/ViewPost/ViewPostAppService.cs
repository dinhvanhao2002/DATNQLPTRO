using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using AccommodationSearchSystem.AccommodationSearchSystem.ManageAppointmentSchedules.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts;
using AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.Statistical.Dto;
using AccommodationSearchSystem.Authorization;
using AccommodationSearchSystem.Authorization.Users;
using AccommodationSearchSystem.Entity;
using AccommodationSearchSystem.EntityFrameworkCore;
using AccommodationSearchSystem.Interfaces;
using AccommodationSearchSystem.Migrations;
using AccommodationSearchSystem.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AccommodationSearchSystem.Authorization.Roles.StaticRoleNames;

namespace AccommodationSearchSystem.AccommodationSearchSystem.ViewPost
{
    [AbpAuthorize(PermissionNames.Pages_View_Posts)]
    public class ViewPostAppService : ApplicationService, IManagePostsAppService
    {
        private readonly IRepository<Post, long> _repositoryPost;
        private readonly IRepository<Tenant, int> _tenantRepo;
        private readonly IRepository<User, long> _repositoryUser;
        private readonly IRepository<AppointmentSchedule, long> _repositorySchedule;
        private readonly IPhotoService _photoService;
        private readonly IRepository<PhotoPost, long> _repositoryPhotoPost;
        private readonly IRepository<PackagePost, long> _repositoryPackagePost;
        private readonly IRepository<UserLikePost, long> _repositoryUserLikePost;
        private readonly AccommodationSearchSystemDbContext _context;

        public ViewPostAppService(
            IRepository<Post, long> repositoryPost,
            IRepository<Accommodate, long> repositoryAccommodate,
            IRepository<AppointmentSchedule, long> repositorySchedule,
            IRepository<PhotoPost, long> repositoryPhotoPost,
            IRepository<PackagePost, long> repositoryPackagePost,
            IPhotoService photoService,
            IRepository<User, long> repositoryUser,
            AccommodationSearchSystemDbContext context,
            IRepository<UserLikePost, long> repositoryUserLikePost,
            IRepository<Tenant, int> tenantRepo)

        {
            _repositoryPost = repositoryPost;
            _tenantRepo = tenantRepo;
            _repositoryUser = repositoryUser;
            _repositorySchedule = repositorySchedule;
            _photoService = photoService;
            _repositoryPhotoPost = repositoryPhotoPost;
            _repositoryPackagePost = repositoryPackagePost;
            _repositoryUserLikePost = repositoryUserLikePost;
            _context = context;

        }

        public Task CreateOrEdit(CreateOrEditIPostDto input)
        {
            throw new NotImplementedException();
        }

        public Task DeletePost(EntityDto<long> input)
        {
            throw new NotImplementedException();
        }

        // Lấy chi tiết bài đăng
        #region Lấy chi tiết bài đăng đó khi click vào, tham số truyền vào Id 
        [AbpAllowAnonymous]
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
                            Post = p,  // lấy toàn bộ thuộc tính bảng Post và bảng User
                            User = u
                        };

            var result = await query.FirstOrDefaultAsync();

            if (result == null)
            {
                throw new UserFriendlyException("Bài đăng không tồn tại hoặc bạn không có quyền truy cập.");
            }

            var post = result.Post;
            var user = result.User;

            // Lấy toàn bộ ảnh đối với bài đăng đó 
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
                CreateByName = user.FullName,
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
        #endregion


        [AbpAllowAnonymous]
        public async Task<PagedResultDto<GetPostForViewDto>> GetAll(GetPostInputDto input)
        {
            int? tenantId = AbpSession.TenantId;

            var query = from p in _repositoryPost.GetAll()
                        .Where(e => e.ConfirmAdmin == true && (tenantId == null || tenantId == e.TenantId))
                        .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                                    || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))

                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id into uGroup
                        from u in uGroup.DefaultIfEmpty()
                        where tenantId == null || u.TenantId == p.TenantId

                        join pk in _repositoryPackagePost.GetAll().AsNoTracking() on p.CreatorUserId equals pk.CreatorUserId into pkGroup
                        from pk in pkGroup.DefaultIfEmpty()
                        where pk == null || (pk.Cancel == false && pk.Confirm == true && pk.PackageType == "Gói VIP")
                        orderby pk.PackageType == null ? 0 : 1 descending, p.Id descending
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
                PackageType = item.PackagePost != null ? item.PackagePost.PackageType : "Gói thường",
                TenantId = tenantId,
                CreateByName = item.User?.FullName,
                EmailAddress = item.User?.EmailAddress,
                PhoneNumber = item.User?.PhoneNumber,
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
            // Gán tổng số lượt thích cho mỗi bài viết
            foreach (var item in postDtos)
            {
                var likeInfor = totalLike.FirstOrDefault(l => l.TenantId == tenantId && l.PostId == item.Id);
                item.TotalLike = likeInfor?.Count ?? 0;
            }

            return new PagedResultDto<GetPostForViewDto>(totalCount, postDtos);
        }
        [AbpAllowAnonymous]
        public async Task<PagedResultDto<GetPostForViewDto>> GetAllForHostVIP(GetPostInputDto input)
        {
            var tenantId = AbpSession.TenantId;
            //var UserId = AbpSession.UserId;
            var query = from p in _repositoryPost.GetAll()
            .Where(e => tenantId == e.TenantId && e.ConfirmAdmin == true)
            .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                                || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))
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




        //Sửa lại hàm tìm kiếm

        #region GET ALL NEW 
        [AbpAllowAnonymous]
        public async Task<PagedResultDto<GetPostForViewDto>> GetAllNEW(GetPostInputNewDto input)
        {
            int? tenantId = AbpSession.TenantId;

            var query = from p in _repositoryPost.GetAll()
                        .Where(e => e.ConfirmAdmin == true && (tenantId == null || tenantId == e.TenantId))
                         .Where(e => e.PriceCategory == input.PriceCategory || input.PriceCategory == null)
                         .Where(e => e.District == input.District || input.District == null)
                         //Check điều kiện diện tích
                         .Where(e => (input.Square == 1 && e.Square < 20) || (input.Square == 2 && (e.Square >= 20 && e.Square < 30)) || (input.Square == 3 && (e.Square >= 30 && e.Square < 40)) || (input.Square == 4 && e.Square >= 40) || input.Square == null)
                         // Check điều kiện giá phòng
                         .Where(e => (input.RoomPrice == 1 && (e.RoomPrice < 1)) || (input.RoomPrice == 2 && (e.RoomPrice >= 1 && e.RoomPrice < 2)) || (input.RoomPrice == 3 && (e.RoomPrice >= 2 && e.RoomPrice < 3)) || (input.RoomPrice == 4 && (e.RoomPrice >= 4)) || input.RoomPrice == null)
                        .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                                    || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))

                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id into uGroup
                        from u in uGroup.DefaultIfEmpty()
                        where tenantId == null || u.TenantId == p.TenantId

                        join pk in _repositoryPackagePost.GetAll().AsNoTracking() on p.CreatorUserId equals pk.CreatorUserId into pkGroup
                        from pk in pkGroup.DefaultIfEmpty()
                        where pk == null || (pk.Cancel == false && pk.Confirm == true && pk.PackageType == "Gói VIP")
                        orderby pk.PackageType == null ? 0 : 1 descending, p.Id descending
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
                PackageType = item.PackagePost != null ? item.PackagePost.PackageType : "Gói thường",
                TenantId = tenantId,
                CreateByName = item.User?.FullName,
                EmailAddress = item.User?.EmailAddress,
                PhoneNumber = item.User?.PhoneNumber,
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
            // Gán tổng số lượt thích cho mỗi bài viết
            foreach (var item in postDtos)
            {
                var likeInfor = totalLike.FirstOrDefault(l => l.TenantId == tenantId && l.PostId == item.Id);
                item.TotalLike = likeInfor?.Count ?? 0;
            }

            return new PagedResultDto<GetPostForViewDto>(totalCount, postDtos);
        }
        #endregion




        #region Làm lại tìm kiếm tin nổi bật
        [AbpAllowAnonymous]
        public async Task<PagedResultDto<GetPostForViewDto>> GetAllForHostVIPNEW(GetPostInputNewDto input)
        {
            var tenantId = AbpSession.TenantId;
            //var UserId = AbpSession.UserId;
            var query = from p in _repositoryPost.GetAll()
            .Where(e => tenantId == e.TenantId && e.ConfirmAdmin == true)
             .Where(e => e.PriceCategory == input.PriceCategory || input.PriceCategory == null)
            .Where(e => e.District == input.District || input.District == null)
            //Check điều kiện diện tích
            .Where(e => (input.Square == 1 && e.Square < 20) || (input.Square == 2 && (e.Square >= 20 || e.Square < 30)) || (input.Square == 3 && (e.Square >= 30 || e.Square < 40)) || (input.Square == 4 && e.Square >= 40) || input.Square == null)
            // Check điều kiện giá phòng
            .Where(e => (input.RoomPrice == 1 && (e.RoomPrice < 1)) || (input.RoomPrice == 2 && (e.RoomPrice >= 1 || e.RoomPrice < 2)) || (input.RoomPrice == 3 && (e.RoomPrice >= 2 || e.RoomPrice < 3)) || (input.RoomPrice == 4 && (e.RoomPrice >= 4)) || input.RoomPrice == null)
            .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                                || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))
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
        #endregion

        #region Check Status = true Phòng vẫn còn hoạt động

        [AbpAllowAnonymous]
        public async Task<bool> StatusRoom(long Id)
        {
            // Kiểm tra có RoomStatus == true
            var data = await _repositoryPost.FirstOrDefaultAsync(e => e.Id == Id && e.TenantId == AbpSession.TenantId && e.RoomStatus == true);
            if (data != null)
            {
                // Nếu có
                return true;
            }
            // Nếu không có
            else return false;
        }
        #endregion


        [AbpAllowAnonymous]
        public Task<GetPostForEditOutput> GetLoyaltyGiftItemForEdit(EntityDto<long> input)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResultDto<GetPostForViewDto>> GetAllForHost(GetPostInputDto input)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResultDto<GetPostForViewDto>> GetAllForAdmin(GetPostInputDto input)
        {
            throw new NotImplementedException();
        }

        public Task ConfirmPostAD(ConfirmPostByAdminDto input)
        {
            throw new NotImplementedException();
        }


        // QUẢN LÝ BÀI ĐĂNG YÊU THÍCH
        [AbpAllowAnonymous]
        public async Task<PostLikeDto> LikePosts(EntityDto<long> input)
        {
            var tenantId = AbpSession.TenantId;
            var userId = AbpSession.UserId;

            var post = await (from p in _repositoryPost.GetAll()
                              where p.Id == input.Id
                              select new PostLikeDto
                              {
                                  PostId = (int)p.Id,
                                  HostId = (int)userId,
    
                              }).FirstOrDefaultAsync();
            // Kiểm tra xem bài đăng đã được yêu thích hay chưa
            var postCount = await (from ul in _repositoryUserLikePost.GetAll()
                                   join user in _repositoryUser.GetAll() on ul.HostId equals user.Id
                                   where ul.PostId == input.Id && ul.TenantId == tenantId && ul.Like == true && ul.HostId == userId && ul.IsDeleted == false
                                   select ul).CountAsync();

            if (postCount >= 1)
            {
                throw new UserFriendlyException(00, "Bạn đã yêu thích bài đăng này ");
            }
            else
            {
                if (post == null)
                {
                    // Xử lý khi không tìm thấy bài đăng tương ứng
                    throw new UserFriendlyException(00, L("PostNotFound"));
                }

                // Khởi tạo đối tượng Schedule và chèn vào repository
                var userlike = ObjectMapper.Map<UserLikePost>(post);
                userlike.TenantId = tenantId;
                userlike.Like = true;
                await _repositoryUserLikePost.InsertAsync(userlike);
            }
            // Trả về thông tin về schedule đã được tạo
            return post;
        }
        [AbpAllowAnonymous]
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

        #region Check để lấy ra bài đăng đã được yêu thích hay chưa 
        [AbpAllowAnonymous]
        public async Task<bool> StatusRoomLike(long Id)
        {
            // Kiểm tra có RoomStatus == true
            var tenantId = AbpSession.TenantId;

            var userId = AbpSession.UserId;
            var post = await (from p in _repositoryPost.GetAll()
                              where p.Id == Id
                              select new PostLikeDto
                              {
                                  PostId = (int)p.Id,
                                  HostId = userId.HasValue ? (int)userId.Value : 0,
                                  Like = true,
                                  TenantId = tenantId

                              }).FirstOrDefaultAsync();
            //var likeCount = _repositoryUserLikePost.GetAll().Where(e => e.TenantId == tenantId && e.HostId == userId && e.Like == true && e.PostId == post.PostId).Count();
            var likeCountQuery = _repositoryUserLikePost.GetAll().Where(e => e.TenantId == tenantId && e.Like == true && e.PostId == post.PostId);

            if (userId.HasValue)
            {
                likeCountQuery = likeCountQuery.Where(e => e.HostId == userId.Value);
            }
            var likeCount = await likeCountQuery.CountAsync();
            if (likeCount >= 1)
            {
                return true; 
            }
            else
            {
                return false;
            }

        }
        #endregion

        #region Lấy toàn bộ bài đăng được like 

        [AbpAllowAnonymous]
        public async Task<PagedResultDto<GetPostForLikeDto>> GetAllLike(GetLikesInputDto input)
        {
            var tenantId = AbpSession.TenantId;
            var UserId = AbpSession.UserId;

            var query = from p in _repositoryPost.GetAll()
           .Where(e => tenantId == e.TenantId && e.ConfirmAdmin == true)
           .Where(e => input.filterText == null || e.Title.Contains(input.filterText)
                               || e.Address.Contains(input.filterText) || e.RoomPrice.Equals(input.filterText))
                        
                        join l in _repositoryUserLikePost.GetAll().AsNoTracking() on p.Id equals l.PostId
                        where tenantId == l.TenantId && UserId == l.CreatorUserId && l.Like == true

                        join u in _repositoryUser.GetAll().AsNoTracking() on p.CreatorUserId equals u.Id into uGroup
                        from u in uGroup.DefaultIfEmpty()
                        where u.TenantId == p.TenantId

                        join pk in _repositoryPackagePost.GetAll().AsNoTracking() on p.CreatorUserId equals pk.CreatorUserId into pkGroup
                        from pk in pkGroup.DefaultIfEmpty()
                        where pk == null || (pk.Cancel == false && pk.Confirm == true)
                        orderby pk.PackageType == null ? 0 : 1 descending, p.Id descending

                        select new { Post = p, User = u, PackagePost = pk, Like = l, Photos = _repositoryPhotoPost.GetAll().AsNoTracking().Where(ph => ph.PostId == p.Id).ToList() };

            var totalCount = await query.CountAsync();
            var pagedAndFilteredPost = query.PageBy(input);

            var result = await pagedAndFilteredPost.ToListAsync();

            var postDtos = result.Select(item => new GetPostForLikeDto
            {
                Id = item.Post.Id,
                LikeId = (int)item.Like.Id,
                HostId = item.Like.HostId,
                PostId = (int)item.Like.Id,
                Like = item.Like.Like,
                Title = item.Post.Title,
                ContentPost = item.Post.ContentPost,
                RoomPrice = item.Post.RoomPrice,
                Address = item.Post.Address,
                District = item.Post.District,
                City = item.Post.City,
                Ward = item.Post.Ward,
                Square = item.Post.Square,
                PriceCategory = item.Post.PriceCategory,
                Wifi = item.Post.Wifi,
                Parking = item.Post.Parking,
                Conditioner = item.Post.Conditioner,
                RoomStatus = item.Post.RoomStatus,
                CreateByName = item.User.FullName,
                PhoneNumber = item.User.PhoneNumber,
                PackageType = item.PackagePost != null ? item.PackagePost.PackageType : "Gói thường",
                TenantId = tenantId,
                Photos = item.Photos.Select(photo => new PhotoDto
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsMain = photo.IsMain,
                    PostId = photo.PostId
                }).ToList(),
            }).ToList();

            return new PagedResultDto<GetPostForLikeDto>(totalCount, postDtos);

        }
        #endregion

        #region Xóa bài đăng
        [AbpAllowAnonymous]
        public async Task DeletePostLike(EntityDto<long> input)
        {
            var LikeID = (long)input.Id;
            var likePost = await _repositoryUserLikePost.FirstOrDefaultAsync(e => e.Id == LikeID);
            likePost.IsDeleted = true;

            await _repositoryUserLikePost.DeleteAsync(likePost.Id);
        }
        #endregion
    }
}
