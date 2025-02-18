using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace nutupsdrv;

public unsafe class Driver
{
    //https://learn.microsoft.com/en-us/windows-hardware/drivers/battery/ups-minidriver-functionality

    public enum UpsState : UInt32
    {
        None = 0,
        Online = 1,
        OnBattery = 2,
        LowBattery = 4,
        NoComm = 8,
        Critical = 16,
    }

    public enum InitState : UInt32
    {
        UnknownError = 0,
        OK = 1,
        [Obsolete("Not sure if this is still used.")]
        NoSuchDriver = 2,
        BadInterface = 3,
        RegistryError = 4,
        CommOperError = 5,
        CommSetupError = 6,
    }

    const UInt32 INFINITE = 0xFFFFFF;

    static CancellationTokenSource cancellationTokenSource = null;

    /// <summary>
    /// The UPSInit function initializes a UPS minidriver, opens communication to the UPS unit, updates the registry, and causes the minidriver to start monitoring the UPS unit.
    /// </summary>
    /// <returns>
    /// The UPSInit function returns one of the following DWORD values:
    /// Return code 	Description
    ///    UPS_INITOK        No errors were encountered during initialization.
    ///    UPS_INITREGISTRYERROR        An error occurred while accessing the registry.
    ///    UPS_INITCOMMOPENERROR        An error occurred while opening the COM port.
    ///    UPS_INITCOMMSETUPERROR       An error occurred while setting up the COM port.
    ///    UPS_INITUNKNOWNERROR        An unidentified error occurred.
    /// </returns>
    [UnmanagedCallersOnly(EntryPoint = "UPSInit")]
    [return: MarshalAs(UnmanagedType.U4)]
    public static InitState UPSInit()
    {
        cancellationTokenSource ??= new();
        return InitState.OK;
    }

    /// <summary>
    /// The UPSGetState function returns the operational state of the UPS.
    /// </summary>
    /// <returns>
    /// The UPSGetState function returns one of the following DWORD values:<br />
    /// Return code 	Description
    ///    <see cref="UPS_ONLINE"/>     Utility-supplied power is normal.
    ///    <see cref="UPS_ONBATTERY"/>  Utility-supplied power is inadequate, and the UPS batteries are discharging.
    ///    <see cref="UPS_LOWBATTERY"/> Utility-supplied power is inadequate, and the UPS batteries are critically low.
    ///    <see cref="UPS_NOCOMM"/>     Communication with the UPS is not currently established.
    /// </returns>
    [UnmanagedCallersOnly(EntryPoint = "UPSGetState")]
    [return: MarshalAs(UnmanagedType.U4)]
    public static UpsState UPSGetState()
    {
        return UpsState.NoComm;
    }

    /// <summary>
    /// The UPSWaitForStateChange function waits until a specified UPS state changes, or until a time-out interval elapses.
    /// </summary>
    /// <param name="aCurrentState">
    /// Specifies the UPS state on which to wait. When the state of the UPS system changes from the specified state to any other state, the function returns.<br/>
    /// The specified value can be one of the following:<br/>
    /// <see cref="UPS_ONLINE"/><br/>
    /// <see cref="UPS_ONBATTERY"/><br/>
    /// <see cref="UPS_LOWBATTERY"/><br/>
    /// <see cref="UPS_NOCOMM"/>
    /// </param>
    /// <param name="anInterval">
    /// Specifies a time-out interval, in milliseconds, for the function. If the UPS state has not changed from the specified state when the interval elapses, the function returns. 
    /// A value of INFINITE means the interval never elapses.
    /// </param>
    [UnmanagedCallersOnly(EntryPoint = "UPSWaitForStateChange")]
    public static void UPSWaitForStateChange([In, MarshalAs(UnmanagedType.U4)] UpsState aCurrentState, [In] UInt32 anInterval)
    {
        if (anInterval == INFINITE)
        {
            Task.Delay(-1, cancellationTokenSource.Token).GetAwaiter().GetResult();
        } else
        {
            Task.Delay((int)anInterval, cancellationTokenSource.Token).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// The UPSCancelWait function cancels all waits initiated by calls to <see cref="UPSWaitForStateChange(uint, uint)"/>.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "UPSCancelWait")]
    public static void UPSCancelWait()
    {
        UPSCancelWaitImpl();
    }

    private static void UPSCancelWaitImpl()
    {
        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// The UPSTurnOff function turns off the UPS unit's power outlets, after a specified delay time.
    /// </summary>
    /// <param name="aTurnOffDelay">
    /// Specifies the minimum amount of time, in seconds, to wait before turning off the UPS unit's power outlets.
    /// </param>
    [UnmanagedCallersOnly(EntryPoint = "UPSTurnOff")]
    public static void UPSTurnOff([In] UInt32 aTurnOffDelay)
    {

    }

    /// <summary>
    /// The UPSStop function causes a UPS minidriver to stop monitoring its UPS unit.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "UPSStop")]
    public static void UPSStop()
    {
        UPSCancelWaitImpl();
        Task.Delay(100).GetAwaiter().GetResult();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }

}
