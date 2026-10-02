using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HomeCare.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ActiveHomeSessionKey = "ActiveHomeId";

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public bool IsAuthenticated => HttpContext?.User.Identity?.IsAuthenticated == true;

        public int? CurrentUserId
        {
            get
            {
                var claim = HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out var id))
                {
                    return id;
                }
                return null;
            }
        }

        public string? CurrentUserName => HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        public string? CurrentUserEmail => HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

        public int? ActiveHomeId
        {
            get
            {
                var sessionVal = HttpContext?.Session.GetInt32(ActiveHomeSessionKey);
                if (sessionVal.HasValue) return sessionVal.Value;

                if (HttpContext?.Request.Cookies.TryGetValue(ActiveHomeSessionKey, out var cookieVal) == true
                    && int.TryParse(cookieVal, out var cookieId))
                {
                    return cookieId;
                }
                return null;
            }
            set
            {
                if (HttpContext == null) return;
                if (value.HasValue)
                {
                    HttpContext.Session.SetInt32(ActiveHomeSessionKey, value.Value);
                    HttpContext.Response.Cookies.Append(ActiveHomeSessionKey, value.Value.ToString(), new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        SameSite = SameSiteMode.Lax
                    });
                }
                else
                {
                    HttpContext.Session.Remove(ActiveHomeSessionKey);
                    HttpContext.Response.Cookies.Delete(ActiveHomeSessionKey);
                }
            }
        }
    }
}
