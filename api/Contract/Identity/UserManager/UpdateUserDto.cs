using Contract.CustomAttribute;
using Core.Const;
using Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace Contract.Identity.UserManager
{
    public class UpdateUserDto
    {
        public string UserName { get; set; }
        
        
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }
        
        
        [Required(ErrorMessage = "User Code is required")]
        public string UserCode { get; set;}
        


        public Gender Gender { get; set; }
        public UserType UserType { get; set; }
        public DateTime DOB { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; }

        public string PhoneNumber { get; set; } = "0000000000";
        
        public bool IsSetPassword { get; set; } = false;
        
        [RequiredIf(nameof(IsSetPassword),true)]
        [MinLength(6)]
        public string Password { get; set; }

        [RequiredIf(nameof(IsSetPassword),true)]
        [Compare(nameof(Password),ErrorMessage = "Password confirm do not match")]
        public string PasswordConfirm { get; set; }

        
        [RegularExpression(ContentRegularExpression.EMAIL,ErrorMessage = "Email has to @gmail.com format")]
        public string? Email { get; set; }
        
    public List<string> Roles { get; set; } = new List<string>();
    
    public int? PositionId { get; set; }
    public int? TeamId { get; set; }
    public string? AvatarURL { get; set; }

        public List<int> DepartmentIds { get; set; } = new List<int>();

        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public int? BloodTypeId { set; get; }
        public string? CitizenIDNumber { get; set; }
        public string? Address { get; set; }
        public int? DependsId { get; set; }
        public int? Relationship { get; set; }
        public int ODX { get; set; } = 0;

    }
}