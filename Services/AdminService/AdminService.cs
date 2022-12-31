using System.Security.Claims;

namespace LOi.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }
    }
}