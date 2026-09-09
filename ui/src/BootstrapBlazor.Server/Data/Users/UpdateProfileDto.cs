using System.ComponentModel.DataAnnotations;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// DTO cho người dùng tự cập nhật thông tin cá nhân.
/// Chỉ chứa các trường AN TOÀN — không có Roles, UserCode, UserType, IsActive, etc.
/// </summary>
public class UpdateProfileDto
{
    [Required(ErrorMessage = "Họ là bắt buộc")]
    [MinLength(1, ErrorMessage = "Họ phải có ít nhất 1 ký tự")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Tên là bắt buộc")]
    [MinLength(1, ErrorMessage = "Tên phải có ít nhất 1 ký tự")]
    public string LastName { get; set; } = "";

    public int Gender { get; set; } // 0: Female, 1: Male, 2: Unknown

    public DateTime DOB { get; set; }

    public string PhoneNumber { get; set; } = "";

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? AvatarURL { get; set; }

    // Change password section
    public bool IsSetPassword { get; set; } = false;

    public string? Password { get; set; }

    public string? PasswordConfirm { get; set; }
}
