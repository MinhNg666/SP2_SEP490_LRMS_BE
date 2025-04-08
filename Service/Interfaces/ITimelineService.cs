using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.DTO.Requests;
using Domain.DTO.Responses;

namespace Service.Interfaces;

public interface ITimelineService
{
    Task<TimelineResponse> CreateTimeline(TimelineRequest request);
    Task<IEnumerable<TimelineResponse>> GetAllTimelines();
    Task<TimelineResponse> GetTimelineById(int id);
} 