using System;
using System.ComponentModel.DataAnnotations;

namespace Contract.Identity.UserManager
{
    public class UserModel
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? LoginProvider { get; set; }
        public string? DeviceId { get; set; }
        public string? Token { get; set; }

    }
}