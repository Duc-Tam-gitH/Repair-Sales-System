using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R_SS.BLL.DTOs.Authentication;

namespace R_SS.BLL.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration data.</param>
    /// <returns>The registration result.</returns>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user account using email or username plus password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>The login result, including role and token.</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
