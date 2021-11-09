///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.Serialization;

namespace API.Exceptions
{
    [Serializable]
    public class InvalidPluginException : Exception
    {
        public InvalidPluginException(InvalidDescriptionException invalidDescriptionException) : base(null, invalidDescriptionException) { }

        public InvalidPluginException(FileNotFoundException fileNotFoundException) : base(null, fileNotFoundException) { }

        public InvalidPluginException(string message) : base(message) { }

        public InvalidPluginException(string message, Exception innerException) : base(message, innerException) { }

        protected InvalidPluginException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public InvalidPluginException(Exception invalidDescriptionException) : base(null, invalidDescriptionException) { }
    }
}