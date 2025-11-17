using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tagging.Domain.Common;
using Tagging.Infrastructure.Data;

namespace Tagging.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaggingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TaggingDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(TaggingDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        // factory specific pentru Tagging
        services.AddSingleton<TaggingDbContextFactory>(sp =>
        {
            return () =>
            {
                var scope = Tagging.Domain.Common.ServiceLocator.CreateScope();
                return scope.ServiceProvider.GetRequiredService<TaggingDbContext>();
            };
        });

        return services;
    }

    public static void InitializeTaggingDbContextProvider(IServiceProvider serviceProvider)
    {
        var contextFactory = serviceProvider.GetRequiredService<TaggingDbContextFactory>();
        TaggingDbContextProvider.Initialize(contextFactory);
    }
}