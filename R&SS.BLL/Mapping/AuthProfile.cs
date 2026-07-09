using AutoMapper;
using R_SS.BLL.DTOs.Authentication;
using R_SS.Models.Entities;

namespace R_SS.BLL.Mapping;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, RegisterResponse>();

        CreateMap<User, LoginResponse>()
            .ForMember(destination => destination.RoleName, options => options.Ignore())
            .ForMember(destination => destination.AccessToken, options => options.Ignore())
            .ForMember(destination => destination.ExpiresAtUtc, options => options.Ignore())
            .ForMember(destination => destination.Message, options => options.Ignore());
    }
}
