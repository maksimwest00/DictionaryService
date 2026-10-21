using System.Reflection;
using Shared.Core.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DictionaryService.Application;

public static class DepedencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(DepedencyInjection).Assembly;

        services.AddHandlers(assembly);

        services.AddValidatorsFromAssembly(typeof(DepedencyInjection).Assembly);

        return services;
    }
}