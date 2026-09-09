using System.ComponentModel.DataAnnotations;
using Core.Const;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class CreateUserDto
    {
        public string UserName { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [MinLength(6)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Compare(nameof(Password))]
        [Required(ErrorMessage = "Password is required")]
        public string PasswordConfirm { get; set; }

        [Required(ErrorMessage = "User Code is required")]
        [MinLength(2, ErrorMessage = "User Code is at least 2 character")]
        public string UserCode { get; set; }

        public Gender Gender { get; set; }
        public UserType UserType { get; set; } = UserType.Inhouse;
        public DateTime DOB { get; set; } = DateTime.Now;
        public string PhoneNumber { get; set; } = "0000000000";
        [RegularExpression(
            ContentRegularExpression.EMAIL,
            ErrorMessage = "Email has to @gmail.com format"
        )]
        public string? Email { get; set; }

        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

    public List<string> Roles { get; set; } = new List<string>();

    public int? PositionId { get; set; }
    public int? TeamId { get; set; }
    public string? AvatarURL { get; set; }
        public List<int> DepartmentIds { get; set; } = new List<int>();
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public string? CitizenIDNumber { get; set; }
        public string? Address { get; set; }
        public int? DependsId { get; set; }
        public int? Relationship { get; set; }
        public int ODX { get; set; }

    }
}
