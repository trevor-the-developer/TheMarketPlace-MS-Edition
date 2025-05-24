using System.ComponentModel.DataAnnotations;

namespace Services.Core.Attributes;

public class PageGreaterThanZeroAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int page && page < 1)
        {
            return new ValidationResult("Page number must be greater than 0.");
        }

        return ValidationResult.Success!;
    }
}
