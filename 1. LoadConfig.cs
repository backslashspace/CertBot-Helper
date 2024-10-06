using BSS.Logging;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

#pragma warning disable CS8600
#pragma warning disable CS8625

namespace CertBotHelper
{
    internal static partial class Program
    {
        private const String REGEX_CERTBOT_IP = "^CertBot-IP\t*=(.+)$";
        private const String REGEX_SSH_USERNAME = "^SSH-Username\t*=(.+)$";
        private const String REGEX_SSH_PKEY_PATH = "^SSH-PrivateKeyPath\t*=(.+)$";
        private const String REGEX_REMOTE_CERTBOT_PATH = "^RemoteCertBotDirectory\t*=(.+)$";
        private const String REGEX_LOCAL_CERT_OUT_PATH = "^LocalCertificateDestinationPath\t*=(.+)$";
        private const String REGEX_NGINX_EXE_PATH = "^PathToNginx\\.exe\t*=(.+)$";
        private const String REGEX_RENEWAL_COMMAND = "^CertBotRenewalCommand\t*=(.+)$";

        internal ref struct Configuration
        {
            internal String CertBotIP;
            internal String SSHUsername;
            internal String SSHPrivateKeyPath;
            internal String RemoteCertBotPath;
            internal String LocalCertificateOutputPath;
            internal String NginxPath;

            internal String RenewalCommand;
        }

        private static Boolean LoadConfig(String assemblyPath, out Configuration configuration)
        {
            configuration.CertBotIP = null;
            configuration.SSHUsername = null;
            configuration.SSHPrivateKeyPath = null;
            configuration.RemoteCertBotPath = null;
            configuration.LocalCertificateOutputPath = null;
            configuration.NginxPath = null;
            configuration.RenewalCommand = null;

            if (!File.Exists(assemblyPath + "\\config.txt"))
            {
                try
                {
                    File.WriteAllText(assemblyPath + "\\config.txt", "CertBot-IP\t\t\t\t\t\t=10.0.1.122\r\nSSH-Username\t\t\t\t\t=user\r\nSSH-PrivateKeyPath\t\t\t\t=C:\\nginx\\CertBot-Helper\\CertBot_Key\r\nRemoteCertBotDirectory\t\t\t=/home/user/CertBot/\r\nLocalCertificateDestinationPath\t=C:\\nginx\\\r\nPathToNginx.exe\t\t\t\t\t=C:\\nginx\\nginx.exe\r\n\r\nCertBotRenewalCommand\t\t\t=certbot certonly --authenticator standalone --csr /home/user/CertBot/domain.csr --agree-tos --no-eff-email --http-01-port 8080 --cert-path /home/user/CertBot/cert.crt --fullchain-path /home/user/CertBot/fullChain.crt --chain-path /home/user/CertBot/chain.crt --config-dir /home/user/CertBot/config --work-dir /home/user/CertBot/workingDirectory --logs-dir /home/user/CertBot/logs");
                    Log.FastLog("Config not found, crating template: " + assemblyPath + "\\config.txt", LogSeverity.Alert, "LoadConfig");
                    return false;
                }
                catch (Exception exception)
                {
                    Log.FastLog("Config not found, unable to create template in: " + assemblyPath + "\n" + exception.Message, LogSeverity.Error, "LoadConfig");
                }

                return false;
            }

            String[] configLines = null;

            try
            {
                configLines = File.ReadAllLines(assemblyPath + "\\config.txt");
            }
            catch (Exception exception)
            {
                Log.FastLog("Unable to read config: " + exception.Message, LogSeverity.Error, "LoadConfig");
                return false;
            }

            Int32 configLinesLength = configLines!.Length;

            if (configLinesLength < 7)
            {
                Log.FastLog("Invalid config file, found less than 7 lines", LogSeverity.Error, "LoadConfig");
                return false;
            }

            ParseConfig(configLines, configLinesLength, ref configuration);

            ValidateConfig(ref configuration);

            return true;
        }
        
        private static void ParseConfig(String[] configLines, Int32 configLinesLength, ref Configuration configuration)
        {
            for (Int32 i = 0; i < configLinesLength; ++i)
            {
                Match match;

                if (configuration.CertBotIP == null && (match = Regex.Match(configLines[i], REGEX_CERTBOT_IP)).Success)
                {
                    if (!IPAddress.TryParse(match.Groups[1].Value, out _))
                    {
                        Log.FastLog($"Invalid config value for 'CertBot-IP', value was: '{match.Groups[1].Value}', expected IPAddress", LogSeverity.Error, "ParseConfig");
                        Environment.Exit(-1);
                    }

                    configuration.CertBotIP = match.Groups[1].Value;
                    continue;
                }

                if (configuration.SSHUsername == null && (match = Regex.Match(configLines[i], REGEX_SSH_USERNAME)).Success)
                {
                    configuration.SSHUsername = match.Groups[1].Value;
                    continue;
                }

                if (configuration.SSHPrivateKeyPath == null && (match = Regex.Match(configLines[i], REGEX_SSH_PKEY_PATH)).Success)
                {
                    if (!File.Exists(match.Groups[1].Value))
                    {
                        Log.FastLog($"Invalid config value for 'SSH-PrivateKeyPath', value was: '{match.Groups[1].Value}', file not found", LogSeverity.Error, "ParseConfig");
                        Environment.Exit(-1);
                    }

                    configuration.SSHPrivateKeyPath = match.Groups[1].Value;
                    continue;
                }

                if (configuration.RemoteCertBotPath == null && (match = Regex.Match(configLines[i], REGEX_REMOTE_CERTBOT_PATH)).Success)
                {
                    configuration.RemoteCertBotPath = match.Groups[1].Value;
                    continue;
                }

                if (configuration.LocalCertificateOutputPath == null && (match = Regex.Match(configLines[i], REGEX_LOCAL_CERT_OUT_PATH)).Success)
                {
                    if (!Directory.Exists(match.Groups[1].Value))
                    {
                        Log.FastLog($"Invalid config value for 'LocalCertificateDestinationPath', value was: '{match.Groups[1].Value}', directory not found", LogSeverity.Error, "ParseConfig");
                        Environment.Exit(-1);
                    }

                    configuration.LocalCertificateOutputPath = match.Groups[1].Value;
                    continue;
                }

                if (configuration.NginxPath == null && (match = Regex.Match(configLines[i], REGEX_NGINX_EXE_PATH)).Success)
                {
                    if (!File.Exists(match.Groups[1].Value))
                    {
                        Log.FastLog($"Invalid config value for 'PathToNginx.exe', value was: '{match.Groups[1].Value}', file not found", LogSeverity.Error, "ParseConfig");
                        Environment.Exit(-1);
                    }

                    configuration.NginxPath = match.Groups[1].Value;
                    continue;
                }

                if (configuration.RenewalCommand == null && (match = Regex.Match(configLines[i], REGEX_RENEWAL_COMMAND)).Success)
                {
                    configuration.RenewalCommand = match.Groups[1].Value;
                    continue;
                }
            }
        }

        private static void ValidateConfig(ref Configuration configuration)
        {
            if (configuration.CertBotIP == null)
            {
                Log.FastLog("Invalid config file, 'CertBot-IP' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }

            if (configuration.SSHUsername == null)
            {
                Log.FastLog("Invalid config file, 'SSH-Username' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }

            if (configuration.SSHPrivateKeyPath == null)
            {
                Log.FastLog("Invalid config file, 'SSH-PrivateKeyPath' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }

            if (configuration.RemoteCertBotPath == null)
            {
                Log.FastLog("Invalid config file, 'RemoteCertBotDirectory' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }

            if (configuration.LocalCertificateOutputPath == null)
            {
                Log.FastLog("Invalid config file, 'LocalCertificateDestinationPath' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }

            if (configuration.NginxPath == null)
            {
                Log.FastLog("Invalid config file, 'PathToNginx.exe' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }

            if (configuration.RenewalCommand == null)
            {
                Log.FastLog("Invalid config file, 'CertBotRenewalCommand' not found", LogSeverity.Error, "ParseConfig");
                Environment.Exit(-1);
            }
        }
    }
}