using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NutWinSvc
{
    public class Program
    {
        internal const string EventSourceName = "NutWinSvc";
        internal static readonly string exePath = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];
        internal static readonly string exeName = Path.GetFileName(exePath);


        public static void Main(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                Interactive.HandleCommand(args);
                return;
            }

            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddWindowsService(options => options.ServiceName = "Network UPS Tools Service");
            builder.Logging.AddEventLog(config => config.SourceName = EventSourceName);
#if !DEBUG
            builder.Configuration.Add<RegistryConfigurationSource>(config =>
            {
                config.RegistryHive = Microsoft.Win32.RegistryHive.LocalMachine;
                config.Path = @"SOFTWARE\NutWinSvc";
            });
#endif
            builder.Services.AddOptions<NutOptions>().Bind(builder.Configuration.GetSection("NutWinSvc")).ValidateOnStart();

            builder.Services.AddHostedService<CoreService>();
            var host = builder.Build();
            host.Run();
        }
    }
}