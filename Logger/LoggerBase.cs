// This code is a modified version of a file from the 'Minecraft Console Client'
// <https://github.com/ORelio/Minecraft-Console-Client>,
// which is released under CDDL-1.0 License.
// <http://opensource.org/licenses/CDDL-1.0>
// Original Copyright 2021 by ORelio and Contributers

using RCC.Helper;

namespace RCC.Logger
{
    /// <summary>
    ///     Abstract class providing basic implementation of the ILogger interface
    /// </summary>
    public abstract class LoggerBase : ILogger
    {
        public bool DebugEnabled { get; set; } = false;
        public bool WarnEnabled { get; set; } = true;
        public bool InfoEnabled { get; set; } = true;
        public bool ErrorEnabled { get; set; } = true;
        public bool ChatEnabled { get; set; } = true;

        public abstract void Chat(string msg);

        public void Chat(string msg, params object[] args)
        {
            Chat(string.Format(msg, args));
        }

        public void Chat(object msg)
        {
            Chat(msg.ToString());
        }

        public abstract void Debug(string msg);

        public void Debug(string msg, params object[] args)
        {
            Debug(string.Format(msg, args));
        }

        public void Debug(object msg)
        {
            Debug(msg.ToString());
        }

        public abstract void Error(string msg);

        public void Error(string msg, params object[] args)
        {
            Error(string.Format(msg, args));
        }

        public void Error(object msg)
        {
            Error(msg.ToString());
        }

        public abstract void Info(string msg);

        public void Info(string msg, params object[] args)
        {
            Info(string.Format(msg, args));
        }

        public void Info(object msg)
        {
            Info(msg.ToString());
        }

        public abstract void Warn(string msg);

        public void Warn(string msg, params object[] args)
        {
            Warn(string.Format(msg, args));
        }

        public void Warn(object msg)
        {
            Warn(msg.ToString());
        }

        protected virtual void Log(object msg)
        {
            ConsoleIO.WriteLineFormatted(msg.ToString());
        }

        protected virtual void Log(string msg)
        {
            ConsoleIO.WriteLineFormatted(msg);
        }

        protected virtual void Log(string msg, params object[] args)
        {
            ConsoleIO.WriteLineFormatted(string.Format(msg, args));
        }
    }
}