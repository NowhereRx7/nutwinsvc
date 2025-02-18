using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace nutupsdrv
{
    internal static class Extensions
    {
        public static T ReadMemory<T>(this IntPtr atAddress)
        {
            var ret = (T)Marshal.PtrToStructure(atAddress, typeof(T));
            return ret;
        }
    }
}
