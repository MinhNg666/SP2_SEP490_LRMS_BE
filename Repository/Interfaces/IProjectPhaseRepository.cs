using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LRMS_API;

namespace Repository.Interfaces;
public interface IProjectPhaseRepository : IGenericRepository<ProjectPhase>
{
    Task<int> AddProjectPhaseAsync(ProjectPhase projectPhase);
    Task<bool> UpdateProjectPhaseStatusAsync(int projectPhaseId, int status);
    Task UpdateProjectPhaseStatusesBasedOnDates();
}
