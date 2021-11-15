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
    /// Thrown when attempting to load an invalid Plugin file
    /// </summary>
    [Serializable]
    public class InvalidPluginException : Exception
    {
        /// <summary>
        /// Constructs a new InvalidPluginException based on the given Exception
        /// </summary>
        /// <param name="cause">Exception that triggered this Exception</param>
        public InvalidPluginException(Exception cause) : base(null, cause) { }

        /// <summary>
        /// Constructs a new InvalidPluginException with the specified detail message
        /// </summary>
        /// <param name="message">The detail message is saved for later retrieval.</param>
        public InvalidPluginException(string message) : base(message) { }
    }
}