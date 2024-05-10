using SubscribeProvider.Models;
using System.ComponentModel.DataAnnotations;

namespace SubscribeProvider.Helpers.Validations;

public class CustomValidation
{
    public static ValidationModel<T> ValidateModel<T>(T model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model!);
        var isValid = Validator.TryValidateObject(model!, context, validationResults, true);

        return new ValidationModel<T>
        {
            IsValid = isValid,
            Value = model,
            ValidationResults = validationResults
        };
    }
}