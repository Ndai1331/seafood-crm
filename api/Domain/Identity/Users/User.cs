using Core.Enum;
using Domain.AppHistories;
using Domain.Positions;
using Domain.Teams;
using Domain.UserDepartments;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Identity.Users
{
    public class User : IdentityUser<int>
    {
        public string UserCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        public UserType UserType { get; set; } = UserType.Inhouse;
        public DateTime DOB { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsDelete { get; set; }
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Keeps the password door open for this one account while the organisation-wide switch is
        /// off. Default false, so the switch keeps meaning what it says; an admin grants it per
        /// person, which is how somebody without a key signs in once and registers one.
        /// </summary>
        public bool PasswordLoginAllowed { get; set; }
        public string? TotpSecretKey { get; set; }
        public bool IsTotpEnabled { get; set; } = false;
        public int TotpFailedCount { get; set; } = 0;
        public DateTime? TotpLockoutEnd { get; set; }
        public string? AvatarURL { get; set; }
        public string? Address { get; set; }
        public int? DependsId { get; set; }
        public User? UserDepend { get; set; }
        public int? Relationship { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public List<AppHistory> AppHistoryUsers { get; set; }


    public int? PositionId { get; set; }
    [ForeignKey("PositionId")]
    public Position Position { get; set; }

    public int? TeamId { get; set; }
    [ForeignKey("TeamId")]
    public Team Team { get; set; }


    [Column("user_name_normalized")]
    public string UserNameNormalized { get; set; } = string.Empty;

    /// <summary>Display name on Google Sheets SEO sync (e.g. "SEO X Miig").</summary>
    [Column("alias_name_ggsheet")]
    [StringLength(255)]
    public string? AliasNameGgsheet { get; set; }

    public List<UserDepartment> UserDepartments { get; set; }
    }
}
