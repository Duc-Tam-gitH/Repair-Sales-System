using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Helpers;
using R_SS.BLL.Services;

namespace R_SS.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Register Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Register Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }

}
