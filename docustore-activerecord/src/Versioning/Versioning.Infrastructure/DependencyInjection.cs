using Versioning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Versioning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVersioningInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VersioningDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(VersioningDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddTransient<DbContext>(sp => sp.GetRequiredService<VersioningDbContext>());


        return services;
    }
}