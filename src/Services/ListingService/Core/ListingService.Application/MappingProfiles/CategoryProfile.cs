using AutoMapper;
using ListingService.Application.Features.Category.Shared;
using ListingService.Domain.Entities;

namespace ListingService.Application.MappingProfiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories != null ? src.SubCategories : new List<Category>()));
    }
}