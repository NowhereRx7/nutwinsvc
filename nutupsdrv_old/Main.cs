using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nutupsdrv
{
    //BOOL WINAPI DllMain(
    //HINSTANCE hinstDLL,  // handle to DLL module
    //DWORD fdwReason,     // reason for calling function
    //LPVOID lpvReserved)  // reserved
    //{
    //    // Perform actions based on the reason for calling.
    //    switch (fdwReason)
    //    {
    //        case DLL_PROCESS_ATTACH:
    //            // Initialize once for each new process.
    //            // Return FALSE to fail DLL load.
    //            break;

    //        case DLL_THREAD_ATTACH:
    //            // Do thread-specific initialization.
    //            break;

    //        case DLL_THREAD_DETACH:
    //            // Do thread-specific cleanup.
    //            break;

    //        case DLL_PROCESS_DETACH:

    //            if (lpvReserved != nullptr)
    //            {
    //                break; // do not do cleanup if process termination scenario
    //            }

    //            // Perform any necessary cleanup.
    //            break;
    //    }
    //    return TRUE;  // Successful DLL_PROCESS_ATTACH.
    //}
    class Main
    {
        [System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "DllMain")]
        public static bool DllMain(IntPtr hinstDll, uint fdwReason, IntPtr lpvReserved)
        {

            switch (fdwReason)
            {
                case 1: // DLL_PROCESS_ATTACH
                        // Initialize once for each new process.
                        // Return false to fail DLL load.
                    break;

                case 2: // DLL_THREAD_ATTACH
                        // Do thread-specific initialization.
                    break;

                case 3: // DLL_THREAD_DETACH
                        // Do thread-specific cleanup.
                    break;

                case 0: // DLL_PROCESS_DETACH
                    if (lpvReserved != IntPtr.Zero)
                    {
                        break; // do not do cleanup if process termination scenario
                    }
                    // Perform any necessary cleanup.
                    break;
            }
            return false;
            //return true; // Successful DLL_PROCESS_ATTACH.
        }
    }
}
