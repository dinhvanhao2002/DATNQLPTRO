using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.UserComment.Dto
{
    public class UserCommentParentDto : EntityDto<long?>
    {
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long PostId { get; set; }
        public string CommentContent { get; set; }
        public long? ParentCommentId { get; set; }
    }
}
