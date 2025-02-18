global using unsafe PUNICODE_STRING = nutupsdrv.UNICODE_STRING*;

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace nutupsdrv
{

    public unsafe struct LDR_MODULE
    {
        LIST_ENTRY InLoadOrderModuleList;
        LIST_ENTRY InMemoryOrderModuleList;
        LIST_ENTRY InInitializationOrderModuleList;
        PVOID BaseAddress;
        PVOID EntryPoint;
        ULONG SizeOfImage;
        UNICODE_STRING FullDllName;
        UNICODE_STRING BaseDllName;
        ULONG Flags;
        SHORT LoadCount;
        SHORT TlsIndex;
        LIST_ENTRY HashTableEntry;
        ULONG TimeDateStamp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct LIST_ENTRY
    {
        public IntPtr Flink;
        public IntPtr Blink;

        public ListEntryWrapper Fwd
        {
            get
            {
                var fwdAddr = Flink.ToInt32();
                return new ListEntryWrapper()
                {
                    Header = Flink.ReadMemory<LIST_ENTRY>(),
                    Body = new IntPtr(fwdAddr + Marshal.SizeOf(typeof(LIST_ENTRY))).ReadMemory<LDR_MODULE>()
                };
            }
        }
        public ListEntryWrapper Back
        {
            get
            {
                var fwdAddr = Blink.ToInt32();
                return new ListEntryWrapper()
                {
                    Header = Flink.ReadMemory<LIST_ENTRY>(),
                    Body = new IntPtr(fwdAddr + Marshal.SizeOf(typeof(LIST_ENTRY))).ReadMemory<LDR_MODULE>()
                };
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ListEntryWrapper
    {
        public LIST_ENTRY Header;
        public LDR_MODULE Body;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING : IDisposable
    {
        public ushort Length;
        public ushort MaximumLength;
        private IntPtr buffer;

        public UNICODE_STRING(string s)
        {
            Length = (ushort)(s.Length * 2);
            MaximumLength = (ushort)(Length + 2);
            buffer = Marshal.StringToHGlobalUni(s);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(buffer);
            buffer = IntPtr.Zero;
        }

        public override string ToString()
        {
            return Marshal.PtrToStringUni(buffer);
        }
    }

}
