
using MetadataIndexing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MetadataIndexing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMetadataIndexingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<MetadataIndexingDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(MetadataIndexingDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddTransient<DbContext>(sp => sp.GetRequiredService<MetadataIndexingDbContext>());


        return services;
    }
}