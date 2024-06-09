﻿using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.Email.Dto
{
    public class EmailModel  : EntityDto<long?>
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public EmailModel( string to , string subject, string content)
        {
            To = to;
            Subject = subject;
            Content = content;
        }
    }
}
