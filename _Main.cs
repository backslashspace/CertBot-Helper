using BSS.Logging;
using Renci.SshNet;
using System;

namespace CertBotHelper
{
    internal static partial class Program
    {
        private static void Main(String[] args)
        {
            String assemblyPath = Log.Initialize();

            if (!LoadConfig(assemblyPath, out Configuration configuration))
            {
                Environment.Exit(-1);
            }

            //

            if (!ConnectSftp(ref configuration, out SftpClient sftpClient))
            {
                Environment.Exit(-1);
            }

            if (!PerformCertificateRenewal(sftpClient, ref configuration))
            {
                sftpClient.Dispose();
                Environment.Exit(-1);
            }

            if (!DownloadCertificates(sftpClient, ref configuration))
            {
                sftpClient.Dispose();
                Environment.Exit(-1);
            }

            if (!RemoveDownloadedCertificatesFromRemoteLocation(sftpClient, ref configuration))
            {
                sftpClient.Dispose();
                Environment.Exit(-1);
            }

            try
            {
                sftpClient.Disconnect();
                sftpClient.Dispose();
                Log.FastLog("Disconnected from server", LogSeverity.Info, "SftpClient");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred disconnecting from the server\n{exception.Message}", LogSeverity.Warning, "SftpClient");
                sftpClient.Dispose();
            }

            if (!SignalNginxReload(ref configuration))
            {
                Environment.Exit(-1);
            }

            Log.FastLog($"All done", LogSeverity.Info, "Main");
        }
    }
}