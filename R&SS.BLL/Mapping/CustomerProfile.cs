using AutoMapper;
using R_SS.BLL.DTOs.Customer;
using R_SS.Models.Entities;

namespace R_SS.BLL.Mapping;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerResponse>();
    }
}
