using BSS.Logging;
using Renci.SshNet;
using System;

#pragma warning disable CS8625

namespace CertBotHelper
{
    internal static partial class Program
    {
        private static Boolean ConnectSftp(ref Configuration configuration, out SftpClient sftpClient)
        {
            try
            {
                sftpClient = new(configuration.CertBotIP, configuration.SSHUsername, new PrivateKeyFile(configuration.SSHPrivateKeyPath));
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to create new SftpClient, invalid private key?\n{exception.Message}", LogSeverity.Critical, "SftpClient");
                sftpClient = null;
                return false;
            }

            try
            {
                sftpClient.Connect();
                Log.FastLog($"Connected to " + configuration.CertBotIP, LogSeverity.Info, "SftpClient");
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to connect to {configuration.CertBotIP}\n{exception.Message}", LogSeverity.Critical, "SftpClient");
                return false;
            }

            return true;
        }
    }
}