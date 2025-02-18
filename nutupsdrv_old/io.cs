global using unsafe PIO_TIMER = nutupsdrv.IO_TIMER*;
global using unsafe PIO_TIMER_ROUTINE = nutupsdrv.IO_TIMER_ROUTINE*;

using System.Runtime.InteropServices;

namespace nutupsdrv
{

    unsafe delegate void IO_TIMER_ROUTINE(
        [In] DEVICE_OBJECT* DeviceObject,
        [In, Optional] PVOID Context
    );

    public unsafe struct IO_TIMER
    {
        CSHORT Type;
        CSHORT TimerFlag;
        LIST_ENTRY TimerList;
        IO_TIMER_ROUTINE TimerRoutine;
        PVOID Context;
        DEVICE_OBJECT* DeviceObject;
    }
}

