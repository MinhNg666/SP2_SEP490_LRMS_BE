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
using Domain.Constants;

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

    public async Task<TimelineSequenceResponse> CreateTimelineSequence(TimelineSequenceRequest request, int createdBy)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SequenceName))
            {
                throw new ServiceException("Sequence Name is required");
            }

            // Set default status to Active if not provided
            if (request.Status == null)
            {
                request.Status = (int)TimelineSequenceStatusEnum.Active;
            }

            var sequence = new TimelineSequence
            {
                SequenceName = request.SequenceName,
                SequenceDescription = request.SequenceDescription,
                SequenceColor = request.SequenceColor,
                Status = request.Status,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.TimelineSequence.AddAsync(sequence);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users.FindAsync(createdBy);
            
            string statusName = sequence.Status.HasValue 
                ? Enum.GetName(typeof(TimelineSequenceStatusEnum), sequence.Status) 
                : "Unknown";
            
            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                Status = sequence.Status,
                StatusName = statusName,
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

    public async Task<TimelineSequenceResponse> UpdateTimelineSequence(int id, TimelineSequenceRequest request, int updatedBy)
    {
        try
        {
            var sequence = await _context.TimelineSequence.FindAsync(id);
            if (sequence == null)
            {
                throw new ServiceException("Timeline Sequence not found");
            }

            // Update the sequence properties
            sequence.SequenceName = request.SequenceName ?? sequence.SequenceName;
            sequence.SequenceDescription = request.SequenceDescription ?? sequence.SequenceDescription;
            sequence.SequenceColor = request.SequenceColor ?? sequence.SequenceColor;
            sequence.Status = request.Status ?? sequence.Status;
            sequence.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var updatedUser = await _context.Users.FindAsync(updatedBy);
            
            string statusName = sequence.Status.HasValue 
                ? Enum.GetName(typeof(TimelineSequenceStatusEnum), sequence.Status) 
                : "Unknown";

            return new TimelineSequenceResponse
            {
                Id = sequence.SequenceId,
                SequenceName = sequence.SequenceName,
                SequenceDescription = sequence.SequenceDescription,
                SequenceColor = sequence.SequenceColor,
                Status = sequence.Status,
                StatusName = statusName,
                CreatedAt = sequence.CreatedAt,
                UpdatedAt = sequence.UpdatedAt,
                CreatedBy = sequence.CreatedBy,
                CreatedByName = updatedUser?.FullName ?? "Unknown"
            };
        }
        catch (Exception e)
        {
            throw new ServiceException(e.Message);
        }
    }
} 