using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Versioning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVersioningApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}