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
    public class InvalidDescriptionException : Exception
    {
        public InvalidDescriptionException(Exception fileNotFoundException) : base(null, fileNotFoundException) { }

        public InvalidDescriptionException(string message) : base(message) { }

        public InvalidDescriptionException(Exception exception, string message) : base(message, exception) { }

        public InvalidDescriptionException(string message, Exception innerException) : base(message, innerException) { }

        protected InvalidDescriptionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}