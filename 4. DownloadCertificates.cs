using BSS.Logging;
using Renci.SshNet;
using System;
using System.IO;

namespace CertBotHelper
{
    internal static partial class Program
    {
        private static Boolean DownloadCertificates(SftpClient sftpClient, ref Configuration configuration)
        {
            try
            {
                using (FileStream fileStream = File.Open(Path.Combine(configuration.LocalCertificateOutputPath, "cert.crt"), FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    sftpClient.DownloadFile(configuration.RemoteCertBotPath + "cert.crt", fileStream);
                }
                Log.FastLog($"Placed cert.crt in: " + Path.Combine(configuration.LocalCertificateOutputPath, "cert.crt"), LogSeverity.Info, "Download");

                using (FileStream fileStream = File.Open(Path.Combine(configuration.LocalCertificateOutputPath, "chain.crt"), FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    sftpClient.DownloadFile(configuration.RemoteCertBotPath + "chain.crt", fileStream);
                }
                Log.FastLog($"Placed cert.crt in: " + Path.Combine(configuration.LocalCertificateOutputPath, "chain.crt"), LogSeverity.Info, "Download");

                using (FileStream fileStream = File.Open(Path.Combine(configuration.LocalCertificateOutputPath, "fullChain.crt"), FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    sftpClient.DownloadFile(configuration.RemoteCertBotPath + "fullChain.crt", fileStream);
                }
                Log.FastLog($"Placed cert.crt in: " + Path.Combine(configuration.LocalCertificateOutputPath, "fullChain.crt"), LogSeverity.Info, "Download");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst downloading the certificates\n{exception.Message}", LogSeverity.Critical, "Download");
                return false;
            }

            return true;
        }
    }
}