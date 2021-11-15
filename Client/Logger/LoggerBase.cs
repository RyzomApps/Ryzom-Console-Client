///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using API.Logger;
using Client.Helper;

namespace Client.Logger
{
    /// <summary>
    /// Abstract class providing basic implementation of the ILogger interface
    /// </summary>
    public abstract class LoggerBase : ILogger
    {
        public bool DebugEnabled { get; set; } = false;
        /// <inheritdoc />
        public bool WarnEnabled { get; set; } = true;
        /// <inheritdoc />
        public bool InfoEnabled { get; set; } = true;
        /// <inheritdoc />
        public bool ErrorEnabled { get; set; } = true;
        /// <inheritdoc />
        public bool ChatEnabled { get; set; } = true;

        /// <inheritdoc />
        public abstract void Chat(string msg);

        /// <inheritdoc />
        public void Chat(string msg, params object[] args)
        {
            Chat(string.Format(msg, args));
        }

        /// <inheritdoc />
        public void Chat(object msg)
        {
            Chat(msg.ToString());
        }

        /// <inheritdoc />
        public abstract void Debug(string msg);

        /// <inheritdoc />
        public void Debug(string msg, params object[] args)
        {
            Debug(string.Format(msg, args));
        }

        /// <inheritdoc />
        public void Debug(object msg)
        {
            Debug(msg.ToString());
        }

        /// <inheritdoc />
        public abstract void Error(string msg);

        /// <inheritdoc />
        public void Error(string msg, params object[] args)
        {
            Error(string.Format(msg, args));
        }

        /// <inheritdoc />
        public void Error(object msg)
        {
            Error(msg.ToString());
        }

        /// <inheritdoc />
        public abstract void Info(string msg);

        /// <inheritdoc />
        public void Info(string msg, params object[] args)
        {
            Info(string.Format(msg, args));
        }

        /// <inheritdoc />
        public void Info(object msg)
        {
            Info(msg.ToString());
        }

        /// <inheritdoc />
        public abstract void Warn(string msg);

        /// <inheritdoc />
        public void Warn(string msg, params object[] args)
        {
            Warn(string.Format(msg, args));
        }

        /// <inheritdoc />
        public void Warn(object msg)
        {
            Warn(msg.ToString());
        }

        /// <summary>
        /// Log a formatted object. ToString is used.
        /// </summary>
        protected virtual void Log(object msg)
        {
            ConsoleIO.WriteLineFormatted(msg.ToString());
        }

        /// <summary>
        /// Log a formatted message
        /// </summary>
        protected virtual void Log(string msg)
        {
            ConsoleIO.WriteLineFormatted(msg);
        }

        /// <summary>
        /// Log a formatted message, with format arguments
        /// </summary>
        protected virtual void Log(string msg, params object[] args)
        {
            ConsoleIO.WriteLineFormatted(string.Format(msg, args));
        }
    }
}