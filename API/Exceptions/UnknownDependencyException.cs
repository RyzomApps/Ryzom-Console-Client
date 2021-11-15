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
    public class UnknownDependencyException : Exception
    {
        /// <summary>
        /// Constructs a new UnknownDependencyException
        /// </summary>
        public UnknownDependencyException() { }

        /// <summary>
        /// Constructs a new UnknownDependencyException with the given message
        /// </summary>
        /// <param name="message">Brief message explaining the cause of the exception</param>
        public UnknownDependencyException(string message) : base(message) { }
    }
}