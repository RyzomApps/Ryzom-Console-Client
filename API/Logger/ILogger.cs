///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

namespace API.Logger
{
    /// <summary>
    /// basic interface for text logging
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// true, if debugging messages should be output
        /// </summary>
        bool DebugEnabled { get; set; }
        /// <summary>
        /// true, if warnings messages should be output
        /// </summary>
        bool WarnEnabled { get; set; }
        /// <summary>
        /// true, if informational messages should be output
        /// </summary>
        bool InfoEnabled { get; set; }
        /// <summary>
        /// true, if error-handling messages should be output
        /// </summary>
        bool ErrorEnabled { get; set; }
        /// <summary>
        /// true, if the ingame chat should be output
        /// </summary>
        bool ChatEnabled { get; set; }

        /// <summary>
        /// Output an informational message
        /// </summary>
        void Info(string msg);
        /// <summary>
        /// Output an informational message with formatting parameters
        /// </summary>
        void Info(string msg, params object[] args);
        /// <summary>
        /// Output an informational message
        /// </summary>
        void Info(object msg);

        /// <summary>
        /// Output a debugging message
        /// </summary>
        void Debug(string msg);
        /// <summary>
        /// Output a debugging message with formatting parameters
        /// </summary>
        void Debug(string msg, params object[] args);
        /// <summary>
        /// Output a debugging message
        /// </summary>
        void Debug(object msg);

        /// <summary>
        /// Output a debugging message
        /// </summary>
        void Warn(string msg);
        /// <summary>
        /// Output a debugging message with formatting parameters
        /// </summary>
        void Warn(string msg, params object[] args);
        /// <summary>
        /// Output a debugging message
        /// </summary>
        void Warn(object msg);

        /// <summary>
        /// Output an error-handling message
        /// </summary>
        void Error(string msg);
        /// <summary>
        /// Output an error-handling message with formatting parameters
        /// </summary>
        void Error(string msg, params object[] args);
        /// <summary>
        /// Output an error-handling message
        /// </summary>
        void Error(object msg);

        /// <summary>
        /// Output an ingame chat message
        /// </summary>
        void Chat(string msg);
        /// <summary>
        /// Output an ingame chat message with formatting parameters
        /// </summary>
        void Chat(string msg, params object[] args);
        /// <summary>
        /// Output an ingame chat message
        /// </summary>
        void Chat(object msg);
    }
}