using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Samhammer.Mongo
{
    public class InitializeConnectionService : IHostedService
    {
        private IServiceScopeFactory Services { get; }

        public InitializeConnectionService(IServiceScopeFactory services)
        {
            Services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                var mongoConnector = scope.ServiceProvider
                    .GetRequiredService<IMongoDbConnector>();
                mongoConnector.GetOrCreateConnection();
            }

            await CompletedTask();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CompletedTask();
        }

        protected virtual Task CompletedTask()
        {
            return Task.CompletedTask;
        }
    }
}
