using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace AccommodationSearchSystem.Entity
{
    public class NotificationScheduleNew : FullAuditedEntity<long>, IEntity<long>
    {
        public string NotificationScheduleName { get; set; }
        public bool IsSending { get; set; }  //đã đọc hay chưa
        public int? TenantId { get; set; }
        public int PostId { get; set; }
        public int ScheduleId { get; set; }  //của lịch nào được thêm vào nữa 
    }
}

