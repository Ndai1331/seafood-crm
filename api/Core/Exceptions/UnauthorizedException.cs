using System.Net;

namespace Core.Exceptions
{
    public class UnauthorizedException : System.Exception
    {
        public HttpStatusCode Status = HttpStatusCode.OK;
        
        public UnauthorizedException(string message,HttpStatusCode status) : base(message)
        {
            Status = status;
        }
    }
}