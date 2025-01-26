namespace NutWinSvc
{
    public class Program
    {
        internal const string EventSourceName = "NutWinSvc";
        internal static readonly string exePath = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];
        internal static readonly string exeName = Path.GetFileName(exePath);


        public static void Main(string[] args)
        {
#if !DEBUG
            if (System.Environment.UserInteractive)
            {
                Interactive.HandleCommand(args);
                return;
            }
#endif

            var builder = Host.CreateApplicationBuilder();
            builder.Services.AddWindowsService(options => options.ServiceName = "Network UPS Tools Service");
#if DEBUG
            builder.Logging.AddConsole();
#else
            builder.Logging.AddEventLog(config =>
            {
                config.SourceName = EventSourceName;
                config.Filter = (s, logLevel) => logLevel > LogLevel.Debug;
            });
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