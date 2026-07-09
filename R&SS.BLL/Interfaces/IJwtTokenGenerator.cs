using R_SS.BLL.Helpers;
using R_SS.Models.Entities;

namespace R_SS.BLL.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>
    /// Creates a signed JWT for the specified user and role.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="roleName">The user's active role name.</param>
    /// <returns>The generated token and its expiration timestamp.</returns>
    JwtTokenResult GenerateToken(User user, string roleName);
}
