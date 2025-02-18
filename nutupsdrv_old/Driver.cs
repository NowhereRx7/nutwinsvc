using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nutupsdrv;

public unsafe class Driver
{


    [System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "DriverEntry")]
    public unsafe static NTSTATUS DriverEntry([In] DRIVER_OBJECT* Driver, [In] PUNICODE_STRING RegistryPath)
    {
        WDF_DRIVER_CONFIG config = new WDF_DRIVER_CONFIG();
        WDF_DRIVER_CONFIG_INIT(ref config);
        config.DriverInitFlags = WDF_DRIVER_INIT_FLAGS.WdfDriverInitNonPnpDriver;


        return NTSTATUS.Success;
    }
}