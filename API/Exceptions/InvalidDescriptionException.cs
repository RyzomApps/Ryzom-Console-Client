///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;

namespace API.Exceptions
{
    /// <summary>
    /// Thrown when attempting to load an invalid PluginDescriptionFile
    /// </summary>
    [Serializable]
    public class InvalidDescriptionException : Exception
    {
        /// <summary>
        /// Constructs a new InvalidDescriptionException based on the given Exception
        /// </summary>
        /// <param name="exception">Exception that triggered this Exception</param>
        /// <param name="message">Brief message explaining the cause of the exception</param>
        public InvalidDescriptionException(Exception exception, string message) : base(message, exception) { }

        /// <summary>
        /// Constructs a new InvalidDescriptionException based on the given Exception
        /// </summary>
        /// <param name="cause">Exception that triggered this Exception</param>
        public InvalidDescriptionException(Exception cause) : base(null, cause) { }

        /// <summary>
        /// Constructs a new InvalidDescriptionException with the given message
        /// </summary>
        /// <param name="message">Brief message explaining the cause of the exception</param>
        public InvalidDescriptionException(string message) : base(message) { }
    }
}