using System.Text;
using Core.Enum;

namespace Contract.Identity.UserManager
{
    public class UserDto
    {
        public int Id { get; set; }
        public int Count { get; set; } = 0;
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public UserType UserType { get; set; }
        public DateTime DOB { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string AvatarURL { get; set; }
        public string? SignImgageUrl { get; set; }

        public string? SignKey { get; set; }
        public string? SignSecrect { get; set; }
        public int? BloodTypeId { set; get; }
        public string? CitizenIDNumber { get; set; }
        public int? CountryId { get; set; }
        public string? CountryName { get; set; }
        public int? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public int? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public int? WardId { get; set; }
        public string? WardName { get; set; }
        public string? Address { get; set; }
    public int? DependsId { get; set; }
    public int? Relationship { get; set; }

    public int? TeamId { get; set; }
    public string? TeamCode { get; set; }
    public string? TeamName { get; set; }

    public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ODX { get; set; }

        //declare examination results
        public string? DOBString { get; set; }
    }
}
