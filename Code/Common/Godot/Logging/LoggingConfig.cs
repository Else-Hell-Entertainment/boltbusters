// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

namespace EHE.Common.Godot.Logging
{
    /// <summary>
    ///  This class contains the configurations for
    ///  <see cref="SimpleLoggingExtensions"/>.
    /// </summary>
    ///
    /// <seealso cref="LogSeverity"/>
    public static class LoggingConfig
    {
        /// <summary>
        ///  The name of the color to use with info logs.
        /// </summary>
        public const string INFO_COLOR = "white";

        /// <summary>
        ///  The name of the color to use with warning logs.
        /// </summary>
        public const string WARNING_COLOR = "yellow";

        /// <summary>
        ///  The name of the color to use with error logs.
        /// </summary>
        public const string ERROR_COLOR = "red";

        /// <summary>
        ///  The name of the color to use with debug logs.
        /// </summary>
        public const string DEBUG_COLOR = "darkgray";

        /// <summary>
        ///  The indentation for stack trace output.
        /// </summary>
        public const string STACK_TRACE_INDENT = "   ";
    }
}
