using Document.Domain.Common;
using DocuStore.Gateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDocuStoreApi();
builder.Services.AddDocuStoreServices(builder.Configuration);
builder.Services.AddDocuStoreCors();

var app = builder.Build();

// ServiceLocator
Document.Domain.Common.ServiceLocator.Initialize(app.Services);
MetadataIndexing.Domain.Common.ServiceLocator.Initialize(app.Services);
Tagging.Domain.Common.ServiceLocator.Initialize(app.Services);
Versioning.Domain.Common.ServiceLocator.Initialize(app.Services);

// DbContextProvider init
Document.Infrastructure.DependencyInjection.InitializeDocumentDbContextProvider(app.Services);
MetadataIndexing.Infrastructure.DependencyInjection.InitializeMetadataIndexingDbContextProvider(app.Services);
Tagging.Infrastructure.DependencyInjection.InitializeTaggingDbContextProvider(app.Services);
Versioning.Infrastructure.DependencyInjection.InitializeVersioningDbContextProvider(app.Services);


// Run database migrations
await app.Services.MigrateDocuStoreDatabaseAsync();

// Configure pipeline
app.UseDocuStoreSwagger();
app.UseDocuStoreEndpoints();

app.Run();