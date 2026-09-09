using System.Net;

namespace Contract
{
    public class ApiResponseBase<T>
    {
        public string Message { get; set; }
        public bool IsSuccess
        {
            get { return string.IsNullOrEmpty(Message); }
        }
        public T Data { get; set; }
        public int? TotalItems { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    //public class ApiIdentityResponse : ApiResponseBase
    //{
    //    public string RefreshToken { get; set; }
    //    public string AccessToken { get; set;}
    //}
}
