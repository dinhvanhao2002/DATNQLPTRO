using Castle.MicroKernel.Registration;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.Users.Dto
{
    public class UploadProfilePictureOutput 
    {
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string FileToken { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public UploadProfilePictureOutput()
        {

        }

        //public UploadProfilePictureOutput(ErrorInfo error)
        //{
        //    Code = error.Code;
        //    Details = error.Details;
        //    Message = error.Message;
        //    ValidationErrors = error.ValidationErrors;
        //}
    }
}
