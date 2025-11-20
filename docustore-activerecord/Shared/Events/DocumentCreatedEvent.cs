namespace Shared.Events;

public record DocumentCreatedEvent(
    Guid DocumentId,
    string FileName,
    byte[] FileContent,
    string ContentType,
    string CreatedBy,
    DateTime CreatedAt
);