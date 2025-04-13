using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Constants;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;

public class DocumentService : IDocumentService
{
    private readonly LRMSDbContext _context;
    private readonly IMapper _mapper;
    private readonly IS3Service _s3Service;
    private readonly ITimelineService _timelineService;

    public DocumentService(
        LRMSDbContext context,
        IMapper mapper,
        IS3Service s3Service,
        ITimelineService timelineService)
    {
        _context = context;
        _mapper = mapper;
        _s3Service = s3Service;
        _timelineService = timelineService;
    }

    public async Task<DocumentResponse> SubmitDocument(int projectId, IFormFile file, int documentType, int uploadedBy, int? sequenceId)
    {
        try
        {
            // Kiểm tra thời gian nộp tài liệu
            var isValidTime = await _timelineService.IsValidTimeForAction(
                TimelineTypes.SubmitDocument,
                sequenceId
            );

            if (!isValidTime)
                throw new ServiceException("Out of time for document submission");

            // Upload file lên S3
            var documentUrl = await _s3Service.UploadFileAsync(file, $"projects/{projectId}/documents");

            // Tạo ProjectResource
            var projectResource = new ProjectResource
            {
                ResourceName = file.FileName,
                ResourceType = 1, // Document type
                ProjectId = projectId,
                Acquired = true,
                Quantity = 1
            };

            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            // Tạo Document
            var document = new Document
            {
                ProjectId = projectId,
                DocumentUrl = documentUrl,
                FileName = file.FileName,
                DocumentType = documentType,
                UploadAt = DateTime.Now,
                UploadedBy = uploadedBy,
                ProjectResourceId = projectResource.ProjectResourceId
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();

            return _mapper.Map<DocumentResponse>(document);
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<IEnumerable<DocumentResponse>> GetProjectDocuments(int projectId)
    {
        try
        {
            var documents = await _context.Documents
                .Include(d => d.UploadedByNavigation)
                .Where(d => d.ProjectId == projectId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentResponse>>(documents);
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
} 