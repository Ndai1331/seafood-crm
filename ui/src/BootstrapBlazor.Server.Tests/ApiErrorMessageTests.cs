using BootstrapBlazor.Server.Services;
using Xunit;

namespace BootstrapBlazor.Server.Tests;

public class ApiErrorMessageTests
{
    [Fact]
    public void Describe_ReadsApiErrorEnvelope()
    {
        var message = ApiErrorMessage.Describe(
            "{\"error\":\"Mã danh mục đã tồn tại trong nhóm này.\",\"stackTrace\":\"\"}",
            "Lỗi mặc định");

        Assert.Equal("Mã danh mục đã tồn tại trong nhóm này.", message);
    }

    [Fact]
    public void Describe_ReadsMessageEnvelope()
    {
        var message = ApiErrorMessage.Describe("{\"message\":\"Dữ liệu không hợp lệ.\"}", "Lỗi mặc định");

        Assert.Equal("Dữ liệu không hợp lệ.", message);
    }

    [Fact]
    public void Describe_UsesFallbackWhenJsonHasNoUsableMessage()
    {
        var message = ApiErrorMessage.Describe("{\"error\":\"\",\"stackTrace\":\"\"}", "Vui lòng thử lại.");

        Assert.Equal("Vui lòng thử lại.", message);
    }
}
