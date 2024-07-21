using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto
{
    public class NotificationDto : Entity<long?>
    {
        public string NotificationName { get; set; }

        public bool IsSending { get; set; }  //đã đọc hay chưa

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int? TenantId { get; set; }
        public int PostId { get; set; }

        public int? CreatorUserId { get; set; }
    }
}
