using System.Net;

namespace Core.Exceptions
{
    public class GlobalException : System.Exception
    {
        public HttpStatusCode Status = HttpStatusCode.OK;
        
        public GlobalException(string message,HttpStatusCode status) : base(message)
        {
            Status = status;
        }
    }
}