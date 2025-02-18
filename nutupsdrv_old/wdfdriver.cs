using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace nutupsdrv;

class wdfdriver
{
    public enum WDF_DRIVER_INIT_FLAGS : ULONG
    {
        WdfDriverInitNonPnpDriver = 0x00000001, //  If set, no Add Device routine is assigned.
        WdfDriverInitNoDispatchOverride = 0x00000002, //  Useful for miniports.
        WdfVerifyOn = 0x00000004, //  Controls whether WDFVERIFY macros are live.
        WdfVerifierOn = 0x00000008, //  Top level verififer flag.
        WdfDriverInitCompanion = 0x00000010, //  If set, Add Companion needs to be assigned
    }

    //_Function_class_(EVT_WDF_DRIVER_DEVICE_ADD)
    //_IRQL_requires_same_
    //_IRQL_requires_max_(PASSIVE_LEVEL)
    //NTAPI
    public unsafe delegate NTSTATUS EVT_WDF_DRIVER_DEVICE_ADD(
        [In]
        object Driver,
        //WDFDRIVER Driver,
        [In][Out]
        ref object DeviceInit
        //ref WDFDEVICE_INIT DeviceInit
        );

    public unsafe delegate void EVT_WDF_DRIVER_UNLOAD(
        [In]
        object Driver
        //WDFDRIVER Driver
        );


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WDF_DRIVER_CONFIG
    {
        /// <summary>
        /// Size of this structure in bytes
        /// </summary>
        public ULONG Size;

        /// <summary>
        /// Device added event callback.
        /// </summary>
        public EVT_WDF_DRIVER_DEVICE_ADD* EvtDriverDeviceAdd;

        /// <summary>
        /// Driver unload event callback.
        /// </summary>
        public EVT_WDF_DRIVER_UNLOAD* EvtDriverUnload;

        /// <summary>
        /// Combination of WDF_DRIVER_INIT_FLAGS values
        /// </summary>
        public WDF_DRIVER_INIT_FLAGS DriverInitFlags;

        /// <summary>
        /// Pool tag to use for all allocations made by the framework on behalf of the client driver.
        /// </summary>
        public ULONG DriverPoolTag;
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void WDF_DRIVER_CONFIG_INIT(
            ref WDF_DRIVER_CONFIG Config,
            [Optional] EVT_WDF_DRIVER_DEVICE_ADD* EvtDriverDeviceAdd
        )
    {
        Config.EvtDriverUnload = null;
        Config.EvtDriverDeviceAdd = null;
        Config.DriverInitFlags = 0;
        Config.DriverPoolTag = 0;
        Config.Size = (ULONG)sizeof(WDF_DRIVER_CONFIG);
        Config.EvtDriverDeviceAdd = EvtDriverDeviceAdd;
    }
}

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
public delegate int PFN_WDFDRIVERCREATE(
    IntPtr DriverGlobals,
    IntPtr DriverObject,
    ref string RegistryPath,
    IntPtr DriverAttributes,
    IntPtr DriverConfig,
    out IntPtr Driver
);

public static class WdfDriverGlobals
{
    public static IntPtr Value { get; set; }
}

public static class WdfDriverCreate
{
    const int WdfDriverCreateTableIndex = 57;

    [return: MarshalAs(UnmanagedType.I4)]
    public static int Invoke(
        IntPtr DriverObject,
        ref string RegistryPath,
        IntPtr DriverAttributes,
        IntPtr DriverConfig,
        out IntPtr Driver
    )
    {
        return ((PFN_WDFDRIVERCREATE)WdfFunctions[WdfDriverCreateTableIndex])(
            WdfDriverGlobals.Value,
            DriverObject,
            ref RegistryPath,
            DriverAttributes,
            DriverConfig,
            out Driver
        );
    }
}