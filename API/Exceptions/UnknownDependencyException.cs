///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace API.Exceptions
{
    [Serializable]
    public class UnknownDependencyException : Exception
    {
        public UnknownDependencyException() { }

        public UnknownDependencyException(string message) : base(message) { }

        public UnknownDependencyException(string message, Exception innerException) : base(message, innerException) { }

        protected UnknownDependencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}