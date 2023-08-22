using Microsoft.AspNetCore.Authorization;

namespace Srs.Api.Controllers.Auth;

public class AuthorizeAdminAttribute : AuthorizeAttribute
{
    public AuthorizeAdminAttribute()
    {
        Roles = Constants.ADMIN_ROLE_NAME;
    }
}