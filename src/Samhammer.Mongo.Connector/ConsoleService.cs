using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Samhammer.Mongo.Connector
{
    public class ConsoleService : BackgroundService
    {
        private readonly ILogger<ConsoleService> _logger;
        private readonly IServiceScopeFactory _services;

        public ConsoleService(ILogger<ConsoleService> logger, IServiceScopeFactory services)
        {
            this._logger = logger;
            this._services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _services.CreateScope())
            {
                _logger.LogInformation("mongodb connecting...");

                try
                {
                    var connectTimer = Stopwatch.StartNew();

                    var connector = scope.ServiceProvider.GetRequiredService<IMongoDbConnector>();
                    var ping = await connector.Ping();

                    connectTimer.Stop();

                    _logger.LogInformation("mongodb connect time: {ConnectTime}", connectTimer.Elapsed);
                    _logger.LogInformation("mongodb ping result: {Ping}", ping);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "mongodb connect failed");
                }
            }
        }
    }
}
