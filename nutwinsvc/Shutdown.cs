using System.Runtime.InteropServices;

namespace NutWinSvc
{
    internal static partial class Shutdown
    {
        public static void ShutdownSystem(string message)
        {
            //"UPS has been on battery for the alotted time and system will now shut down."
            InitiateSystemShutdownExW(string.Empty, message, 20, true, false, ReasonPower);
        }

        private const UInt32 SHTDN_REASON_MAJOR_POWER = 0x00060000;
        private const UInt32 SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c;
        private const UInt32 ReasonPower = SHTDN_REASON_MAJOR_POWER | SHTDN_REASON_MINOR_ENVIRONMENT;

        [LibraryImport("advapi32.dll", EntryPoint = "InitiateSystemShutdownExW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool InitiateSystemShutdownExW(
            [MarshalAs(UnmanagedType.LPWStr)] String lpMachineName,
            [MarshalAs(UnmanagedType.LPWStr)] String lpMessage,
            [MarshalAs(UnmanagedType.U4)] UInt32 dwTimeout,
            [MarshalAs(UnmanagedType.Bool)] bool bForceAppsClosed,
            [MarshalAs(UnmanagedType.Bool)] bool bRebootAfterShutdown,
            [MarshalAs(UnmanagedType.U4)] UInt32 dwReason);

    }
}
