using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

#pragma warning disable IDE0079
#pragma warning disable IDE0032
#pragma warning disable IDE1006
#pragma warning disable CS8618
#pragma warning disable CS8600
#pragma warning disable CS8603

namespace BSS.Logging
{
    internal static class Log
    {
        private static Boolean _isInitialized = false;
        internal static Boolean IsInitialized { get => _isInitialized; }

        private static String _assemblyPath;
        private static readonly Object _fileLock = new();

        private const Int32 DEFAULT_PADDING_WIDTH = 52;

        private static Int32 _padding;

        internal static String Initialize()
        {
            if (_isInitialized) return null;

#if DEBUG
            if (!xDebug.IsInitialized) throw new InvalidProgramException();
#endif

            _padding = DEFAULT_PADDING_WIDTH;
            _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

            return _assemblyPath;
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
            if (!Directory.Exists($"{_assemblyPath}\\logs")) Directory.CreateDirectory($"{_assemblyPath}\\logs");

            //

            Int32 lineLength = 27 + formattedLogMessage.Source.Length;

            String source = $"]-[{formattedLogMessage.Source}]";
            String timeStampString = $"[{timeStamp:dd.MM.yyyy HH:mm:ss}] [";

            String severityString = null;
            switch (formattedLogMessage.Severity)
            {
                case LogSeverity.Info:
                    lineLength += 4;
                    severityString += "Info";
                    break;
                case LogSeverity.Debug:
                    lineLength += 5;
                    severityString += "Debug";
                    break;
                case LogSeverity.Warning:
                    lineLength += 7;
                    severityString += "Warning";
                    break;
                case LogSeverity.Verbose:
                    lineLength += 7;
                    severityString += "Verbose";
                    break;
                case LogSeverity.Error:
                    lineLength += 5;
                    severityString += "Error";
                    break;
                case LogSeverity.Critical:
                    lineLength += 8;
                    severityString += "Critical";
                    break;
                case LogSeverity.Alert:
                    lineLength += 5;
                    severityString += "Alert";
                    break;
            }

            String padding;
            if (lineLength < _padding)
            {
                padding = new String(' ', _padding - lineLength);
            }
            else
            {
                padding = " ";
            }

            String logLine = timeStampString + severityString + source + padding + formattedLogMessage.Message;
      
            try
            {
                lock (_fileLock)
                {
                    ColoredDebugPrint(timeStampString, formattedLogMessage.Severity, severityString!, source, padding, formattedLogMessage.Message);

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

        [Conditional("DEBUG")]
        private static void ColoredDebugPrint(String timeStampString, LogSeverity logSeverity, String severityString, String source, String padding, String message)
        {
            ConsoleColor foreground = logSeverity switch
            {
                LogSeverity.Info => ConsoleColor.DarkCyan,
                LogSeverity.Debug => ConsoleColor.DarkGreen,
                LogSeverity.Warning => ConsoleColor.DarkYellow,
                LogSeverity.Verbose => ConsoleColor.Magenta,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Critical => ConsoleColor.DarkRed,
                LogSeverity.Alert => ConsoleColor.Yellow,
                _ => xDebug.DefaultForegroundColor,
            };

            Console.Write(timeStampString);
            Console.ForegroundColor = foreground;
            Console.Write(severityString);
            Console.ForegroundColor = xDebug.DefaultForegroundColor;
            Console.WriteLine(source + padding + message);
        }
    }
}