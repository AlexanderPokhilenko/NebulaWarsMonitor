using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NebulaWarsMonitor.Models.EF;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NebulaWarsMonitor
{
    public class ServersCheckerService : IHostedService, IDisposable
    {
        private const int DefaultPort = 14065;
        private readonly Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly TimeSpan Period = TimeSpan.FromSeconds(5);

        public ServersCheckerService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer.Change(Period, Period);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            await StopAsync(new CancellationToken());
            using (var scope = _scopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
                {
                    foreach (var server in context.Servers)
                    {
                        try
                        {
                            var httpClient = new HttpClient();
                            var result = await httpClient.GetAsync("http://" + server.Address + $":{DefaultPort}/Status");
                            if (result.IsSuccessStatusCode)
                            {
                                await context.LogRecords.AddAsync(await CreateRecord(context, server, MessageTypeEnum.Info, "OK"));
                            }
                            else
                            {
                                await context.LogRecords.AddAsync(await CreateRecord(context, server, MessageTypeEnum.Warning, $"Status code was {result.StatusCode}"));
                            }
                        }
                        catch (Exception e)
                        {
                            await context.LogRecords.AddAsync(await CreateRecord(context, server, MessageTypeEnum.Error, e.Message));
                        }
                    }

                    await context.SaveChangesAsync();
                }
            }
            await StartAsync(new CancellationToken());
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private static async Task<LogRecord> CreateRecord(ApplicationContext context, Server server, MessageTypeEnum type, string message)
        {
            return new LogRecord(type, await GetOrCreateMessage(context, message), server);
        }

        private  static async Task<LogMessage> GetOrCreateMessage(ApplicationContext context, string message)
        {
            return await context.LogMessages.FirstOrDefaultAsync(m => m.Message == message) ?? new LogMessage(message);
        }
    }
}
