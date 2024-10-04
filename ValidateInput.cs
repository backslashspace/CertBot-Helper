using BSS.Logging;
using System;
using System.IO;
using System.Net;

namespace SSHFile
{
    internal static partial class Program
    {
        // [0] = CertBot IP
        // [1] = SSH username
        // [2] = SSH privateKey (no password)
        // [3] = CertBot username
        // [4] = certificate destination path
        // [5] = nginx.exe path
        private static void ValidateInput(String[] args)
        {
            if (args.Length < 5)
            {
                Log.FastLog("Invalid args[], use the following format:\n\n" +
                    "arg[0] = CertBot IP\n" +
                    "arg[1] = SSH Username\n" +
                    "arg[2] = SSH PrivateKey Path (no password)\n" +
                    "arg[3] = CertBot Username\n" +
                    "arg[4] = Local Certificate Output Path\n" +
                    "arg[5] = nginx.exe path (no white spaces allowed) - will automatically signal a reload\n\n" +
                    "sample: 10.0.1.123 user CertBot_Key user C:\\nginx C:\\nginx\\nginx.exe", LogSeverity.Error, "Initialization");

                Environment.Exit(-1);
            }

            if (!IPAddress.TryParse(args[0], out _))
            {
                Log.FastLog("Invalid IP: " + args[0], LogSeverity.Error, "Initialization");
                Environment.Exit(-1);
            }

            if (!File.Exists(args[2]))
            {
                Log.FastLog($"Private key file not found: path was '{args[2]}'", LogSeverity.Error, "Initialization");
                Environment.Exit(-1);
            }

            if (!Directory.Exists(args[4]))
            {
                Log.FastLog($"Certificate output path not found: path was '{args[4]}'", LogSeverity.Error, "Initialization");
                Environment.Exit(-1);
            }

            if (!File.Exists(args[5]))
            {
                Log.FastLog($"nginx.exe not found: path was '{args[5]}'", LogSeverity.Error, "Initialization");
                Environment.Exit(-1);
            }
        }
    }
}