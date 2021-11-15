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
    /// Thrown when a plugin attempts to interact with the server when it is not enabled
    /// </summary>
    [Serializable]
    public class IllegalPluginAccessException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="IllegalPluginAccessException"/> without detail message.
        /// </summary>
        public IllegalPluginAccessException() { }

        /// <summary>
        /// Constructs an instance of <see cref="IllegalPluginAccessException"/> with the specified detail message.
        /// </summary>
        /// <param name="message">The detail message.</param>
        public IllegalPluginAccessException(string message) : base(message) { }
    }
}