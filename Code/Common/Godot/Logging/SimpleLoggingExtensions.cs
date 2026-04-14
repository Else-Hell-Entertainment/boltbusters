// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System;
using System.Diagnostics;
using Godot;

namespace EHE.Common.Godot.Logging
{
    /// <summary>
    ///  <para>
    ///   This class contains logging-related extension methods that can be used
    ///   by any <see cref="object"/>. The purpose of these methods is to
    ///   provide a standardized way to write log messages to the console and
    ///   the log file in both production and debugging releases.
    ///  </para>
    ///  <para>
    ///   The class contains methods for logging the following types of
    ///   information: info, warning, error, fatal error, and debug. The
    ///   methods for warning, error, and fatal error have two (2) variants:
    ///   one that includes the <see cref="StackTrace"/> in the output and one
    ///   that doesn't. It is advised to use the non stack trace variants
    ///   sparingly as some critical information might not be included.
    ///  </para>
    /// </summary>
    ///
    /// <seealso cref="LogSeverity"/>
    /// <seealso cref="LoggingConfig"/>
    public static class SimpleLoggingExtensions
    {
        #region Public API
        // MARK: Public API

        #region Normal
        // MARK: Normal

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Info"/>
        ///  severity. Use this for general log messages.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log message.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogDebug"/>
        /// <seealso cref="LogWarning"/>
        /// <seealso cref="LogError"/>
        /// <seealso cref="LogFatalError"/>
        public static void LogInfo(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Info, includeStackTrace: false, what);

        /// <summary>
        ///  Logs a debug message to the console. Only functional in DEBUG
        ///  builds!
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log a debug message.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogInfo"/>
        /// <seealso cref="LogWarning"/>
        /// <seealso cref="LogError"/>
        /// <seealso cref="LogFatalError"/>
        public static void LogDebug(this object obj, params object[] what)
        {
#if DEBUG
            obj.Log(LogSeverity.Debug, includeStackTrace: false, what);
#endif
        }

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Warning"/>
        ///  severity. The <see cref="StackTrace"/> will be included.
        ///  Use this for errors that can be easily recovered from.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log the warning.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogWarningNoStackTrace"/>
        public static void LogWarning(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Warning, includeStackTrace: true, what);

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Error"/>
        ///  severity. The <see cref="StackTrace"/> will be included.
        ///  Use this for errors that are more severe.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log the error.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogErrorNoStackTrace"/>
        public static void LogError(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Error, includeStackTrace: true, what);

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Fatal"/>
        ///  severity. The <see cref="StackTrace"/> will be included.
        ///  Use this for errors that cannot be recovered from.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log the fatal error.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogFatalErrorNoStackTrace"/>
        public static void LogFatalError(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Fatal, includeStackTrace: true, what);

        #endregion Normal


        #region Variants with StackTrace Omitted
        // MARK: StackTrace Omitted

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Warning"/>
        ///  severity. The <see cref="StackTrace"/> will NOT be included.
        ///  Use this with caution for errors that can be easily recovered from.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log the warning.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogWarning"/>
        public static void LogWarningNoStackTrace(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Warning, includeStackTrace: false, what);

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Error"/>
        ///  severity. The <see cref="StackTrace"/> will NOT be included.
        ///  Use this with caution for errors that are more severe.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log the error.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogError"/>
        public static void LogErrorNoStackTrace(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Error, includeStackTrace: false, what);

        /// <summary>
        ///  Writes a message to the log using <see cref="LogSeverity.Fatal"/>
        ///  severity. The <see cref="StackTrace"/> will NOT be included.
        ///  Use this with caution for errors that cannot be recovered from.
        /// </summary>
        ///
        /// <param name="obj">The object that wants to log the fatal error.</param>
        /// <param name="what">Arguments that are written to the log.</param>
        ///
        /// <seealso cref="LogFatalError"/>
        public static void LogFatalErrorNoStackTrace(this object obj, params object[] what) =>
            obj.Log(LogSeverity.Fatal, includeStackTrace: false, what);

        #endregion No StackTrace Variants

        #endregion Public API


        #region Private Implementations
        // MARK: Private Implementations

        /// <summary>
        ///  Returns the stack trace as a string.
        /// </summary>
        private static string GetStackTrace()
        {
            // Skip 4 frames since there are 3 calls before this one.
            var stackTrace = new StackTrace(4, true);

            if (stackTrace.FrameCount == 0)
            {
                return string.Empty;
            }

            var indent = LoggingConfig.STACK_TRACE_INDENT;
            var stackTraceText = $"\n{indent}Stack trace (most recent call first):";

            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var stackFrame = stackTrace.GetFrame(i);

                if (stackFrame == null)
                {
                    continue;
                }

                var stackFrameText = string.Empty;
                var method = stackFrame.GetMethod();

                if (method != null)
                {
                    var fullMethodName = $"{method.DeclaringType?.FullName}.{method.Name}";

                    var parameters = method.GetParameters();
                    var parameterList = string.Join(
                        ", ",
                        Array.ConvertAll(parameters, p => $"{p.ParameterType.FullName}")
                    );

                    stackFrameText = $"\n{indent}{indent}at {fullMethodName}({parameterList})";
                }

#if DEBUG
                // Only include file info if in debug mode as this is info is
                // not available in release builds.
                var line = stackFrame.GetFileLineNumber();
                var column = stackFrame.GetFileColumnNumber();
                var file = stackFrame.GetFileName();
                stackFrameText += $"\n{indent}{indent}   {file} [{line}:{column}]";
#endif

                stackTraceText += stackFrameText;
            }

            stackTraceText += $"\n{indent}End of stack trace.";
            return stackTraceText;
        }

        /// <summary>
        ///  Forms the log message that will be written using the format
        ///  <c>[date &amp; time] [severity] [sender name]: message</c>
        /// </summary>
        ///
        /// <param name="sender">
        ///  Type (or name) of the object that wants to write something to the
        ///  log.
        /// </param>
        /// <param name="severity">Severity of the information.</param>
        /// <param name="includeStackTrace">
        ///  Whether to include the <see cref="StackTrace"/> in the output or
        ///  not.
        /// </param>
        /// <param name="what">What to include in the message.</param>
        ///
        /// <returns>
        ///  A single string containing all the given information.
        /// </returns>
        private static string ComposeLogMessage(
            string sender,
            LogSeverity severity,
            bool includeStackTrace,
            params object[] what
        )
        {
            var message =
                $"[{DateTime.Now}] [{severity.ToString().ToUpper()}] [{sender}]" + $": {string.Join(" ", what).Trim()}";

            if (includeStackTrace)
            {
                message += GetStackTrace();
            }

            return message;
        }

        /// <summary>
        ///  Prints the given message in the given color.
        /// </summary>
        ///
        /// <param name="message">Message text to print.</param>
        /// <param name="color">
        ///  Color to use for the message text. See the list of available
        ///  color names on Wikipedia.org:
        ///  <see href="https://en.wikipedia.org/wiki/Web_colors#HTML_color_names"/>
        /// </param>
        private static void PrintColored(string message, string color)
        {
            GD.PrintRich($"[color={color}]{message}[/color]");
        }

        /// <summary>
        ///  Log a message to the console and the log file using the given
        ///  severity. If no severity is provided, uses
        ///  <see cref="LogSeverity.Info"/> by default.
        /// </summary>
        ///
        /// <param name="sender">The object that wants to log a message.</param>
        /// <param name="includeStackTrace">
        ///  Whether to include the <see cref="StackTrace"/> in the output or
        ///  not.
        /// </param>
        /// <param name="what">What to write in the log.</param>
        /// <param name="severity">
        ///  Severity of the log message. For all severity labels,
        ///  see <see cref="LogSeverity"/>.
        /// </param>
        private static void Log(this object sender, LogSeverity severity, bool includeStackTrace, params object[] what)
        {
            string senderName;

            if (sender is Node node)
            {
                senderName = node.Name;
            }
            else
            {
                senderName = sender.GetType().ToString();
            }

            var logMessage = ComposeLogMessage(senderName, severity, includeStackTrace, what);

            switch (severity)
            {
                case LogSeverity.Info:
                    PrintColored(logMessage, LoggingConfig.INFO_COLOR);
                    break;
                case LogSeverity.Warning:
                    PrintColored(logMessage, LoggingConfig.WARNING_COLOR);
                    break;
                case LogSeverity.Error:
                case LogSeverity.Fatal:
                    PrintColored(logMessage, LoggingConfig.ERROR_COLOR);
                    break;
                case LogSeverity.Debug:
                    PrintColored(logMessage, LoggingConfig.DEBUG_COLOR);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }

        #endregion Private Implementations
    }
}
