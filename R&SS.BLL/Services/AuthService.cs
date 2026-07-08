using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R_SS.BLL.DTOs.Authentication;
using R_SS.BLL.Interfaces;
using R_SS.DAL.UnitOfWork;

namespace R_SS.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        throw new NotImplementedException();
    }
}
