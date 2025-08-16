using System.ComponentModel.DataAnnotations;

namespace ChurchAttendanceSystem.Application.Validation;

public static class ValidationExtensions
{
    public static bool IsValid<T>(this T model, out List<string> errors)
    {
        errors = new List<string>();
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(model!, context, results, true);
        
        if (!isValid)
        {
            errors.AddRange(results.Select(r => r.ErrorMessage ?? "Validation error"));
        }
        
        return isValid;
    }
}