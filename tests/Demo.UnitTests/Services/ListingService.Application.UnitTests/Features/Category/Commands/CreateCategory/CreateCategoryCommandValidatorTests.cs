using Moq;
using Xunit;
using FluentValidation.TestHelper;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Commands.CreateCategory;

namespace ListingService.Application.UnitTests.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    
    public CreateCategoryCommandValidatorTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _validator = new CreateCategoryCommandValidator(_mockCategoryRepository.Object);
    }
    
    [Fact]
    public async Task Should_HaveError_When_NameIsEmpty()
    {
        // Arrange
        var command = new CreateCategoryCommand { Name = "" };
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
    
    [Fact]
    public async Task Should_HaveError_When_NameExceedsMaxLength()
    {
        // Arrange
        var command = new CreateCategoryCommand { Name = new string('a', 101) }; // 101 characters
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
    
    [Fact]
    public async Task Should_HaveError_When_DescriptionExceedsMaxLength()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Valid Name",
            Description = new string('a', 501) // 501 characters
        };
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }
    
    [Fact]
    public async Task Should_HaveError_When_ParentCategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Valid Name",
            ParentCategoryId = Guid.NewGuid()
        };
        
        _mockCategoryRepository.Setup(r => r.ExistsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ParentCategoryId);
    }
    
    [Fact]
    public async Task Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Valid Name",
            Description = "Valid Description"
        };
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public async Task Should_NotHaveError_When_ParentCategoryExists()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var command = new CreateCategoryCommand
        {
            Name = "Valid Name",
            ParentCategoryId = parentId
        };
        
        _mockCategoryRepository.Setup(r => r.ExistsByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}