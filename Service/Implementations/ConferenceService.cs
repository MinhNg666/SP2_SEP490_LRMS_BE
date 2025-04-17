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

public class ConferenceService : IConferenceService
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IS3Service _s3Service;
    private readonly ITimelineService _timelineService;
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public ConferenceService(
        IConferenceRepository conferenceRepository,
        IProjectRepository projectRepository,
        IS3Service s3Service,
        ITimelineService timelineService,
        IMapper mapper,
        LRMSDbContext context)
    {
        _conferenceRepository = conferenceRepository;
        _projectRepository = projectRepository;
        _s3Service = s3Service;
        _timelineService = timelineService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ConferenceResponse> CreateConference(CreateConferenceRequest request, int createdBy)
    {
        try
        {
            // Kiểm tra project tồn tại
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new ServiceException("Project not found");

            // Tạo conference
            var conference = new Conference
            {
                ConferenceName = request.ConferenceName,
                ConferenceRanking = request.ConferenceRanking,
                Location = request.Location,
                PresentationDate = request.PresentationDate,
                AcceptanceDate = request.AcceptanceDate,
                PresentationType = request.PresentationType,
                ProjectId = request.ProjectId
            };

            await _conferenceRepository.AddAsync(conference);

            // Tạo conference expense
            var expense = new ConferenceExpense
            {
                Accomodation = request.Accomodation,
                AccomodationExpense = request.AccomodationExpense,
                Travel = request.Travel,
                TravelExpense = request.TravelExpense,
                ConferenceId = conference.ConferenceId
            };

            await _conferenceRepository.AddExpenseAsync(expense);

            // Lấy thông tin conference vừa tạo kèm theo project và expense
            var createdConference = await _conferenceRepository.GetConferenceWithDetailsAsync(conference.ConferenceId);
            return _mapper.Map<ConferenceResponse>(createdConference);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error creating conference: {ex.Message}");
        }
    }

    public async Task AddConferenceDocument(int conferenceId, IFormFile documentFile, int userId)
    {
        try
        {
            // Kiểm tra conference tồn tại
            var conference = await _conferenceRepository.GetByIdAsync(conferenceId);
            if (conference == null)
                throw new ServiceException("Conference not found");

            // Lấy conference expense
            var expense = await _context.ConferenceExpenses
                .FirstOrDefaultAsync(e => e.ConferenceId == conferenceId);
            if (expense == null)
                throw new ServiceException("Conference expense not found");

            // Upload file
            var documentUrl = await _s3Service.UploadFileAsync(documentFile, $"conferences/{conferenceId}/documents");

            // Tạo ProjectResource cho document
            var projectResource = new ProjectResource
            {
                ResourceName = documentFile.FileName,
                ResourceType = 1, // Document type
                ProjectId = conference.ProjectId.Value,
                Acquired = true,
                Quantity = 1
            };
            
            await _context.ProjectResources.AddAsync(projectResource);
            await _context.SaveChangesAsync();

            // Tạo document
            var document = new Document
            {
                ConferenceExpenseId = expense.ExpenseId,
                ProjectId = conference.ProjectId,
                DocumentUrl = documentUrl,
                FileName = documentFile.FileName,
                DocumentType = (int)DocumentTypeEnum.ConferenceProposal,
                UploadAt = DateTime.Now,
                UploadedBy = userId,
                ProjectResourceId = projectResource.ProjectResourceId
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error uploading conference document: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceResponse>> GetAllConferences()
    {
        try
        {
            var conferences = await _conferenceRepository.GetAllConferencesWithDetailsAsync();
            return _mapper.Map<IEnumerable<ConferenceResponse>>(conferences);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting all conferences: {ex.Message}");
        }
    }

    public async Task<ConferenceResponse> GetConferenceById(int conferenceId)
    {
        try
        {
            var conference = await _conferenceRepository.GetConferenceWithDetailsAsync(conferenceId);
            if (conference == null)
                throw new ServiceException("Conference not found");

            return _mapper.Map<ConferenceResponse>(conference);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting conference by id: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ConferenceResponse>> GetConferencesByProjectId(int projectId)
    {
        try
        {
            var conferences = await _conferenceRepository.GetAllConferencesWithDetailsAsync();
            var projectConferences = conferences.Where(c => c.ProjectId == projectId);
            return _mapper.Map<IEnumerable<ConferenceResponse>>(projectConferences);
        }
        catch (Exception ex)
        {
            throw new ServiceException($"Error getting conferences by project id: {ex.Message}");
        }
    }

    public async Task AddConferenceDocuments(int conferenceId, IEnumerable<IFormFile> documentFiles, int userId)
    {
        try
        {
            var conference = await _context.Conferences
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);
            
            if (conference == null)
                throw new ServiceException("Conference not found");
            
            int projectId = conference.ProjectId.Value;
            
            if (documentFiles != null && documentFiles.Any())
            {
                var urls = await _s3Service.UploadFilesAsync(documentFiles, $"projects/{projectId}/conferences/{conferenceId}");
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
                        DocumentType = (int)DocumentTypeEnum.ConferenceProposal,
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
            throw new ServiceException($"Error uploading conference documents: {ex.Message}");
        }
    }
} 