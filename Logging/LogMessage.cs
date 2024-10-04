using System;

namespace BSS.Logging
{
    internal ref struct LogMessage
    {
        internal LogMessage(String message, LogSeverity severity, String source)
        {
            Message = message;
            Severity = severity;
            Source = source;
        }

        internal LogSeverity Severity;

        internal String Source;
        internal String Message;
    }
}
