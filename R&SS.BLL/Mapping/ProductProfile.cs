using AutoMapper;
using R_SS.BLL.DTOs.Product;
using R_SS.Models.Entities;

namespace R_SS.BLL.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(destination => destination.CategoryName, options => options.MapFrom(source => source.ProductCategory == null ? null : source.ProductCategory.CategoryName))
            .ForMember(destination => destination.BrandName, options => options.MapFrom(source => source.Supplier == null ? null : source.Supplier.SupplierName));

        CreateMap<Product, ProductDetailResponse>()
            .ForMember(destination => destination.CategoryName, options => options.MapFrom(source => source.ProductCategory == null ? null : source.ProductCategory.CategoryName))
            .ForMember(destination => destination.BrandName, options => options.MapFrom(source => source.Supplier == null ? null : source.Supplier.SupplierName))
            .ForMember(destination => destination.Message, options => options.Ignore());
    }
}
