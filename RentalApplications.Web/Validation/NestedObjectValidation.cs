using System.ComponentModel.DataAnnotations;

namespace RentalApplications.Validation;

public static class NestedObjectValidation
{
    public static IEnumerable<ValidationResult> ValidateObject(
        object instance,
        string propertyPrefix)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);

        foreach (var result in results)
        {
            var members = result.MemberNames.Any()
                ? result.MemberNames.Select(m => $"{propertyPrefix}.{m}").ToArray()
                : [propertyPrefix];

            yield return new ValidationResult(result.ErrorMessage, members);
        }
    }
}
