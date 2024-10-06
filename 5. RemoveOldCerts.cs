using BSS.Logging;
using Renci.SshNet;
using System;

namespace CertBotHelper
{
    internal static partial class Program
    {
        private static Boolean RemoveDownloadedCertificatesFromRemoteLocation(SftpClient sftpClient, ref Configuration configuration)
        {
            try
            {
                sftpClient.DeleteFile(configuration.RemoteCertBotPath + "cert.crt");
                sftpClient.DeleteFile(configuration.RemoteCertBotPath + "chain.crt");
                sftpClient.DeleteFile(configuration.RemoteCertBotPath + "fullChain.crt");
                Log.FastLog($"Successfully removed all three certificates from the server", LogSeverity.Info, "Delete");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst removing the certificates from the server\n{exception.Message}", LogSeverity.Critical, "Delete");
                return false;
            }

            return true;
        }
    }
}