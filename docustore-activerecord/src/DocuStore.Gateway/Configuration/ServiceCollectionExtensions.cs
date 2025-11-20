using Document.Application;
using Document.Infrastructure;
using MetadataIndexing.Application;
using MetadataIndexing.Infrastructure;
using Shared.Events;
using Tagging.Application;
using Tagging.Infrastructure;
using Versioning.Application;
using Versioning.Infrastructure;

namespace DocuStore.Gateway.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuStoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add module services
        services.AddDocumentApplication();
        services.AddDocumentInfrastructure(configuration);
        
        services.AddMetadataIndexingApplication();
        services.AddMetadataIndexingInfrastructure(configuration);
        
        services.AddTaggingApplication();
        services.AddTaggingInfrastructure(configuration);

        services.AddVersioningApplication();
        services.AddVersioningInfrastructure(configuration);
        
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

        return services;
    }

    public static IServiceCollection AddDocuStoreApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "DocuStore API", Version = "v1" });
        });

        return services;
    }

    public static IServiceCollection AddDocuStoreCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }
}