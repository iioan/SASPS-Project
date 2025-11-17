using Microsoft.EntityFrameworkCore;

namespace MetadataIndexing.Domain.Common;

public static class MetadataIndexingDbContextProvider
{
    private static MetadataIndexingDbContextFactory? _contextFactory;

    public static void Initialize(MetadataIndexingDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public static DbContext GetContext()
    {
        if (_contextFactory == null)
        {
            throw new InvalidOperationException(
                "MetadataIndexingDbContextProvider has not been initialized. Call Initialize() during application startup.");
        }

        return _contextFactory();
    }
}