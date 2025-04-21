using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LRMS_API.Services
{
    public class ProjectStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ProjectStatusUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IProjectPhaseRepository>();
                        await repository.UpdateProjectPhaseStatusesBasedOnDates();
                        await repository.UpdateProjectStatusBasedOnPhases();
                        Console.WriteLine($"Project statuses updated automatically at {DateTime.Now}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ProjectStatusUpdateService: {ex.Message}");
                }

                // Run once per day
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
