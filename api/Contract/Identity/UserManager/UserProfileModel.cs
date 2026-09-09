using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Contract.Identity.UserManager
{
    public class UserProfileModel
    {
        public string Name { get; set; }
        
        // [DataType(DataType.Upload)]
        // [FileExtensions(Extensions = "jpg,png")]
        // public IFormFile File { get; set;}
    }
}