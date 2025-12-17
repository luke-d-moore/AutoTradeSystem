using System;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RequiredEitherNotBothAttribute : ValidationAttribute
{
    private readonly string _propertyAName;
    private readonly string _propertyBName;

    public RequiredEitherNotBothAttribute(string propertyAName, string propertyBName)
    {
        _propertyAName = propertyAName;
        _propertyBName = propertyBName;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var propertyAInfo = validationContext.ObjectType.GetProperty(_propertyAName);
        var propertyBInfo = validationContext.ObjectType.GetProperty(_propertyBName);

        if (propertyAInfo == null || propertyBInfo == null)
        {
            return new ValidationResult($"Unknown property name(s) supplied to validator.");
        }

        var valueA = propertyAInfo.GetValue(value) as decimal?;
        var valueB = propertyBInfo.GetValue(value) as decimal?;

        bool aIsValid = valueA.HasValue && valueA.Value > 0;
        bool bIsValid = valueB.HasValue && valueB.Value > 0;

        if (aIsValid ^ bIsValid)
        {
            return ValidationResult.Success;
        }
        else
        {
            return new ValidationResult(
                ErrorMessage ?? $"Either '{_propertyAName}' or '{_propertyBName}' must be supplied as greater than 0, but not both simultaneously.",
                [_propertyAName, _propertyBName] 
            );
        }
    }
}
