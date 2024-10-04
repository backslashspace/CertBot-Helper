using System;
using System.IO;
using System.Reflection;
using System.Text;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace BSS.Logging
{
    internal static class Log
    {
        private static Boolean _isInitialized = false;
        private static String _assemblyPath;
        private static readonly Object _fileLock = new();

        internal static void Initialize(String assemblyPath = null)
        {
            if (_isInitialized) return;

            _assemblyPath = assemblyPath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (Directory.Exists($"{_assemblyPath}\\logs"))
            {
                DateTime now = DateTime.Now;

                if (File.Exists($"{_assemblyPath}\\logs\\{now:dd.MM.yyyy}.txt"))
                {
                    using (StreamWriter streamWriter = new($"{_assemblyPath}\\logs\\{now:dd.MM.yyyy}.txt", true, Encoding.UTF8))
                    {
                        streamWriter.WriteLine();
                    }
                }
            }

            _isInitialized = true;
        }

        // #######################################################################################

        internal static void FastLog(String message, LogSeverity severity, String source)
        {
            DateTime now = DateTime.Now;

            LogMessage logMessage;
            logMessage.Message = message;
            logMessage.Severity = severity;
            logMessage.Source = source;

            WriteFile(ref logMessage, ref now);
        }

        // # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static void WriteFile(ref readonly LogMessage formattedLogMessage, ref readonly DateTime timeStamp)
        {
            if (!_isInitialized) throw new MethodAccessException("Logging class not initialized");

            if (!Directory.Exists($"{_assemblyPath}\\logs"))
            {
                Directory.CreateDirectory($"{_assemblyPath}\\logs");
            }

            String logLine = "";
            UInt16 lineLength = 27;

            logLine += $"[{timeStamp:dd.MM.yyyy HH:mm:ss}] [";

            switch (formattedLogMessage.Severity)
            {
                case LogSeverity.Info:
                    lineLength += 4;
                    logLine += "Info";
                    break;
                case LogSeverity.Debug:
                    lineLength += 5;
                    logLine += "Debug";
                    break;
                case LogSeverity.Warning:
                    lineLength += 7;
                    logLine += "Warning";
                    break;
                case LogSeverity.Verbose:
                    lineLength += 7;
                    logLine += "Verbose";
                    break;
                case LogSeverity.Error:
                    lineLength += 5;
                    logLine += "Error";
                    break;
                case LogSeverity.Critical:
                    lineLength += 8;
                    logLine += "Critical";
                    break;
                case LogSeverity.Alert:
                    lineLength += 5;
                    logLine += "Alert";
                    break;
            }

            logLine += $"]-[{formattedLogMessage.Source}]";

            lineLength += (UInt16)formattedLogMessage.Source.Length;

            if (lineLength < 52)
            {
                for (UInt16 i = lineLength; i < 52; ++i)
                {
                    logLine += " ";
                }
            }
            else
            {
                logLine += " ";
            }

            logLine += formattedLogMessage.Message;

            //

            try
            {
                lock (_fileLock)
                {
                    xDebug.WriteLine(logLine);

                    using (StreamWriter streamWriter = new($"{_assemblyPath}\\logs\\{timeStamp:dd.MM.yyyy}.txt", true, Encoding.UTF8))
                    {
                        streamWriter.WriteLine(logLine);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FieldAccessException($"Unable to write log file to:\n{_assemblyPath}\\logs\\{timeStamp:dd.MM.yyyy}.txt\n\nError: {ex.Message}");
            }
        }
    }
}