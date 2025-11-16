using Microsoft.Extensions.DependencyInjection;

namespace MetadataIndexing.Domain.Common;

public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public static T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException(
                "ServiceLocator has not been initialized. Call ServiceLocator.Initialize() during application startup.");
        }

        var service = _serviceProvider.GetService(typeof(T));
        if (service == null)
        {
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        return (T)service;
    }

    public static IServiceScope CreateScope()
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException(
                "ServiceLocator has not been initialized. Call ServiceLocator.Initialize() during application startup.");
        }

        return _serviceProvider.CreateScope();
    }
}