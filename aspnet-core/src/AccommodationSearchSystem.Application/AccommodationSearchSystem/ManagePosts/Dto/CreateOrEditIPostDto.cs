﻿using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.AccommodationSearchSystem.ManagePosts.Dto
{
    public class CreateOrEditIPostDto : EntityDto<long?>
    {
        public string PostCode { get; set; }
        public int? TenantId { get; set; }
        [StringLength(4000)]
        public string Title { get; set; }
        [StringLength(4000)]
        public string ContentPost { get; set; }
        public string Photo { get; set; }
        public decimal RoomPrice { get; set; } // Giá phòng
        public string Address { get; set; }
        [StringLength(4000)]
        public string District { get; set; }
        [StringLength(4000)]
        public string City { get; set; }
        [StringLength(4000)]
        public string Ward { get; set; }
        [StringLength(4000)]
        public string Area { get; set; }
        public decimal Square { get; set; }
        public bool RoomStatus { get; set; }
        public string PriceCategory { get; set; } // Loại phòng
        public bool Wifi { get; set; } // Có wifi hay không
        public bool Parking { get; set; } // Có chỗ để xe hay không
        public bool Conditioner { get; set; } // Có điều hóa hay không
        public ICollection<PhotoDto> Photos { get; set; }
        public bool ConfirmAdmin { get; set; }
        public bool IsShowCancel { get; set; }

    }
}