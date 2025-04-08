using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.DTO.Requests;
using Domain.DTO.Responses;
using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using Service.Interfaces;

namespace Service.Implementations;

public class TimelineSequenceService : ITimelineSequenceService
{
    private readonly IMapper _mapper;
    private readonly LRMSDbContext _context;

    public TimelineSequenceService(IMapper mapper, LRMSDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<TimelineSequenceResponse> CreateTimelineSequence(TimelineSequenceRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SequenceName) || request.CreatedBy == null)
            {
                throw new ServiceException("Sequence Name and CreatedBy are required");
            }

            var sequence = new TimelineSequence
            {
                SequenceName = request.SequenceName,
                SequenceDescription = request.SequenceDescription,
                SequenceColor = request.SequenceColor,
                CreatedBy = request.CreatedBy.Value,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.TimelineSequence.AddAsync(sequence);
            await _context.SaveChangesAsync();

            var createdUser = request.CreatedBy != null ? await _context.Users.FindAsync(request.CreatedBy) : null;
            
            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                CreatedAt = sequence.CreatedAt,
                UpdatedAt = sequence.UpdatedAt,
                CreatedBy = sequence.CreatedBy,
                CreatedByName = createdUser?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<IEnumerable<TimelineSequenceResponse>> GetAllTimelineSequences()
    {
        try
        {
            var sequences = await _context.TimelineSequence
                .Include(t => t.CreatedByNavigation)
                .AsNoTracking()
                .ToListAsync();

            return sequences.Select(s => new TimelineSequenceResponse
            {
                Id = s.SequenceId,
                SequenceName = s.SequenceName,
                SequenceDescription = s.SequenceDescription,
                SequenceColor = s.SequenceColor,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                CreatedByName = s.CreatedByNavigation?.FullName ?? "Unknown"
            });
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }

    public async Task<TimelineSequenceResponse> GetTimelineSequenceById(int id)
    {
        try
        {
            var sequence = await _context.TimelineSequence
                .Include(t => t.CreatedByNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.SequenceId == id);

            if (sequence == null)
                throw new ServiceException("Timeline Sequence not found");

            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                CreatedAt = sequence.CreatedAt,
                UpdatedAt = sequence.UpdatedAt,
                CreatedBy = sequence.CreatedBy,
                CreatedByName = sequence.CreatedByNavigation?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
} 