using AutoMapper;
using Domain.Constants;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class QuotaService : IQuotaService
    {
        private readonly LRMSDbContext _context;
        private readonly IMapper _mapper;

        public QuotaService(LRMSDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuotaResponse>> GetAllQuotas()
        {
            try
            {
                var quotas = await _context.Quotas
                    .Include(q => q.Project)
                    .Include(q => q.AllocatedByNavigation)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<QuotaResponse>>(quotas);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting all quotas: {ex.Message}");
            }
        }

        public async Task<IEnumerable<QuotaResponse>> GetQuotasByUserId(int userId)
        {
            try
            {
                // Get all groups the user is a member of
                var userGroups = await _context.GroupMembers
                    .Where(gm => gm.UserId == userId && gm.Status == (int)GroupMemberStatus.Active)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();

                if (!userGroups.Any())
                {
                    return Enumerable.Empty<QuotaResponse>();
                }

                // Get projects associated with these groups
                var projectIds = await _context.Projects
                    .Where(p => p.GroupId.HasValue && userGroups.Contains(p.GroupId.Value))
                    .Select(p => p.ProjectId)
                    .ToListAsync();

                if (!projectIds.Any())
                {
                    return Enumerable.Empty<QuotaResponse>();
                }

                // Get quotas for these projects
                var quotas = await _context.Quotas
                    .Include(q => q.Project)
                    .Include(q => q.AllocatedByNavigation)
                    .Where(q => q.ProjectId.HasValue && projectIds.Contains(q.ProjectId.Value))
                    .ToListAsync();

                return _mapper.Map<IEnumerable<QuotaResponse>>(quotas);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error getting quotas by user ID: {ex.Message}");
            }
        }
    }
}
