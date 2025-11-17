using Document.Domain.Common;
using Document.Domain.Services;
using Document.Infrastructure.Data;
using Document.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DocumentDbContext>(
            options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DocuStoreDb"),
                    b => b.MigrationsAssembly(typeof(DocumentDbContext).Assembly.FullName)),
            contextLifetime: ServiceLifetime.Transient,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddSingleton<DocumentDbContextFactory>(sp =>
        {
            return () =>
            {
                var scope = Document.Domain.Common.ServiceLocator.CreateScope();
                return scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
            };
        });

        services.AddSingleton<IFileStorageService, FileStorageService>();
        
        return services;
    }

    public static void InitializeDocumentDbContextProvider(IServiceProvider serviceProvider)
    {
        var contextFactory = serviceProvider.GetRequiredService<DocumentDbContextFactory>();
        DocumentDbContextProvider.Initialize(contextFactory);
    }
}