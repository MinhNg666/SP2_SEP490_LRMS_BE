// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Domain.DTO.Responses;
// using LRMS_API;
// using Microsoft.EntityFrameworkCore;
// using Repository.Interfaces;
// using Service.Exceptions;
// using Service.Interfaces;

// namespace Service.Implementations;

// public class PublicationService : IPublicationService
// {
//     private readonly IMapper _mapper;
//     private readonly LRMSDbContext _context;

//     public PublicationService(IMapper mapper, LRMSDbContext context)
//     {
//         _mapper = mapper;
//         _context = context;
//     }

//     public async Task<IEnumerable<PublicationResponse>> GetAllPublications()
//     {
//         try
//         {
//             var results = await _context.Projects
//                 .Where(p => p.ProjectType == 1 || p.ProjectType == 2) // 1: Journal, 2: Conference
//                 .Include(p => p.Authors)
//                     .ThenInclude(a => a.User)
//                 .Include(p => p.Department)
//                 .Include(p => p.Categories)
//                 .Include(p => p.Journals)
//                 .Include(p => p.Conferences)
//                 .Include(p => p.Milestones)
//                 .AsNoTracking()
//                 .ToListAsync();

//             return results.Select(p => new PublicationResponse
//             {
//                 Id = p.ProjectId,
//                 Type = p.ProjectType == 1 ? "Journal" : "Conference",
//                 Title = p.ProjectName,
//                 Abstract = p.Description,
//                 Publisher = p.ProjectType == 1 ?
//                     p.Journals?.FirstOrDefault()?.JournalName :
//                     p.Conferences?.FirstOrDefault()?.ConferenceName,
//                 Department = p.Department != null ? new DepartmentResponse
//                 {
//                     Id = p.Department.DepartmentId,
//                     Name = p.Department.DepartmentName
//                 } : null,
//                 Category = p.Categories?.Select(c => new CategoryResponse
//                 {
//                     Id = c.CategoryId,
//                     Name = c.CategoryName
//                 }).FirstOrDefault(),
//                 Status = p.Status == 0 ? "pending" :
//                         p.Status == 1 ? "accepted" :
//                         p.Status == 2 ? "published" : "unknown",
//                 SubmissionDate = p.ProjectType == 1 ?
//                     p.Journals?.FirstOrDefault()?.SubmissionDate :
//                     p.Conferences?.FirstOrDefault()?.AcceptanceDate,
//                 PublicationDate = p.ProjectType == 1 ?
//                     p.Journals?.FirstOrDefault()?.PublicationDate :
//                     p.Conferences?.FirstOrDefault()?.PresentationDate,
//                 Authors = p.Authors?.Select(a => new AuthorResponse
//                 {
//                     Name = a.User?.Email ?? "Unknown",
//                     Role = a.Role == 0 ? "Main Author" : "Co-Author",
//                     Email = a.User?.Email ?? "Unknown"
//                 }).ToList() ?? new List<AuthorResponse>(),
//                 Progress = p.Milestones != null && p.Milestones.Any(m => m.Status == 1) ?
//                     Convert.ToDecimal(p.Milestones.Where(m => m.Status == 1)
//                         .Average(m => m.Status)) : 0m,
//                 Royalties = new RoyaltyResponse
//                 {
//                     Total = p.ApprovedBudget ?? 0,
//                     Received = p.SpentBudget,
//                     PendingPayment = (p.ApprovedBudget ?? 0) > p.SpentBudget
//                 }
//             });
//         }
//         catch (Exception e)
//         {
//             throw new ServiceException(e.Message);
//         }
//     }

//     public async Task<PublicationResponse> GetPublicationById(int id)
//     {
//         try
//         {
//             var result = await _context.Projects
//                 .Where(p => p.ProjectId == id)
//                 .Include(p => p.Authors)
//                     .ThenInclude(a => a.User)
//                 .Include(p => p.Department)
//                 .Include(p => p.Categories)
//                 .Include(p => p.Journals)
//                 .Include(p => p.Conferences)
//                 .Include(p => p.Milestones)
//                 .AsNoTracking()
//                 .FirstOrDefaultAsync();

//             if (result == null)
//                 throw new ServiceException("Publication not found");

//             return new PublicationResponse
//             {
//                 Id = result.ProjectId,
//                 Type = result.ProjectType == 1 ? "Journal" : "Conference",
//                 Title = result.ProjectName,
//                 Abstract = result.Description,
//                 Publisher = result.ProjectType == 1 ?
//                     result.Journals?.FirstOrDefault()?.JournalName :
//                     result.Conferences?.FirstOrDefault()?.ConferenceName,
//                 Department = result.Department != null ? new DepartmentResponse
//                 {
//                     Id = result.Department.DepartmentId,
//                     Name = result.Department.DepartmentName
//                 } : null,
//                 Category = result.Categories?.Select(c => new CategoryResponse
//                 {
//                     Id = c.CategoryId,
//                     Name = c.CategoryName
//                 }).FirstOrDefault(),
//                 Status = result.Status == 0 ? "pending" :
//                         result.Status == 1 ? "accepted" :
//                         result.Status == 2 ? "published" : "unknown",
//                 SubmissionDate = result.ProjectType == 1 ?
//                     result.Journals?.FirstOrDefault()?.SubmissionDate :
//                     result.Conferences?.FirstOrDefault()?.AcceptanceDate,
//                 PublicationDate = result.ProjectType == 1 ?
//                     result.Journals?.FirstOrDefault()?.PublicationDate :
//                     result.Conferences?.FirstOrDefault()?.PresentationDate,
//                 Authors = result.Authors?.Select(a => new AuthorResponse
//                 {
//                     Name = a.User?.Email ?? "Unknown",
//                     Role = a.Role == 0 ? "Main Author" : "Co-Author",
//                     Email = a.User?.Email ?? "Unknown"
//                 }).ToList() ?? new List<AuthorResponse>(),
//                 Progress = result.Milestones != null && result.Milestones.Any(m => m.Status == 1) ?
//                     Convert.ToDecimal(result.Milestones.Where(m => m.Status == 1)
//                         .Average(m => m.Status)) : 0m,
//                 Royalties = new RoyaltyResponse
//                 {
//                     Total = result.ApprovedBudget ?? 0,
//                     Received = result.SpentBudget,
//                     PendingPayment = (result.ApprovedBudget ?? 0) > result.SpentBudget
//                 }
//             };
//         }
//         catch (Exception e)
//         {
//             throw new ServiceException(e.Message);
//         }
//     }
// }