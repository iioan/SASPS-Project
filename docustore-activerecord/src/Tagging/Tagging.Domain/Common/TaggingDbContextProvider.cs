using Microsoft.EntityFrameworkCore;

namespace Tagging.Domain.Common;

public static class TaggingDbContextProvider
{
    private static TaggingDbContextFactory? _contextFactory;

    public static void Initialize(TaggingDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public static DbContext GetContext()
    {
        if (_contextFactory == null)
        {
            throw new InvalidOperationException(
                "TaggingDbContextProvider has not been initialized. Call Initialize() during application startup.");
        }

        return _contextFactory();
    }
}