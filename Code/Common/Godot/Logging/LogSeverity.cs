// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

namespace EHE.Common.Godot.Logging
{
    /// <summary>
    ///  Contains keys for different levels of severity.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        ///  General information.
        /// </summary>
        Info,

        /// <summary>
        ///  Errors that can be easily recovered from.
        /// </summary>
        Warning,

        /// <summary>
        ///  Errors that are more critical than <see cref="Warning"/>s but
        ///  don't lead to a system crash.
        /// </summary>
        Error,

        /// <summary>
        ///  Errors that cannot be recovered from. The system should probably
        ///  bring itself down after this in a managed way.
        /// </summary>
        Fatal,

        /// <summary>
        ///  Only used for debugging purposes. Logs with this severity rating
        ///  are not written to the log file when running a release build of
        ///  the game.
        /// </summary>
        Debug,
    }
}
