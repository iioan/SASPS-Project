using Microsoft.EntityFrameworkCore;

namespace MetadataIndexing.Domain.Common;

public abstract class ActiveRecordBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public string CreatedBy { get; protected set; } = string.Empty;
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    /// <summary>
    /// Gets the DbContext from service locator
    /// </summary>
    protected static DbContext GetDbContext()
    {
        return ServiceLocator.GetService<DbContext>();
    }

    /// <summary>
    /// Gets a scoped service from service locator
    /// </summary>
    protected static T GetService<T>() where T : notnull
    {
        return ServiceLocator.GetService<T>();
    }

    protected void SetCreatedBy(string userId)
    {
        CreatedBy = userId;
    }

    protected void SetUpdatedBy(string userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }
}