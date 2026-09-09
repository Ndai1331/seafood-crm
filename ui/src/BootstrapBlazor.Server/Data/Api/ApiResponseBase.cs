using System.Net;

namespace BootstrapBlazor.Server.Data;

public class ApiResponseBase<T>
{
    public string Message { get; set; }
    /// <summary>Explicit success flag for API responses</summary>
    public bool Success { get; set; }
    public bool IsSuccess
    {
        get { return string.IsNullOrEmpty(Message); }
    }
    public T Data { get; set; }
    public int? TotalItems { get; set; } = 0;
    public  int? Total { get; set; } = 0;

    public HttpStatusCode StatusCode { get; set; }
}
