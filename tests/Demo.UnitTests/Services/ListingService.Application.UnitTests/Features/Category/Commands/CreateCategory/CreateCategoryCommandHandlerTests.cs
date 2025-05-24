using AutoMapper;
using Moq;
using Shouldly;
using Xunit;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Commands.CreateCategory;
using ListingService.Application.Features.Category.Shared;
using ListingService.Application.MappingProfiles;
using ListingService.Domain.Entities;

namespace ListingService.Application.UnitTests.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        
        var mapperConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<CategoryProfile>();
        });
        
        _mapper = mapperConfig.CreateMapper();
    }
    
    [Fact]
    public async Task Handle_ValidCategory_ReturnsSuccessfulCreation()
    {
        // Arrange
        var handler = new CreateCategoryCommandHandler(_mockCategoryRepository.Object, _mapper);
        
        var command = new CreateCategoryCommand
        {
            Name = "Test Category",
            Description = "Test Description"
        };
        
        var newCategory = new Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockCategoryRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<CategoryDto>();
        result.Name.ShouldBe(command.Name);
        result.Description.ShouldBe(command.Description);
    }
    
    [Fact]
    public async Task Handle_WithParentCategory_SetsParentCategoryName()
    {
        // Arrange
        var handler = new CreateCategoryCommandHandler(_mockCategoryRepository.Object, _mapper);
        
        var parentId = Guid.NewGuid();
        var parentCategory = new Domain.Entities.Category
        {
            Id = parentId,
            Name = "Parent Category",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var command = new CreateCategoryCommand
        {
            Name = "Child Category",
            Description = "Child Description",
            ParentCategoryId = parentId
        };
        
        var newCategory = new Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            ParentCategoryId = command.ParentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _mockCategoryRepository.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);
            
        _mockCategoryRepository.Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentCategory);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.ShouldNotBeNull();
        result.ParentCategoryId.ShouldBe(parentId);
        result.ParentCategoryName.ShouldBe(parentCategory.Name);
    }
}