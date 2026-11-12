using System.Text.Json;
using FluentValidation.Results;
using Shared.Kernel;

namespace Shared.Core.Validation.ValidationExtensions;

public static class ValidationExtensions
{
    public static Errors ToError(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors;

        var errors = validationErrors.Select(
            validationError => JsonSerializer.Deserialize<Error>(validationError.ErrorMessage)!);

        return new Errors(errors.ToList());
    }
}