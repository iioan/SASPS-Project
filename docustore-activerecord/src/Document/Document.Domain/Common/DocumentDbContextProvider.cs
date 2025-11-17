using Microsoft.EntityFrameworkCore;

namespace Document.Domain.Common;

/// <summary>
/// Provides access to DocumentDbContext without creating a circular dependency
/// </summary>
public static class DocumentDbContextProvider
{
    private static DocumentDbContextFactory? _contextFactory;

    /// <summary>
    /// Initialize the provider with a factory function (called from Infrastructure layer)
    /// </summary>
    public static void Initialize(DocumentDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Get the DocumentDbContext instance
    /// </summary>
    public static DbContext GetContext()
    {
        if (_contextFactory == null)
        {
            throw new InvalidOperationException(
                "DocumentDbContextProvider has not been initialized. Call Initialize() during application startup.");
        }

        return _contextFactory();
    }
}