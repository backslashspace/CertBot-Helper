using BSS.Logging;
using Renci.SshNet;
using System;

namespace CertBotHelper
{
    internal static partial class Program
    {
        private static Boolean PerformCertificateRenewal(SftpClient sftpClient, ref Configuration configuration)
        {
            #region Prepare and Connect
            try
            {
                if (sftpClient.Exists(configuration.RemoteCertBotPath + "cert.crt"))
                {
                    Log.FastLog("Found cert.crt in remote CertBot directory, removing.", LogSeverity.Info, "PreRenewal");
                    sftpClient.DeleteFile(configuration.RemoteCertBotPath + "cert.crt");
                }

                if (sftpClient.Exists(configuration.RemoteCertBotPath + "chain.crt"))
                {
                    Log.FastLog("Found chain.crt in remote CertBot directory, removing.", LogSeverity.Info, "PreRenewal");
                    sftpClient.DeleteFile(configuration.RemoteCertBotPath + "chain.crt");
                }

                if (sftpClient.Exists(configuration.RemoteCertBotPath + "fullChain.crt"))
                {
                    Log.FastLog("Found fullChain.crt in remote CertBot directory, removing.", LogSeverity.Info, "PreRenewal");
                    sftpClient.DeleteFile(configuration.RemoteCertBotPath + "fullChain.crt");
                }
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst preparing the remote certificate directory\n{exception.Message}", LogSeverity.Error, "PreCheck");
                return false;
            }

            SshClient sshClient;

            try
            {
                sshClient = new(configuration.CertBotIP, configuration.SSHUsername, new PrivateKeyFile(configuration.SSHPrivateKeyPath));
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to create new SshClient, invalid private key?\n{exception.Message}", LogSeverity.Critical, "SshClient");
                return false;
            }

            try
            {
                sshClient.Connect();
                Log.FastLog($"Connected to " + configuration.CertBotIP, LogSeverity.Info, "SshClient");
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to connect to {configuration.CertBotIP}\n{exception.Message}", LogSeverity.Critical, "SshClient");
                return false;
            }
            #endregion

            String stdOut;
            String errOut;

            #region Run Command
            try
            {
                Log.FastLog("Attempting to run renewal command on machine", LogSeverity.Info, "CertRenewal");
                SshCommand sshCommand = sshClient.CreateCommand(configuration.RenewalCommand);
                stdOut = sshCommand.Execute();
                errOut = sshCommand.Error;

                sshCommand.Dispose();
                sshClient.Disconnect();
                sshClient.Dispose();
            }
            catch (Exception exception)
            {
                Log.FastLog($"Failed to run renewal command on remote machine\n{exception.Message}", LogSeverity.Critical, "CertRenewal");
                return false;
            }
            #endregion

            #region Validate after Command
            try
            {
                Boolean filesArePresent = true;

                if (!sftpClient.Exists(configuration.RemoteCertBotPath + "cert.crt")) filesArePresent = false;
                if (!sftpClient.Exists(configuration.RemoteCertBotPath + "chain.crt")) filesArePresent = false;
                if (!sftpClient.Exists(configuration.RemoteCertBotPath + "fullChain.crt")) filesArePresent = false;

                if (!filesArePresent)
                {
                    Log.FastLog("Not all certificates were found! CertBot errOut output was:\n" + errOut, LogSeverity.Error, "PostRenewCheck");
                    
                    return false;
                }

                Log.FastLog("All certificates present on server after renewal, proceeding with download", LogSeverity.Info, "PostRenewCheck");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst checking if all certificates are present on the server\n{exception.Message}", LogSeverity.Error, "PostRenewCheck");
                return false;
            }
            #endregion

            return true;
        }
    }
}