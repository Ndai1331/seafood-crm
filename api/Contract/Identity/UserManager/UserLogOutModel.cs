using System.ComponentModel.DataAnnotations;

namespace Contract.Identity.UserManager
{
    public class UserLogOutModel
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "DeviceId is required")]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class WebisteUserLogOutModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
    }
}
