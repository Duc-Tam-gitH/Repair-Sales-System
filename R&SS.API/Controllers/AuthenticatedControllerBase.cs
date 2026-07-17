using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace R_SS.API.Controllers;

public abstract class AuthenticatedControllerBase : ControllerBase
{
    protected int CurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : 0;
    }

    protected string CurrentRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
