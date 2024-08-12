using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.UI;
using AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto;
using AccommodationSearchSystem.AccommodationSearchSystem.UserComment.Dto;
using AccommodationSearchSystem.Authorization.Users;
using AccommodationSearchSystem.Chat.Signalr;
using AccommodationSearchSystem.Entity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.UserComment
{
    public class UserCommentAppService : ApplicationService, IUserCommentAppService
    {
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IRepository<UserComments, long> _repositoryComment;
        private readonly IRepository<User, long> _repositoryUser;
        private readonly IRepository<Post, long> _repositoryPost;

        public UserCommentAppService(
           IRepository<Post, long> repositoryPost,
           IRepository<UserComments, long> repositoryComment,
            IHubContext<CommentHub> hubContext,
           IRepository<User, long> repositoryUser)

        {
            _repositoryUser = repositoryUser;
            _repositoryPost = repositoryPost;
            _repositoryComment = repositoryComment;
            _hubContext = hubContext;
        }
        #region Thêm comment
        [AbpAllowAnonymous]
        public async Task<UserCommentDto> AddComment(long postId, UserCommentDto input)
        {
            var userId = AbpSession.UserId;
            var tenanId = AbpSession.TenantId;
            input.PostId = postId;
            input.TenantId = tenanId;
            input.UserId = (long)userId;

            // Tiến hành lưu bình luận vào cơ sở dữ liệu
            var comment = ObjectMapper.Map<UserComments>(input);
            await _repositoryComment.InsertAsync(comment);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveComment", input);

            return input;
        }
        #endregion

        #region Lấy thông báo bình luận đối với chủ trọ 
        [AbpAllowAnonymous]
        public async Task<List<UserCommentViewDto>> GetCommentForRent()
        {
            // sẽ lọc ra những bài đăng chưa được đọc bình luận và có trạng thái DataRead == null
            var userId = AbpSession.UserId;
            var data = await (from c in _repositoryComment.GetAll()
                              join u in _repositoryUser.GetAll() on c.UserId equals u.Id
                              join p in _repositoryPost.GetAll() on c.PostId equals p.Id
                              where p.CreatorUserId == userId && p.CreatorUserId != c.CreatorUserId 
                              orderby c.Id descending
                              select new UserCommentViewDto
                              {
                                  Id = c.Id,
                                  TenantId = c.TenantId,
                                  PostId = p.Id,
                                  UserId = u.Id,
                                  CommentContent = c.CommentContent,
                                  CreateByName = u.FullName,
                                  CreationTime = c.CreationTime,
                              }).ToListAsync();
            // Gửi danh sách bình luận về client thông qua SignalR Hub
            await _hubContext.Clients.All.SendAsync("GetCommentForRent", data);
            return data;

        }


        #endregion

        #region Chỉnh sửa comment
        [AbpAllowAnonymous]
        public async Task<UserCommentDto> Update(UserCommentDto input)
        {
            var userId = AbpSession.UserId;
            var tenanId = AbpSession.TenantId;
            var comment = await _repositoryComment.FirstOrDefaultAsync(c => c.Id == (long)input.Id && c.UserId == userId);
            if (comment == null)
            {
                throw new UserFriendlyException("Không tìm thấy bình luận để sửa");
            }

            comment.CommentContent = input.CommentContent; 

            // Tiến hành cập nhật thông tin bình luận vào cơ sở dữ liệu
            await _repositoryComment.UpdateAsync(comment);
            await CurrentUnitOfWork.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UpdateComment", input);

            return input;
        }
        #endregion

        #region Xóa comment
        [AbpAllowAnonymous]
        public async Task DeleteComment(EntityDto<long> input)
        {
            var userId = AbpSession.UserId;
            var tenanId = AbpSession.TenantId;

            var data = await _repositoryComment.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == input.Id && c.TenantId == tenanId);
            if (data != null)
            {

                await _repositoryComment.DeleteAsync(data);
                await _hubContext.Clients.All.SendAsync("DeleteComment", input.Id);
            }
            else
            {
                throw new UserFriendlyException("Không tìm thấy bình luận để xóa");
            }
        }
        #endregion

        #region Lấy toàn bộ comment đối với bài đăng đó 

        [AbpAllowAnonymous]
        public async Task<List<UserCommentViewDto>> GetAllComment(long postId)
        {
            var tenanId = AbpSession.TenantId;
            var data = await (from c in _repositoryComment.GetAll()
                              join u in _repositoryUser.GetAll() on c.UserId equals u.Id
                              join p in _repositoryPost.GetAll() on c.PostId equals p.Id
                              where c.PostId == postId && c.TenantId == tenanId

                              orderby c.Id descending
                              select new UserCommentViewDto
                              {
                                  Id = c.Id,
                                  TenantId = c.TenantId,
                                  PostId = p.Id,
                                  UserId = u.Id,
                                  CommentContent = c.CommentContent,
                                  CreateByName = u.FullName,
                                  CreationTime = c.CreationTime,
                                  // Các trường thông tin người dùng khác cũng có thể được thêm vào nếu cần
                              }).ToListAsync();
            // Gửi danh sách bình luận về client thông qua SignalR Hub
            await _hubContext.Clients.All.SendAsync("ReceiveAllComments", data);

            return data;
        }
        #endregion

        #region Lấy ra những bình luận mà user đã bình luận vào cái bài đăng đó đối với những tk chủ trọ
        [AbpAllowAnonymous] 
        public async Task<List<UserCommentViewDto>> GetAllCommentNoReadHost(long userID)
        {
            var userId = AbpSession.UserId;
            //var data = await (from c in _repositoryComment.GetAll()
            //                  join u in _repositoryUser.GetAll() on c.UserId equals u.Id
            //                  join p in _repositoryPost.GetAll() on c.PostId equals p.Id
            //                  where c.UserId == userID && c.DataRead == null
            //                  orderby c.Id descending
            //                  select new UserCommentViewDto
            //                  {
            //                      Id = c.Id,
            //                      TenantId = c.TenantId,
            //                      PostId = p.Id,
            //                      UserId = u.Id,
            //                      CommentContent = c.CommentContent,
            //                      CreateByName = u.FullName,
            //                      CreationTime = c.CreationTime,
            //                  }).ToListAsync();

            var data = await (from c in _repositoryComment.GetAll()
                              join u in _repositoryUser.GetAll() on c.UserId equals u.Id
                              join p in _repositoryPost.GetAll() on c.PostId equals p.Id
                              where (from innerPost in _repositoryPost.GetAll()
                               where innerPost.Id == c.PostId && innerPost.CreatorUserId == userId
                               select innerPost.Id).Any() && c.CreatorUserId != userId && c.DataRead == null
                               orderby c.Id descending
                        select new UserCommentViewDto
                        {
                            Id = c.Id,
                            TenantId = c.TenantId,
                            PostId = p.Id,
                            UserId = u.Id,
                            CommentContent = c.CommentContent,
                            CreateByName = u.FullName,
                            CreationTime = c.CreationTime,
                        }).ToListAsync();
            // Gửi danh sách bình luận về client thông qua SignalR Hub
            await _hubContext.Clients.All.SendAsync("ReceiveAllComments", data);

            return data;

        }

        #endregion

        #region Lấy ra những bình luận mà user đã bình luận vào cái bài đăng đó đối với những người thuê trọ
        [AbpAllowAnonymous]
        public async Task<List<UserCommentViewDto>> GetAllCommentNoReadRenter(long userID)
        {
            var userId = AbpSession.TenantId;
            // lấy ra những bài mà người thuê trọ bình luận và đợi phản hồi của cá những bình luận khác về bài mà mình đã đăng
            var data = await (from c in _repositoryComment.GetAll()
                              join u in _repositoryUser.GetAll() on c.UserId equals u.Id
                              join p in _repositoryPost.GetAll() on c.PostId equals p.Id
                              where c.UserId == userID && c.DataRead == null
                              orderby c.Id descending
                              select new UserCommentViewDto
                              {
                                  Id = c.Id,
                                  TenantId = c.TenantId,
                                  PostId = p.Id,
                                  UserId = u.Id,
                                  CommentContent = c.CommentContent,
                                  CreateByName = u.FullName,
                                  CreationTime = c.CreationTime,
                              }).ToListAsync();

            // Gửi danh sách bình luận về client thông qua SignalR Hub
            await _hubContext.Clients.All.SendAsync("ReceiveAllComments", data);

            return data;

        }

        #endregion

        #region Lấy tổng số lượng bình luận

        [AbpAllowAnonymous]

        public async Task<int> GetTotalComment(long postId)
        {
            var tenantId = AbpSession.TenantId;

            // Lấy tổng số lượng bình luận của bài đăng
            var totalComment = await _repositoryComment.GetAll()
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.PostId == postId)
                .CountAsync();

            await _hubContext.Clients.All.SendAsync("GetTotalComments", totalComment);

            return totalComment;

        }
        #endregion
        #region Lấy bình luận theo Id

        [AbpAllowAnonymous]
        public async Task<UserCommentDto> GetCommentById(long id)
        {
            var comment = await _repositoryComment
                .FirstOrDefaultAsync(c => c.Id == id);

            return ObjectMapper.Map<UserCommentDto>(comment);
        }
        #endregion

        #region Tạo phản hồi bình luận 
        public async Task<UserCommentDto> AddReply(long parentCommentId, UserCommentDto input)
        {
            var userId = AbpSession.UserId;
            var tenantId = AbpSession.TenantId;

            // Kiểm tra bình luận cha có tồn tại không
            var parentComment = await _repositoryComment.FirstOrDefaultAsync(c => c.Id == parentCommentId);
            if (parentComment == null)
            {
                throw new UserFriendlyException("Bình luận cha không tồn tại");
            }

            // Tạo bình luận mới và thiết lập các thuộc tính
            var replyComment = ObjectMapper.Map<UserComments>(input);
            replyComment.ParentCommentId = parentCommentId;
            replyComment.PostId = parentComment.PostId; // Đảm bảo bình luận con thuộc cùng bài đăng
            replyComment.TenantId = tenantId;
            replyComment.UserId = (long)userId;

            await _repositoryComment.InsertAsync(replyComment);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Gửi bình luận con đến client
            await _hubContext.Clients.All.SendAsync("ReceiveReply", replyComment);

            return ObjectMapper.Map<UserCommentDto>(replyComment);
        }
        #endregion

        #region Chỉnh sửa bình luận phản hồi 
        public async Task<UserCommentDto> UpdateReply(UserCommentDto input)
        {
            var userId = AbpSession.UserId;

            // Lấy bình luận cần chỉnh sửa
            var comment = await _repositoryComment.FirstOrDefaultAsync(c => c.Id == input.Id && c.UserId == userId);
            if (comment == null)
            {
                throw new UserFriendlyException("Không tìm thấy bình luận để sửa");
            }

            comment.CommentContent = input.CommentContent;

            await _repositoryComment.UpdateAsync(comment);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Gửi bình luận đã chỉnh sửa đến client
            await _hubContext.Clients.All.SendAsync("UpdateReply", comment);

            return ObjectMapper.Map<UserCommentDto>(comment);
        }
        #endregion

        #region Xóa bình luận 
        public async Task DeleteReply(long commentId)
        {
            var userId = AbpSession.UserId;

            // Lấy bình luận cần xóa
            var comment = await _repositoryComment.FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
            if (comment == null)
            {
                throw new UserFriendlyException("Không tìm thấy bình luận để xóa");
            }

            await _repositoryComment.DeleteAsync(comment);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Gửi thông báo xóa bình luận đến client
            await _hubContext.Clients.All.SendAsync("DeleteReply", commentId);
        }

        #endregion

        #region Lấy danh sách bình luận 
        public async Task<List<UserCommentDto>> GetReplies(long parentCommentId)
        {
            var tenantId = AbpSession.TenantId;

            var replies = await (from c in _repositoryComment.GetAll()
                                 join u in _repositoryUser.GetAll() on c.UserId equals u.Id
                                 where c.ParentCommentId == parentCommentId && c.TenantId == tenantId
                                 orderby c.Id descending
                                 select new UserCommentDto
                                 {
                                     Id = c.Id,
                                     TenantId = c.TenantId,
                                     PostId = c.PostId,
                                     UserId = u.Id,
                                     CommentContent = c.CommentContent,
                                     ParentCommentId = c.ParentCommentId
                                 }).ToListAsync();

            return replies;
        }

        #endregion
    }
}
