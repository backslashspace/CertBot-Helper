using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006

namespace BSS.Logging
{
    internal static class xDebug
    {
        internal static ConsoleColor DefaultForegroundColor;
        internal static readonly Boolean IsInitialized = false;

        [DllImport("kernel32.dll")]
        static extern Boolean AllocConsole();

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
        static xDebug()
        {
#if DEBUG
            if (Console.LargestWindowWidth == 0) AllocConsole();

            DefaultForegroundColor = Console.ForegroundColor;
            IsInitialized = true;
#endif
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #
        [Conditional("DEBUG")]
        internal static void Write(Object input) => Console.Write(input);
        [Conditional("DEBUG")]
        internal static void WriteLine(Object input) => Console.WriteLine(input);
    }
}