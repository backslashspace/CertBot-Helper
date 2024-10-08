using BSS.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CertBotHelper
{
    internal static partial class Program
    {
        private static Boolean SignalNginxReload(ref Configuration configuration)
        {
            Log.FastLog("Signaling as: " + Environment.UserName, LogSeverity.Verbose, "Signal");

            Process nginx = new();
            nginx.StartInfo.FileName = configuration.NginxPath;
            nginx.StartInfo.Arguments = "-s reload";
            nginx.StartInfo.WorkingDirectory = Directory.GetParent(configuration.NginxPath).FullName;
            nginx.StartInfo.CreateNoWindow = true;
            nginx.StartInfo.UseShellExecute = false;
            nginx.StartInfo.RedirectStandardError = true;

            try
            {
                nginx.Start();
                Log.FastLog($"Send reload signal to '{configuration.NginxPath}'", LogSeverity.Info, "Signal-NGINX");

                String errOut = nginx.StandardError.ReadToEnd();

                nginx.WaitForExit();
                nginx.Dispose();

                if (errOut != "")
                {
                    Log.FastLog($"An error occurred trying signal a reload to nginx, error was:'\n{errOut}", LogSeverity.Error, "Signal-NGINX");
                    return false;
                }
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred trying to start '{configuration.NginxPath} -s reload'\n{exception.Message}", LogSeverity.Error, "Signal-NGINX");
                nginx.Dispose();
                return false;
            }

            Log.FastLog($"Successfully signaled a reload to nginx (errOut was empty)", LogSeverity.Info, "Signal-NGINX");
            return true;
        }
    }
}