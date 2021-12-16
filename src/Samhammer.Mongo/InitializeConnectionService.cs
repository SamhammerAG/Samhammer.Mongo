using System;
using System.Diagnostics;
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
                try
                {
                    Logger.LogInformation("mongodb connecting...");
                    var mongoConnector = scope.ServiceProvider.GetRequiredService<IMongoDbConnector>();

                    var connectTimer = Stopwatch.StartNew();
                    await mongoConnector.Ping();
                    connectTimer.Stop();

                    Logger.LogInformation("mongodb connected in {ConnectTime} ms", (int)connectTimer.Elapsed.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "mongodb connect failed");
                }
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
