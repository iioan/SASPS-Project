using Document.Application.Interfaces;
using Document.Domain.Enums;
using MediatR;
using Versioning.Application.DTOs;
using Versioning.Application.Interfaces;
using Versioning.Domain.Entities;
using Versioning.Domain.Services;

namespace Versioning.Application.Commands.CreateVersion;

public class CreateVersionCommandHandler : IRequestHandler<CreateVersionCommand, VersionDto>
{
    private readonly IVersioningUnitOfWork _unitOfWork;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;

    public CreateVersionCommandHandler(
        IVersioningUnitOfWork unitOfWork,
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<VersionDto> Handle(CreateVersionCommand request, CancellationToken cancellationToken)
    {
        // Check if document exists and is active
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null || document.Status != DocumentStatus.Active)
        {
            throw new InvalidOperationException(
                $"Cannot add version. Document with ID '{request.DocumentId}' not found or is deleted");
        }

        // Get next version number
        var nextVersionNumber = await _unitOfWork.Versions.GetNextVersionNumberAsync(request.DocumentId, cancellationToken);

        // Create version entity
        var version = VersionEntity.Create(
            documentId: request.DocumentId,
            versionNumber: nextVersionNumber,
            fileName: request.FileName,
            contentType: request.ContentType,
            notes: request.Notes,
            createdBy: request.UserId
        );

        // Save file to disk
        var filePathOnDisk = await _fileStorageService.SaveVersionFileAsync(
            request.DocumentId,
            request.FileContent,
            nextVersionNumber,
            cancellationToken);

        // Set file info
        version.SetFileInfo(filePathOnDisk, request.FileContent.Length);

        // Mark other versions as not current
        var currentVersions = await _unitOfWork.Versions.GetCurrentVersionsForDocumentAsync(request.DocumentId, cancellationToken);
        foreach (var cv in currentVersions)
        {
            cv.MarkAsNotCurrent();
        }
        _unitOfWork.Versions.UpdateRange(currentVersions);

        // Add new version and save
        await _unitOfWork.Versions.AddAsync(version, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update current version marker on disk
        await _fileStorageService.UpdateCurrentVersionMarkerAsync(
            request.DocumentId,
            nextVersionNumber,
            cancellationToken);

        return new VersionDto(
            Id: version.Id,
            DocumentId: version.DocumentId,
            VersionNumber: version.VersionNumber,
            FileName: version.FileName,
            FileSizeInBytes: version.FileSizeInBytes,
            ContentType: version.ContentType,
            Notes: version.Notes,
            IsCurrent: version.IsCurrent,
            CreatedAt: version.CreatedAt,
            CreatedBy: version.CreatedBy
        );
    }
}
