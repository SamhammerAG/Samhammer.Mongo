using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Samhammer.Mongo
{
    public class InitializeConnectionService : IHostedService
    {
        private IServiceScopeFactory Services { get; }

        private ILogger<InitializeConnectionService> Logger { get; }

        public InitializeConnectionService(IServiceScopeFactory services, ILogger<InitializeConnectionService> logger)
        {
            Services = services;
            Logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                Logger.LogInformation("mongodb connecting...");

                var mongoConnector = scope.ServiceProvider.GetRequiredService<IMongoDbConnector>();
                await mongoConnector.Ping();

                Logger.LogInformation("mongodb connected!");
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
