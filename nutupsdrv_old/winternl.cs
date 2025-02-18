using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace nutupsdrv
{

    //public unsafe struct PEB
    //{
    //    BYTE Reserved1[2];
    //    BYTE BeingDebugged;
    //    BYTE Reserved2[1];
    //    PVOID Reserved3[2];
    //    PPEB_LDR_DATA Ldr;
    //    PRTL_USER_PROCESS_PARAMETERS ProcessParameters;
    //    PVOID Reserved4[3];
    //    PVOID AtlThunkSListPtr;
    //    PVOID Reserved5;
    //    ULONG Reserved6;
    //    PVOID Reserved7;
    //    ULONG Reserved8;
    //    ULONG AtlThunkSListPtr32;
    //    PVOID Reserved9[45];
    //    BYTE Reserved10[96];
    //    PPS_POST_PROCESS_INIT_ROUTINE PostProcessInitRoutine;
    //    BYTE Reserved11[128];
    //    PVOID Reserved12[1];
    //    ULONG SessionId;
    //}

    public struct PEB
    {
        [System.Runtime.InteropServices.MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
        public byte[] Reserved1; // 2 bytes
        public byte BeingDebugged;
        [System.Runtime.InteropServices.MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 1)]
        public byte[] Reserved2; // 1 byte
        [System.Runtime.InteropServices.MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.FunctionPtr, SizeConst = 2)]
        public IntPtr[] Reserved3; // 2 pointers
        public IntPtr Ldr; // Pointer to PEB_LDR_DATA
        public IntPtr ProcessParameters; // Pointer to RTL_USER_PROCESS_PARAMETERS
        [System.Runtime.InteropServices.MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.FunctionPtr, SizeConst = 3)]
        public IntPtr[] Reserved4; // 3 pointers
        public IntPtr AtlThunkSListPtr; // Pointer
        public IntPtr Reserved5; // Pointer
        public uint Reserved6;
        public IntPtr Reserved7; // Pointer
        public uint Reserved8;
        public uint AtlThunkSListPtr32;
        [System.Runtime.InteropServices.MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.FunctionPtr, SizeConst = 45)]
        public IntPtr[] Reserved9; // 45 pointers
        public byte[] Reserved10; // 96 bytes
        public IntPtr PostProcessInitRoutine; // Pointer to PPS_POST_PROCESS_INIT_ROUTINE
        public byte[] Reserved11; // 128 bytes
        public IntPtr[] Reserved12; // 1 pointer
        public uint SessionId;

        public PEB()
        {
            Reserved1 = new byte[2];
            BeingDebugged = 0;
            Reserved2 = new byte[1];
            Reserved3 = new IntPtr[2];
            Ldr = IntPtr.Zero;
            ProcessParameters = IntPtr.Zero;
            Reserved4 = new IntPtr[3];
            AtlThunkSListPtr = IntPtr.Zero;
            Reserved5 = IntPtr.Zero;
            Reserved6 = 0;
            Reserved7 = IntPtr.Zero;
            Reserved8 = 0;
            AtlThunkSListPtr32 = 0;
            Reserved9 = new IntPtr[45];
            Reserved10 = new byte[96];
            PostProcessInitRoutine = IntPtr.Zero;
            Reserved11 = new byte[128];
            Reserved12 = new IntPtr[1];
            SessionId = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct PROCESS_BASIC_INFORMATION
    {
        public int ExitStatus; // NTSTATUS is typically represented as an int in C#
        public IntPtr PebBaseAddress; // PEB is a pointer, represented as IntPtr in C#
        public ulong AffinityMask; // ULONG_PTR is typically represented as ulong in C#
        public int BasePriority; // LONG is typically represented as int in C#
        public ulong UniqueProcessId; // ULONG_PTR is typically represented as ulong in C#
        public ulong InheritedFromUniqueProcessId; // ULONG_PTR is typically represented as ulong in C#
    }


}