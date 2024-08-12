using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.ManageAppointmentSchedules.Dto
{
    public class NotificationScheduleNewDto :   Entity<long?>
    {
        public string NotificationScheduleName { get; set; }
        public long ScheduleId { get; set; }
        public DateTime CreationTime { get; set; }
        public long PostId { get; set; }
        public bool IsSending { get; set; }
        public long CreatorUserId { get; set; }
    }
}
