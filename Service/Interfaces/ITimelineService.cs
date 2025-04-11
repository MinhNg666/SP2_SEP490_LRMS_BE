using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.DTO.Requests;
using Domain.DTO.Responses;

namespace Service.Interfaces;

public interface ITimelineService
{
    Task<TimelineResponse> CreateTimeline(TimelineRequest request, int createdBy);
    Task<IEnumerable<TimelineResponse>> GetAllTimelines();
    Task<TimelineResponse> GetTimelineById(int id);
    Task<IEnumerable<TimelineResponse>> GetTimelinesBySequenceId(int sequenceId);
    Task<TimelineResponse> UpdateTimeline(int id, TimelineRequest request, int updatedBy);
} 