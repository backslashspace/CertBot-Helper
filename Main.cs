using BSS.Logging;
using Renci.SshNet;
using System;
using System.Diagnostics;
using System.IO;

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
        private static void Main(String[] args)
        {
            Log.Initialize();

            ValidateInput(args);

            SftpClient sftpClient = null;

            try
            {
                sftpClient = new(args[0], args[1], new PrivateKeyFile(args[2]));
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to SftpClient, invalid private key?\n{exception.Message}", LogSeverity.Critical, "SftpClient");
                Environment.Exit(-1);
            }

            try
            {
                sftpClient.Connect();
                Log.FastLog($"Connected to " + args[0], LogSeverity.Critical, "Connect()");
            }
            catch (Exception exception)
            {
                Log.FastLog($"Unable to connect to {args[0]}\n{exception.Message}", LogSeverity.Critical, "Connect()");
                Environment.Exit(-1);
            }

            try
            {
                Boolean filesArePresent = true;

                if (!sftpClient.Exists($"/home/{args[3]}/CertBot/cert.crt")) filesArePresent = false;
                if (!sftpClient.Exists($"/home/{args[3]}/CertBot/chain.crt")) filesArePresent = false;
                if (!sftpClient.Exists($"/home/{args[3]}/CertBot/fullChain.crt")) filesArePresent = false;

                if (!filesArePresent)
                {
                    Console.WriteLine();
                    Log.FastLog("Not all certificates were found!", LogSeverity.Error, "PreCheck");
                    sftpClient.Dispose();
                    Environment.Exit(-2);
                }

                Log.FastLog("All certificates present on server, proceeding with download", LogSeverity.Info, "PreCheck");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst checking if all certificates are present on the server\n{exception.Message}", LogSeverity.Error, "PreCheck");
                sftpClient.Dispose();
                Environment.Exit(-1);
            }

            try
            {
                using (FileStream fileStream = File.Open(Path.Combine(args[4] + "\\cert.crt"), FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    sftpClient.DownloadFile($"/home/{args[3]}/CertBot/cert.crt", fileStream);
                }
                Log.FastLog($"Placed cert.crt in: " + Path.Combine(args[4] + "\\cert.crt"), LogSeverity.Info, "Download");

                using (FileStream fileStream = File.Open(Path.Combine(args[4] + "\\chain.crt"), FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    sftpClient.DownloadFile($"/home/{args[3]}/CertBot/chain.crt", fileStream);
                }
                Log.FastLog($"Placed cert.crt in: " + Path.Combine(args[4] + "\\chain.crt"), LogSeverity.Info, "Download");

                using (FileStream fileStream = File.Open(Path.Combine(args[4] + "\\fullChain.crt"), FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    sftpClient.DownloadFile($"/home/{args[3]}/CertBot/fullChain.crt", fileStream);
                }
                Log.FastLog($"Placed cert.crt in: " + Path.Combine(args[4] + "\\fullChain.crt"), LogSeverity.Info, "Download");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst downloading the certificates\n{exception.Message}", LogSeverity.Critical, "Download");
                sftpClient.Dispose();
                Environment.Exit(-1);
            }

            try
            {
                sftpClient.DeleteFile($"/home/{args[3]}/CertBot/cert.crt");
                sftpClient.DeleteFile($"/home/{args[3]}/CertBot/chain.crt");
                sftpClient.DeleteFile($"/home/{args[3]}/CertBot/fullChain.crt");
                Log.FastLog($"Successfully cleaned all three certificates from the server", LogSeverity.Info, "Delete");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred whilst removing the certificates from the server\n{exception.Message}", LogSeverity.Critical, "Delete");
                sftpClient.Dispose();
                Environment.Exit(-1);
            }

            try
            {
                sftpClient.Disconnect();
                sftpClient.Dispose();
                Log.FastLog("Disconnected from server", LogSeverity.Info, "Disconnect()");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred disconnecting from the server\n{exception.Message}", LogSeverity.Warning, "Disconnect()");
                sftpClient.Dispose();
            }

            Process nginx = new();
            nginx.StartInfo.FileName = args[5];
            nginx.StartInfo.Arguments = "-s reload";
            nginx.StartInfo.WorkingDirectory = Directory.GetParent(args[5]).FullName;
            nginx.StartInfo.CreateNoWindow = true;
            nginx.StartInfo.UseShellExecute = false;

            try
            {
                nginx.Start();
                Log.FastLog($"Send reload signal to '{args[5]}'", LogSeverity.Info, "Signal-NGINX");
            }
            catch (Exception exception)
            {
                Log.FastLog($"An error occurred trying to start '{args[5]} -s reload'\n{exception.Message}", LogSeverity.Error, "Signal-NGINX");
                Environment.Exit(-1);
            }

            Log.FastLog($"Done", LogSeverity.Info, "Main");
        }
    }
}