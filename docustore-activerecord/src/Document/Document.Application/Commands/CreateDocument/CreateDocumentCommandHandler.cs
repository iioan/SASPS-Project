using Document.Application.DTOs;
using Document.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Document.Application.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create document entity (factory method with validation)
        var document = DocumentEntity.Create(
            title: request.Title,
            description: request.Description,
            fileName: request.FileName,
            contentType: request.ContentType,
            createdBy: request.UserId
        );

        // 2. Entity handles file upload AND database save
        await document.UploadAndSave(request.FileContent, cancellationToken);

        // 3. Map to DTO and return
        return new DocumentDto(
            Id: document.Id,
            Title: document.Title,
            Description: document.Description,
            FileName: document.FileName,
            FileSizeInBytes: document.FileSizeInBytes,
            ContentType: document.ContentType,
            CreatedAt: document.CreatedAt,
            CreatedBy: document.CreatedBy
        );
    }
}