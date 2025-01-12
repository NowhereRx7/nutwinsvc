using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace NutWinSvc
{
    internal class CoreService(ILogger<CoreService> logger, IOptions<NutOptions> options) : BackgroundService
    {
        private readonly ILogger<CoreService> _logger = logger;
        private readonly NutOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

       
    }
}
