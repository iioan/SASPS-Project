using Microsoft.EntityFrameworkCore;

namespace Versioning.Domain.Common;

public static class VersioningDbContextProvider
{
    private static VersioningDbContextFactory? _contextFactory;

    public static void Initialize(VersioningDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public static DbContext GetContext()
    {
        if (_contextFactory == null)
        {
            throw new InvalidOperationException(
                "VersioningDbContextProvider has not been initialized. Call Initialize() during application startup.");
        }

        return _contextFactory();
    }
}