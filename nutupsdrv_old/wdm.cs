global using unsafe PMDL = nutupsdrv.MDL*;
using unsafe PDEVICE_OBJECT = nutupsdrv.DEVICE_OBJECT*;
using KPROCESSOR_MODE = sbyte; //CCHAR
using KIRQL = sbyte; //CCHAR
using unsafe PDRIVER_ADD_DEVICE = nutupsdrv.DRIVER_ADD_DEVICE*;

using nutupsdrv;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace nutupsdrv;

[StructLayout(LayoutKind.Sequential, Pack = 0)]
public unsafe struct MDL
{
    MDL  *Next;
    CSHORT Size;
    CSHORT MdlFlags;
    EPROCESS  *Process;
    PVOID MappedSystemVa;
    PVOID StartVa;
    ULONG ByteCount;
    ULONG ByteOffset;
}


//    [StructLayout(LayoutKind.Sequential, Pack = 0)]
//public unsafe struct IRP
//{
//    CSHORT Type;
//    USHORT Size;
//    PMDL MdlAddress;
//    ULONG Flags;
//    union {
//        IRP* MasterIrp;
//        volatile LONG IrpCount;
//        PVOID SystemBuffer;
//    }
//AssociatedIrp;
//  LIST_ENTRY ThreadListEntry;
//IO_STATUS_BLOCK IoStatus;
//KPROCESSOR_MODE RequestorMode;
//BOOLEAN PendingReturned;
//CHAR StackCount;
//CHAR CurrentLocation;
//BOOLEAN Cancel;
//KIRQL CancelIrql;
//CCHAR ApcEnvironment;
//UCHAR AllocationFlags;
//union {
//    PIO_STATUS_BLOCK UserIosb;
//PVOID IoRingContext;
//  };
//PKEVENT UserEvent;
//union {
//    struct {
//    union {
//        PIO_APC_ROUTINE UserApcRoutine;
//    PVOID IssuingProcess;
//};
//union {
//        PVOID                 UserApcContext;
//#if ...
//        _IORING_OBJECT        *IoRing;
//#else
//struct _IORING_OBJECT *IoRing;
//#endif
//      };
//    } AsynchronousParameters;
//LARGE_INTEGER AllocationSize;
//  } Overlay;
//volatile PDRIVER_CANCEL CancelRoutine;
//PVOID UserBuffer;
//union {
//    struct {
//    union {
//        KDEVICE_QUEUE_ENTRY DeviceQueueEntry;
//    struct {
//        PVOID DriverContext[4];
//    };
//};
//PETHREAD Thread;
//PCHAR AuxiliaryBuffer;
//struct {
//    LIST_ENTRY ListEntry;
//    union {
//          struct _IO_STACK_LOCATION *CurrentStackLocation;
//          ULONG PacketType;
//};
//      };
//PFILE_OBJECT OriginalFileObject;
//    } Overlay;
//KAPC Apc;
//PVOID CompletionKey;
//  } Tail;
//}

public unsafe struct VPB
{
    public short Type;
    public short Size;
    public ushort Flags;
    public ushort VolumeLabelLength;
    public PDEVICE_OBJECT DeviceObject;
    public PDEVICE_OBJECT RealDevice;
    public uint SerialNumber;
    public uint ReferenceCount;
    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
    public string VolumeLabel; // WCHAR array as string
}

//typedef struct _KDEVICE_QUEUE_ENTRY
//{
//    LIST_ENTRY DeviceListEntry;
//    ULONG SortKey;
//    BOOLEAN Inserted;
//}
//KDEVICE_QUEUE_ENTRY, * PKDEVICE_QUEUE_ENTRY,
//* RESTRICTED_POINTER PRKDEVICE_QUEUE_ENTRY;

//typedef struct _KDEVICE_QUEUE
//{
//    CSHORT Type;
//    CSHORT Size;
//    LIST_ENTRY DeviceListHead;
//    KSPIN_LOCK Lock;
//    BOOLEAN Busy;
//}
//KDEVICE_QUEUE, * PKDEVICE_QUEUE, * RESTRICTED_POINTER PRKDEVICE_QUEUE;


public struct KDEVICE_QUEUE_ENTRY
{
    public LinkedListNode<KDEVICE_QUEUE_ENTRY> DeviceListEntry;
    public uint SortKey;
    public bool Inserted;
}

public struct KDEVICE_QUEUE
{
    public short Type;
    public short Size;
    public LinkedList<KDEVICE_QUEUE_ENTRY> DeviceListHead;
    public object Lock; // Replace with appropriate lock type if needed
    public bool Busy;
}



public struct WAIT_CONTEXT_BLOCK
{
    public KDEVICE_QUEUE_ENTRY WaitQueueEntry;
    public IntPtr DeviceRoutine;
    public IntPtr DeviceContext;
    public uint NumberOfMapRegisters;
    public IntPtr DeviceObject;
    public IntPtr CurrentIrp;
    public IntPtr BufferChainingDpc;
}

[StructLayout(LayoutKind.Sequential)]
public struct KDPC
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DUMMYUNIONNAMEUNION
    {
        [FieldOffset(0)]
        public uint TargetInfoAsUlong;

        [FieldOffset(0)]
        public DUMMYSTRUCTNAMEUNION DUMMYSTRUCTNAME;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DUMMYSTRUCTNAMEUNION
    {
        public byte Type;
        public byte Importance;
        public ushort Number; // volatile is not directly represented in C#
    }

    public DUMMYSTRUCTNAMEUNION DUMMYUNIONNAME;
    public IntPtr DpcListEntry; // SINGLE_LIST_ENTRY is typically represented as IntPtr
    public IntPtr ProcessorHistory; // KAFFINITY is typically represented as IntPtr
    public IntPtr DeferredRoutine; // PKDEFERRED_ROUTINE is typically represented as IntPtr
    public IntPtr DeferredContext;
    public IntPtr SystemArgument1;
    public IntPtr SystemArgument2;
    public IntPtr DpcData;
}

[StructLayout(LayoutKind.Sequential)]
public struct DISPATCHER_HEADER
{
    public byte Type;
    public byte Absolute;
    public byte Size;
    public byte Inserted;
    public int SignalState;
    public IntPtr WaitListHead; // Assuming LIST_ENTRY is a pointer type
}

[StructLayout(LayoutKind.Sequential)]
public struct KEVENT
{
    public DISPATCHER_HEADER Header;
}

[StructLayout(LayoutKind.Sequential)]
public struct DEVICE_OBJECT
{
    public short Type;
    public ushort Size;
    public int ReferenceCount;
    public IntPtr DriverObject;
    public IntPtr NextDevice;
    public IntPtr AttachedDevice;
    public IntPtr CurrentIrp;
    public IntPtr Timer;
    public uint Flags;
    public uint Characteristics;
    public IntPtr Vpb;
    public IntPtr DeviceExtension;
    public ULONG DeviceType;
    public byte StackSize;
    public QueueUnion Queue;
    public uint AlignmentRequirement;
    public KDEVICE_QUEUE DeviceQueue;
    public KDPC Dpc;
    public uint ActiveThreadCount;
    public IntPtr SecurityDescriptor;
    public KEVENT DeviceLock;
    public ushort SectorSize;
    public ushort Spare1;
    public IntPtr DeviceObjectExtension; //DEVOBJ_EXTENSION *DeviceObjectExtension;
    public IntPtr Reserved; //PVOID
    [StructLayout(LayoutKind.Explicit)]
    public struct QueueUnion
    {
        [FieldOffset(0)]
        public LIST_ENTRY ListEntry;
        [FieldOffset(8)]
        public WAIT_CONTEXT_BLOCK Wcb;
    }
}

//[StructLayout(LayoutKind.Sequential, Pack = 0)]
//public unsafe struct DEVICE_OBJECT
//{
//    CSHORT Type;
//    USHORT Size;
//    LONG ReferenceCount;
//    DRIVER_OBJECT* DriverObject;
//    DEVICE_OBJECT* NextDevice;
//    DEVICE_OBJECT* AttachedDevice;
//    //IRP* CurrentIrp;
//    IntPtr CurrentIrp;
//    PIO_TIMER Timer;
//    ULONG Flags;
//    ULONG Characteristics;
//    volatile PVPB Vpb;
//    PVOID DeviceExtension;
//    DEVICE_TYPE DeviceType;
//    CCHAR StackSize;
//    union {
//        LIST_ENTRY ListEntry;
//        WAIT_CONTEXT_BLOCK Wcb;
//    } Queue;
//    ULONG AlignmentRequirement;
//    KDEVICE_QUEUE DeviceQueue;
//    KDPC Dpc;
//    ULONG ActiveThreadCount;
//    PSECURITY_DESCRIPTOR SecurityDescriptor;
//    KEVENT DeviceLock;
//    USHORT SectorSize;
//    USHORT Spare1;
//    DEVOBJ_EXTENSION *DeviceObjectExtension;
//    PVOID Reserved;
//}

//[StructLayout(LayoutKind.Sequential, Pack = 0)]
//public unsafe struct DRIVER_OBJECT
//{
//    CSHORT Type;
//    CSHORT Size;
//    DEVICE_OBJECT* DeviceObject;
//    ULONG Flags;
//    PVOID DriverStart;
//    ULONG DriverSize;
//    PVOID DriverSection;
//    DRIVER_EXTENSION* DriverExtension;
//    UNICODE_STRING DriverName;
//    UNICODE_STRING* HardwareDatabase;
//    FAST_IO_DISPATCH* FastIoDispatch;
//    DRIVER_INITIALIZE* DriverInit;
//    DRIVER_STARTIO* DriverStartIo;
//    DRIVER_UNLOAD* DriverUnload;
//    DRIVER_DISPATCH* MajorFunction[IRP_MJ_MAXIMUM_FUNCTION + 1];
//}

public delegate int DRIVER_ADD_DEVICE(IntPtr driverObject, IntPtr deviceObject);

public unsafe struct DRIVER_EXTENSION
{
    public DRIVER_OBJECT *DriverObject;
    public PDRIVER_ADD_DEVICE AddDevice;
    public uint Count;
    public UNICODE_STRING ServiceKeyName;
}

public struct DRIVER_OBJECT
{
    public short Type;
    public short Size;
    public IntPtr DeviceObject; // Assuming PDEVICE_OBJECT is a pointer
    public uint Flags;
    public IntPtr DriverStart; // Assuming PVOID is a pointer
    public uint DriverSize;
    public IntPtr DriverSection; // Assuming PVOID is a pointer
    public DRIVER_EXTENSION DriverExtension;
    public string DriverName; // Assuming UNICODE_STRING can be represented as a string
    public IntPtr HardwareDatabase; // Assuming PUNICODE_STRING is a pointer
    public IntPtr FastIoDispatch; // Assuming PVOID is a pointer
    public IntPtr DriverInit; // Assuming PDRIVER_INITIALIZE is a function pointer
    public IntPtr DriverStartIo; // Assuming PDRIVER_STARTIO is a function pointer
    public IntPtr DriverUnload; // Assuming PDRIVER_UNLOAD is a function pointer
    public IntPtr[] MajorFunction; // Assuming PDRIVER_DISPATCH is a function pointer array
}

