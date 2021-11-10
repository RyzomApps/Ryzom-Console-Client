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
    public class IllegalPluginAccessException : Exception
    {
        public IllegalPluginAccessException() { }

        public IllegalPluginAccessException(string message) : base(message) { }

        public IllegalPluginAccessException(string message, Exception innerException) : base(message, innerException) { }

        protected IllegalPluginAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}