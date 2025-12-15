using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace VendingMachines.API.Tests.Helpers;

public static class ControllerValidationExtensions
{
    public static void ValidateModel<T>(this ControllerBase controller, T model) where T : class
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
        {
            foreach (var result in validationResults)
            {
                foreach (var memberName in result.MemberNames)
                {
                    controller.ModelState.AddModelError(memberName, result.ErrorMessage ?? "Validation error");
                }
            }
        }
    }
}