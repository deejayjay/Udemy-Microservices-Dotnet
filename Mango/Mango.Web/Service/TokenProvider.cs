using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class TokenProvider(IHttpContextAccessor contextAccessor) : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            if (_contextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out var token) is true)
                return token;
            
            return null;
        }

        public void SetToken(string token)
        {
            _contextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}
