using static NutWinSvc.Installer;
using static NutWinSvc.Program;

namespace NutWinSvc;

internal static class Interactive
{
    internal static void HandleCommand(string[] args)
    {
        if (args.Length > 0)
            switch (args[0].ToLowerInvariant())
            {
                case "--install":
                    Install();
                    break;
                case "--uninstall":
                    Uninstall();
                    break;
                case "--configure":
                    Configure();
                    break;
                default:
                    Usage();
                    break;
            }
        else
            Usage();
    }


    private static void Usage()
    {
        Console.WriteLine($@"{exeName} <command>
<command>
  --install     Install the service.
  --uninstall   Uninstall the service.
  --configure   Configures the parameters of the service. [Interactive]");
        Environment.Exit(-1);
    }


    private static void CheckElevated()
    {
        if (!Environment.IsPrivilegedProcess)
        {
            Console.WriteLine("Command requires elevated privileges.");
            Environment.Exit(5);
        }
    }

    private static void Install()
    {
        CheckElevated();
        try
        {
            RegisterEventLog();
            RegisterService();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"{ex.Message}");
            Environment.Exit(-3);
        }
    }


    private static void Uninstall()
    {
        CheckElevated();
        try
        {
            UnregisterService();
            UnregisterEventLog();
            //TODO: Option to delete settings
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"{ex.Message}");
            Environment.Exit(-3);
        }
    }


    private static void Configure()
    {
        CheckElevated();
        //TODO: Configuration prompts
    }
}
