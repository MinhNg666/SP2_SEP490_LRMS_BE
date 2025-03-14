using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;

public class PublicationService : IPublicationService
{
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public PublicationService(IMapper mapper, LRMSDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<PublicationResponse>> GetAllPublications()
    {
        try
        {
            var results = await _context.Projects
                .Where(p => p.Type == "Journal" || p.Type == "Conference")
                .Select(p => new PublicationResponse
                {
                    Id = p.ProjectId,
                    Type = p.Type,
                    Title = p.Title,
                    Abstract = p.Description,
                    Publisher = p.Type == "Journal" ?
                        p.Journals.FirstOrDefault().JournalName :
                        p.Conferences.FirstOrDefault().ConferenceName,
                    Department = p.Departments.Select(d => new DepartmentResponse
                    {
                        Id = d.DepartmentId,
                        Name = d.DepartmentName
                    }).FirstOrDefault(),
                    Category = p.Categories.Select(c => new CategoryResponse
                    {
                        Id = c.CategoryId,
                        Name = c.CategoryName
                    }).FirstOrDefault(),
                    Status = p.Status == 0 ? "pending" :
                            p.Status == 1 ? "accepted" :
                            p.Status == 2 ? "published" : "unknown",
                    SubmissionDate = p.Type == "Journal" ?
                        p.Journals.FirstOrDefault().SubmissionDate :
                        p.Conferences.FirstOrDefault().AcceptanceDate,
                    PublicationDate = p.Type == "Journal" ?
                        p.Journals.FirstOrDefault().PublicationDate :
                        p.Conferences.FirstOrDefault().PresentationDate,
                    Authors = p.Authors.Select(a => new AuthorResponse
                    {
                        Email = a.Email,
                        Role = a.Role == 0 ? "Main Author" : "Co-Author",
                        Name = a.Email
                    }).ToList(),
                    Progress = p.Milestones.Where(m => m.Status == 1)
                        .Average(m => m.ProgressPercentage ?? 0m),
                    Royalties = new RoyaltyResponse
                    {
                        Total = p.ApprovedBudget ?? 0,
                        Received = p.SpentBudget ?? 0,
                        PendingPayment = (p.ApprovedBudget ?? 0) > (p.SpentBudget ?? 0)
                    }
                })
                .AsNoTracking()
                .ToListAsync();

            return results;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<PublicationResponse> GetPublicationById(int id)
    {
        try
        {
            var result = await _context.Projects
                .Where(p => p.ProjectId == id)
                .Select(p => new PublicationResponse
                {
                    Id = p.ProjectId,
                    Type = p.Type,
                    Title = p.Title,
                    Abstract = p.Description,
                    Publisher = p.Type == "Journal" ?
                        p.Journals.FirstOrDefault().JournalName :
                        p.Conferences.FirstOrDefault().ConferenceName,
                    Department = p.Departments.Select(d => new DepartmentResponse
                    {
                        Id = d.DepartmentId,
                        Name = d.DepartmentName
                    }).FirstOrDefault(),
                    Category = p.Categories.Select(c => new CategoryResponse
                    {
                        Id = c.CategoryId,
                        Name = c.CategoryName
                    }).FirstOrDefault(),
                    Status = p.Status == 0 ? "pending" :
                            p.Status == 1 ? "accepted" :
                            p.Status == 2 ? "published" : "unknown",
                    SubmissionDate = p.Type == "Journal" ?
                        p.Journals.FirstOrDefault().SubmissionDate :
                        p.Conferences.FirstOrDefault().AcceptanceDate,
                    PublicationDate = p.Type == "Journal" ?
                        p.Journals.FirstOrDefault().PublicationDate :
                        p.Conferences.FirstOrDefault().PresentationDate,
                    Authors = p.Authors.Select(a => new AuthorResponse
                    {
                        Email = a.Email,
                        Role = a.Role == 0 ? "Main Author" : "Co-Author",
                        Name = a.Email
                    }).ToList(),
                    Progress = p.Milestones.Where(m => m.Status == 1)
                        .Average(m => m.ProgressPercentage ?? 0m),
                    Royalties = new RoyaltyResponse
                    {
                        Total = p.ApprovedBudget ?? 0,
                        Received = p.SpentBudget ?? 0,
                        PendingPayment = (p.ApprovedBudget ?? 0) > (p.SpentBudget ?? 0)
                    }
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (result == null)
                throw new ServiceException("Publication not found");

            return result;
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
}