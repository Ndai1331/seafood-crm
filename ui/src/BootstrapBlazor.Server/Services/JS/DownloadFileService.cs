using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Services
{
    public class DownloadFileService
    {
        private readonly IJSRuntime _jsInterop;

        public DownloadFileService(IJSRuntime jsInterop)
        {
            _jsInterop = jsInterop;
        }

        public async Task DownloadFileAsync(Byte[] bytes,string extension,string fileName)
        {
            // Gọi helper tải file dùng chung trong wwwroot/js/clipboard-copy.js. Trước đây gọi
            // "saveAsFile" — hàm đó KHÔNG tồn tại ở bất kỳ file JS nào trong repo, nên mọi nút
            // xuất file đi qua service này đều ném lỗi interop ngay khi bấm.
            var mime = string.Equals(extension, "xlsx", StringComparison.OrdinalIgnoreCase)
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : null;
            await _jsInterop.InvokeVoidAsync(
                "downloadBase64File",
                $"{fileName}-{DateTime.Now:yyyyMMddHHmmss}.{extension}",
                Convert.ToBase64String(bytes),
                mime);
        }
    }
}
