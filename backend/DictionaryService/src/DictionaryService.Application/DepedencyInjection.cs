using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Abstractions;

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