using System.ComponentModel.DataAnnotations;

namespace AspPlayground.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class ManufacturingYearRangeAttribute : ValidationAttribute
{
    private readonly int _minYear;

    public ManufacturingYearRangeAttribute(int minYear = 1886)
    {
        _minYear = minYear;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success; // [Required] handles null
        }

        if (value is not int year)
        {
            return new ValidationResult("Invalid year value.");
        }

        int maxYear = DateTime.UtcNow.Year + 1;

        if (year < _minYear || year > maxYear)
        {
            return new ValidationResult(
                $"{validationContext.MemberName} must be between {_minYear} and {maxYear}."
            );
        }

        return ValidationResult.Success;
    }
}