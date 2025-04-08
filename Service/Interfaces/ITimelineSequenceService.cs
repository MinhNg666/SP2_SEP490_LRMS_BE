using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.DTO.Requests;
using Domain.DTO.Responses;

namespace Service.Interfaces;

public interface ITimelineSequenceService
{
    Task<TimelineSequenceResponse> CreateTimelineSequence(TimelineSequenceRequest request);
    Task<IEnumerable<TimelineSequenceResponse>> GetAllTimelineSequences();
    Task<TimelineSequenceResponse> GetTimelineSequenceById(int id);
} 