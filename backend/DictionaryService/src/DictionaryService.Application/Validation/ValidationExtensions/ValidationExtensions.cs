using System.Text.Json;
using Shared.Kernel;
using FluentValidation.Results;

namespace DictionaryService.Application.Validation.ValidationExtensions;

public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult validationResult)
    {
        List<ValidationFailure> validationErrors = validationResult.Errors;

        var errors = validationErrors.Select(validationError => validationError.ErrorMessage);

        return Error.Validation(null, errors.ToArray());
    }
}