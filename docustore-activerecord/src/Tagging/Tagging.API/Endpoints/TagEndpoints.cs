using Document.API.Models;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Tagging.API.Models;
using Tagging.Domain.Entities;

namespace Tagging.API.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tags")
            .WithTags("Tags");

        // US-010: Create a Tag
        group.MapPost("/", CreateTag)
            .Produces<TagResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .WithName("CreateTag")
            .WithSummary("Create a new tag")
            .WithDescription("Create a new tag with a unique name and optional description");

        // US-011: List All Available Tags
        group.MapGet("/", ListTags)
            .Produces<List<TagResponse>>(StatusCodes.Status200OK)
            .WithName("ListTags")
            .WithSummary("List all tags")
            .WithDescription("Retrieve all available tags sorted alphabetically by name");

        // US-012: Add Tag to Document
        group.MapPost("/documents/{documentId:guid}/tags", AddTagToDocument)
            .Produces<DocumentTagResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("AddTagToDocument")
            .WithSummary("Add tag to document")
            .WithDescription("Associate a tag with a document");

        // US-013: Remove Tag from Document
        group.MapDelete("/documents/{documentId:guid}/tags/{tagId:guid}", RemoveTagFromDocument)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("RemoveTagFromDocument")
            .WithSummary("Remove tag from document")
            .WithDescription("Remove the association between a tag and a document");

        // US-014: View Document Tags
        group.MapGet("/documents/{documentId:guid}/tags", GetDocumentTags)
            .Produces<List<DocumentTagResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetDocumentTags")
            .WithSummary("Get document tags")
            .WithDescription("Retrieve all tags associated with a specific document");

        // US-015: Find Documents by Tag
        group.MapGet("/{tagId:guid}/documents", GetDocumentsByTag)
            .Produces<List<DocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithName("GetDocumentsByTag")
            .WithSummary("Find documents by tag")
            .WithDescription("Retrieve all documents that have a specific tag");

        // US-015 with multiple tags support
        group.MapGet("/documents", GetDocumentsByTags)
            .Produces<List<DocumentResponse>>(StatusCodes.Status200OK)
            .WithName("GetDocumentsByTags")
            .WithSummary("Find documents by multiple tags")
            .WithDescription("Retrieve all documents that have all specified tags (AND logic)");

        return endpoints;
    }

    private static async Task<IResult> CreateTag(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if tag with same name already exists (case-insensitive)
            var existingTag = await Tag.FindByName(request.Name, cancellationToken);
            if (existingTag != null)
            {
                return Results.BadRequest(new ErrorResponse(
                    Message: $"Tag with name '{request.Name}' already exists",
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }

            var tag = Tag.Create(request.Name, request.Description, "system");
            await tag.Save(cancellationToken);

            var response = new TagResponse(
                Id: tag.Id,
                Name: tag.Name,
                Description: tag.Description,
                CreatedAt: tag.CreatedAt,
                CreatedBy: tag.CreatedBy
            );

            return Results.Created($"/api/tags/{tag.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse(
                Message: ex.Message,
                StatusCode: StatusCodes.Status400BadRequest
            ));
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while creating the tag"
            );
        }
    }

    private static async Task<IResult> ListTags(
        [FromQuery] bool includeDocumentCount,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = await Tag.All(cancellationToken);
            
            Dictionary<Guid, int>? documentCounts = null;
            if (includeDocumentCount)
            {
                documentCounts = await DocumentTag.GetDocumentCountsByTag(cancellationToken);
            }

            var response = tags.Select(tag => new TagResponse(
                Id: tag.Id,
                Name: tag.Name,
                Description: tag.Description,
                CreatedAt: tag.CreatedAt,
                CreatedBy: tag.CreatedBy,
                DocumentCount: includeDocumentCount && documentCounts != null
                    ? documentCounts.GetValueOrDefault(tag.Id, 0)
                    : null
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving tags"
            );
        }
    }

    private static async Task<IResult> AddTagToDocument(
        Guid documentId,
        [FromBody] AddTagToDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate tag exists
            var tagExists = await Tag.Exists(request.TagId, cancellationToken);
            if (!tagExists)
            {
                return Results.NotFound(new ErrorResponse(
                    Message: $"Tag with ID '{request.TagId}' not found",
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }

            // Check if document exists by calling the Document module
            // For now, we'll skip this check and assume the document exists
            // In a real implementation, this would call the Document module API or use events

            // Check if association already exists
            var existingAssociation = await DocumentTag.FindByDocumentAndTag(documentId, request.TagId, cancellationToken);
            if (existingAssociation != null)
            {
                return Results.BadRequest(new ErrorResponse(
                    Message: "Tag is already associated with this document",
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }

            var documentTag = DocumentTag.Create(documentId, request.TagId, "system");
            await documentTag.Save(cancellationToken);

            // Reload to get tag information
            var savedDocumentTag = await DocumentTag.Find(documentTag.Id, cancellationToken);

            var response = new DocumentTagResponse(
                Id: savedDocumentTag!.Id,
                TagId: savedDocumentTag.TagId,
                TagName: savedDocumentTag.Tag.Name,
                TagDescription: savedDocumentTag.Tag.Description,
                AddedAt: savedDocumentTag.CreatedAt,
                AddedBy: savedDocumentTag.CreatedBy
            );

            return Results.Created($"/api/tags/documents/{documentId}/tags/{request.TagId}", response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while adding tag to document"
            );
        }
    }

    private static async Task<IResult> RemoveTagFromDocument(
        Guid documentId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            var documentTag = await DocumentTag.FindByDocumentAndTag(documentId, tagId, cancellationToken);
            if (documentTag == null)
            {
                return Results.NotFound(new ErrorResponse(
                    Message: "Tag is not associated with this document",
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }

            await documentTag.Delete(cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while removing tag from document"
            );
        }
    }

    private static async Task<IResult> GetDocumentTags(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if document exists - for now we'll assume it exists
            // In production, validate against Document module

            var documentTags = await DocumentTag.GetByDocument(documentId, cancellationToken);

            var response = documentTags.Select(dt => new DocumentTagResponse(
                Id: dt.Id,
                TagId: dt.TagId,
                TagName: dt.Tag.Name,
                TagDescription: dt.Tag.Description,
                AddedAt: dt.CreatedAt,
                AddedBy: dt.CreatedBy
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving document tags"
            );
        }
    }

    private static async Task<IResult> GetDocumentsByTag(
        Guid tagId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate tag exists
            var tagExists = await Tag.Exists(tagId, cancellationToken);
            if (!tagExists)
            {
                return Results.NotFound(new ErrorResponse(
                    Message: $"Tag with ID '{tagId}' not found",
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }

            var documentTags = await DocumentTag.GetByTag(tagId, cancellationToken);
            var documentIds = documentTags.Select(dt => dt.DocumentId).Distinct().ToList();

            // In production, this would call the Document module to get document details
            // For now, return just the document IDs
            var response = documentIds.Select(docId => new DocumentResponse(
                Id: docId,
                Title: "Document Title", // Placeholder - would come from Document module
                Description: null,
                FileName: "file.pdf",
                FileSizeInBytes: 0,
                ContentType: "application/pdf",
                CreatedAt: DateTime.UtcNow,
                CreatedBy: "system",
                Status: "Active"
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving documents by tag"
            );
        }
    }

    private static async Task<IResult> GetDocumentsByTags(
        [FromQuery] string tagIds,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tagIds))
            {
                return Results.BadRequest(new ErrorResponse(
                    Message: "At least one tag ID must be provided",
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }

            // Parse comma-separated tag IDs
            var tagIdList = tagIds.Split(',')
                .Select(id => Guid.TryParse(id.Trim(), out var guid) ? guid : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            if (tagIdList.Count == 0)
            {
                return Results.BadRequest(new ErrorResponse(
                    Message: "Invalid tag IDs provided",
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }

            // Get all document-tag associations for the specified tags
            var documentTags = await DocumentTag.GetByTags(tagIdList, cancellationToken);

            // Group by document ID and filter for documents that have ALL specified tags
            var documentIdsWithAllTags = documentTags
                .GroupBy(dt => dt.DocumentId)
                .Where(g => g.Select(dt => dt.TagId).Distinct().Count() == tagIdList.Count)
                .Select(g => g.Key)
                .ToList();

            // In production, this would call the Document module to get document details
            var response = documentIdsWithAllTags.Select(docId => new DocumentResponse(
                Id: docId,
                Title: "Document Title",
                Description: null,
                FileName: "file.pdf",
                FileSizeInBytes: 0,
                ContentType: "application/pdf",
                CreatedAt: DateTime.UtcNow,
                CreatedBy: "system",
                Status: "Active"
            )).ToList();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while retrieving documents by tags"
            );
        }
    }
}
