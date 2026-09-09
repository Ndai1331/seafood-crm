using System.ComponentModel.DataAnnotations;

namespace Contract.Identity.UserManager
{
    public class NewUserPasswordDto
    {
        public string UserName { get; set; }

        [Required(ErrorMessage = "NewPassword Is Required")]
        public string? NewPassword { get; set; }

        [Compare(nameof(NewPassword))]
        public string? NewPasswordConfirm { get; set; }
    }
}
