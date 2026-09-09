using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace BootstrapBlazor.Server.Controllers;

/// <summary>Internal endpoint for FAQ search — called from ChatWidget AI context</summary>
[ApiController]
[Route("api/internal/chat/faq")]
public class ChatFaqController : ControllerBase
{
    private readonly ILogger<ChatFaqController> _logger;

    private static readonly List<FaqEntry> _faqs =
    [
        new("Cách import Excel CPD", "Vào menu CPD → Import → chọn file Excel → kiểm tra preview → bấm Confirm."),
        new("Quy trình duyệt chi phí", "IC tạo yêu cầu → Assistant duyệt → Manager phê duyệt → HEAD xác nhận. Xem chi tiết tại trang Payment Workflow."),
        new("Cách thêm keyword mới", "Vào SEO Management → Keywords → Add New → điền thông tin → lưu. Keyword sẽ xuất hiện trong báo cáo ngày hôm sau."),
        new("Geo Block Checker là gì", "Công cụ kiểm tra domain có bị block theo quốc gia không, tích hợp Cloudflare detection."),
        new("Cách xuất báo cáo Excel", "Mỗi bảng dữ liệu có nút Export ở góc trên bên phải. Nhấn để tải file Excel."),
        new("Hướng dẫn sử dụng hệ thống", "Task9.pro là hệ thống quản lý SEO nội bộ. Dùng menu bên trái để điều hướng giữa các module: CPD, Keywords, Domains, Reports."),
        new("CPD là gì", "CPD (Chi Phí Dịch vụ) là module quản lý chi phí SEO hàng tháng. Mỗi IC nhập chi phí và team leader duyệt."),
        new("Cách reset mật khẩu", "Liên hệ admin hoặc dùng nút 'Đổi mật khẩu' trong menu profile góc trên bên phải."),
        new("Báo cáo SEO là gì", "Tổng hợp ranking keyword, traffic domain theo tháng. Vào mục Reports → SEO để xem."),
        new("Domain Check Daily là gì", "Công cụ kiểm tra trạng thái domain hàng ngày: HTTP status, redirect, index Google."),
    ];

    public ChatFaqController(ILogger<ChatFaqController> logger) => _logger = logger;

    [HttpGet("search")]
    [AllowAnonymous]
    public IActionResult Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new { results = Array.Empty<object>() });

        var query = q.ToLowerInvariant();
        var results = _faqs
            .Where(f => f.Question.ToLowerInvariant().Contains(query)
                     || f.Answer.ToLowerInvariant().Contains(query))
            .Take(3)
            .Select(f => new { f.Question, f.Answer })
            .ToList();

        _logger.LogInformation("FAQ search: '{Query}' → {Count} results", q, results.Count);
        return Ok(new { results });
    }
}

public record FaqEntry(string Question, string Answer);
