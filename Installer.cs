using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using static NutWinSvc.Program;

namespace NutWinSvc
{
    internal static class Installer
    {
        private const string serviceShortName = "nutwinsvc";
        private const string serviceDisplayName = "NUT Windows Service";

        public static void RegisterEventLog()
        {
            Console.WriteLine("Adding event log source...");
            if (!EventLog.SourceExists(EventSourceName))
                EventLog.CreateEventSource(EventSourceName, "Application");
        }

        public static void UnregisterEventLog()
        {
            Console.WriteLine("Removing event log source...");
            if (EventLog.SourceExists(EventSourceName))
                EventLog.DeleteEventSource(EventSourceName, "Application");
        }

        public static void RegisterService()
        {
            Console.WriteLine("Adding service...");
            using RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true) ?? throw new InvalidOperationException("Could not open Services registry.");
            if (!reg.GetSubKeyNames().Contains(serviceShortName, StringComparer.OrdinalIgnoreCase))
            {
                using RegistryKey svcReg = reg.CreateSubKey(serviceShortName, true) ?? throw new InvalidOperationException("Could not create Services registry key.");
                svcReg.SetValue("Description", "Network UPS Tools Windows monitoring and shutdown service");
                svcReg.SetValue("DisplayName", serviceDisplayName);
                svcReg.SetValue("ErrorControl", 0x1);
                svcReg.SetValue("ImagePath", exePath);
                svcReg.SetValue("ObjectName", "LocalSystem");
                svcReg.SetValue("Start", 0x2);
                svcReg.SetValue("Type", 0x10);
                svcReg.Close();
            }
            reg.Close();
        }

        public static void UnregisterService()
        {
            ServiceController? svc = ServiceController.GetServices().Where(x => x.DisplayName == serviceDisplayName).FirstOrDefault();
            if (svc != null)
            {
                try
                {
                    if (svc.Status != ServiceControllerStatus.Stopped)
                    {
                        Console.WriteLine("Stopping service...");
                        svc.Stop();
                    }
                    int i = 0;
                    while (svc.Status != ServiceControllerStatus.Stopped && i < 10)
                    {
                        Task.Delay(TimeSpan.FromSeconds(1));
                        svc.Refresh();
                        i++;
                    }
                }
                catch { };
                if (svc.Status != ServiceControllerStatus.Stopped) throw new InvalidOperationException("Could not stop service to remove it.");
            }
            Console.WriteLine("Removing service...");
            using RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true) ?? throw new InvalidOperationException("Could not open Services registry.");
            if (reg.GetSubKeyNames().Contains(serviceShortName, StringComparer.OrdinalIgnoreCase))
                reg.DeleteSubKeyTree(serviceShortName, false);
        }
    }
}
