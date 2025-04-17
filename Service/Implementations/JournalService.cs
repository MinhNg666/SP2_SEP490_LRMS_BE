using AutoMapper;
using Domain.Constants;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;

public class JournalService : IJournalService
{
    private readonly IJournalRepository _journalRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IS3Service _s3Service;
    private readonly ITimelineService _timelineService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public JournalService(
        IJournalRepository journalRepository,
        IProjectRepository projectRepository,
        IS3Service s3Service,
        ITimelineService timelineService,
        IMapper mapper,
        LRMSDbContext context)
    {
        _journalRepository = journalRepository;
        _projectRepository = projectRepository;
        _s3Service = s3Service;
        _timelineService = timelineService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<JournalResponse> CreateJournal(CreateJournalRequest request, int createdBy)
    {
        try
        {
            // Kiểm tra project tồn tại
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new ServiceException("Project not found");

            // Tạo journal
            var journal = new Journal
            {
                JournalName = request.JournalName,
                PublisherName = request.PublisherName,
                PublisherStatus = request.PublisherStatus,
                DoiNumber = request.DoiNumber,
                AcceptanceDate = request.AcceptanceDate,
                PublicationDate = request.PublicationDate,
                SubmissionDate = request.SubmissionDate,
                ReviewerComments = request.ReviewerComments,
                ProjectId = request.ProjectId
            };

            await _journalRepository.AddAsync(journal);
            
            // Lấy thông tin journal vừa tạo kèm theo project
            var createdJournal = await _journalRepository.GetJournalWithDetailsAsync(journal.JournalId);
            return _mapper.Map<JournalResponse>(createdJournal);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating journal: {ex.Message}");
        }
    }

    public async Task AddJournalDocument(int journalId, IFormFile documentFile, int userId)
    {
        try
        {
            // Kiểm tra journal tồn tại
            var journal = await _journalRepository.GetByIdAsync(journalId);
            if (journal == null)
                throw new ServiceException("Journal not found");

            // Upload file
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"journals/{journalId}/documents");

            // Tạo ProjectResource cho document
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document type
                ProjectId = journal.ProjectId,
                Acquired = true,
                Quantity = 1
            };
            
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            // Tạo document
            var document = new Document
            {
                ProjectId = journal.ProjectId,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.JournalPaper,
                UploadAt = DateTime.Now,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading journal document: {ex.Message}");
        }
    }

    public async Task<IEnumerable<JournalResponse>> GetAllJournals()
    {
        try
        {
            var journals = await _journalRepository.GetAllJournalsWithDetailsAsync();
            Console.WriteLine($"Found {journals?.Count() ?? 0} journals");
            
            if (journals == null || !journals.Any())
            {
                Console.WriteLine("No journals found");
                return new List<JournalResponse>();
            }

            var responses = _mapper.Map<IEnumerable<JournalResponse>>(journals);
            Console.WriteLine($"Mapped {responses?.Count() ?? 0} journal responses");
            
            return responses ?? new List<JournalResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllJournals: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw new ServiceException($"Error getting all journals: {ex.Message}");
        }
    }

    public async Task<JournalResponse> GetJournalById(int journalId)
    {
        try
        {
            var journal = await _journalRepository.GetJournalWithDetailsAsync(journalId);
            if (journal == null)
                throw new ServiceException("Journal not found");

            return _mapper.Map<JournalResponse>(journal);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting journal by id: {ex.Message}");
        }
    }

    public async Task<IEnumerable<JournalResponse>> GetJournalsByProjectId(int projectId)
    {
        try
        {
            var journals = await _journalRepository.GetAllJournalsWithDetailsAsync();
            var projectJournals = journals.Where(j => j.ProjectId == projectId);
            return _mapper.Map<IEnumerable<JournalResponse>>(projectJournals);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting journals by project id: {ex.Message}");
        }
    }

    public async Task AddJournalDocuments(int journalId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            var journal = await _context.Journals
                .Include(j => j.Project)
                .FirstOrDefaultAsync(j => j.JournalId == journalId);
            
            if (journal == null)
                throw new ServiceException("Journal not found");
            
            int projectId = journal.ProjectId.Value;
            
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/journals/{journalId}");
                int index = 0;
                
                foreach (var file in documentFiles)
                {
                    // Create ProjectResource for document
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
                    
                    // Create document with the resource
                    var document = new Document
                    {
                        ProjectId = projectId,
                        DocumentUrl = urls[index],
                        FileName = file.FileName,
                        DocumentType = (int)DocumentTypeEnum.JournalPaper,
                        UploadAt = DateTime.Now,
                        UploadedBy = userId,
                        ProjectResourceId = projectResource.ProjectResourceId
                    };

                    await _context.Documents.AddAsync(document);
                    index++;
                }
                
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading journal documents: {ex.Message}");
        }
    }
} 