namespace BootstrapBlazor.Server.Data.MarketShareSeo;

/// <summary>
/// Hai chế độ của mục nạp Checktop. Chúng dùng chung dialog nhưng KHÁC HẲN nhau về
/// hậu quả, nên phải tách bạch ở kiểu dữ liệu chứ không phải bằng một cờ bool.
/// </summary>
public enum CheckTopUploadMode
{
    /// <summary>
    /// Nạp thêm dòng mới (ADMIN). Dòng đã có bị bỏ qua, không sửa gì. Mặc định.
    /// </summary>
    Append,

    /// <summary>
    /// Ghi đè team/pic cho các dòng đang mang ô gộp "K Na", lấy từ cột N/O của file
    /// (SUPER_ADMIN). Sửa dữ liệu đã chốt — luôn có bản ghi hoàn tác.
    /// </summary>
    FixPic
}
