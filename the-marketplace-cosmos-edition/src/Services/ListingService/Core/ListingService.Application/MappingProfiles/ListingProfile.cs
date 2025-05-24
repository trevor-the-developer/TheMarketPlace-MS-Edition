using AutoMapper;
using ListingService.Application.Features.Listing.Shared;
using ListingService.Domain.Entities;

namespace ListingService.Application.MappingProfiles;

public class ListingProfile : Profile
{
    public ListingProfile()
    {
        CreateMap<Listing, ListingDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null ? src.Tags : new List<Tag>()))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images : new List<ListingImage>()));
            
        CreateMap<ListingImage, ListingImageDto>();
        CreateMap<Tag, TagDto>();
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories != null ? src.SubCategories : new List<Category>()));
    }
}