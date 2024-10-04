using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BSS.Logging
{
    internal static class xDebug
    {
        [DllImport("kernel32.dll")]
        static extern Boolean AllocConsole();

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
        [Conditional("DEBUG")]
        internal static void Initialize() => AllocConsole();

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
        [Conditional("DEBUG")]
        internal static void Write(Object input) => Console.Write(input);
        [Conditional("DEBUG")]
        internal static void WriteLine(Object input) => Console.WriteLine(input);
    }
}