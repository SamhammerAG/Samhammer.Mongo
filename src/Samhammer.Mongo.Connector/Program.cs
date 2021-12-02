using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Samhammer.Mongo.Connector
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("MongoDbOptions:TraceDriver", "true")
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddMongoDb(hostContext.Configuration);

                    services
                        .AddHostedService<ConsoleService>();
                });
    }
}
