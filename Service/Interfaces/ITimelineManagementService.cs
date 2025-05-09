using Domain.DTO.Requests;
using Domain.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ITimelineManagementService
    {
        Task<IEnumerable<TimelineResponse>> GetCurrentActiveTimelines();
        Task<TimelineResponse> CreateActionTimeline(TimelineRequest request, int createdBy);
        Task<bool> ExtendTimeline(int timelineId, DateTime newEndDate, int updatedBy);
    }
} 