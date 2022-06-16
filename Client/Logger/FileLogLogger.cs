///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using API.Chat;
using API.Helper;

namespace Client.Logger
{
    /// <summary>
    /// Logger for logfiles
    /// </summary>
    public class FileLogLogger : FilteredLogger
    {
        private readonly string _logFile;
        private readonly object _logFileLock = new object();
        private readonly bool _prependTimestamp;

        /// <summary>
        /// Constructor
        /// </summary>
        public FileLogLogger(string file, bool prependTimestamp = false)
        {
            _logFile = file;
            _prependTimestamp = prependTimestamp;
            Save("### Log started at " + Misc.GetTimestamp() + " ###");
        }

        /// <summary>
        /// Processing of the message
        /// </summary>
        private void LogAndSave(string msg)
        {
            Log(msg);
            Save(msg);
        }

        /// <summary>
        /// Saves the message to the logfile
        /// </summary>
        private void Save(string msg)
        {
            try
            {
                msg = ChatColor.GetVerbatim(msg);
                msg = ChatManagerHelper.GetVerbatim(msg);

                if (_prependTimestamp)
                    msg = $"{Misc.GetTimestamp()} {msg}";

                var directory = Path.GetDirectoryName(_logFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                lock (_logFileLock)
                {
                    var stream = new FileStream(_logFile, FileMode.OpenOrCreate);
                    var writer = new StreamWriter(stream);
                    stream.Seek(0, SeekOrigin.End);
                    writer.WriteLine(msg);
                    writer.Dispose();
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                // Must use base since we already failed to write log
                base.Error("Cannot write to log file: " + e.Message);
                base.Debug("Stack trace: \n" + e.StackTrace);
            }
        }

        /// <inheritdoc />
        public override void Chat(string msg)
        {
            if (!ChatEnabled) return;
            if (ShouldDisplay(FilterChannel.Chat, msg))
            {
                LogAndSave(msg);
            }
            else Debug("[Logger] One Chat message filtered: " + msg);
        }

        /// <inheritdoc />
        public override void Debug(string msg)
        {
            if (!DebugEnabled) return;
            if (ShouldDisplay(FilterChannel.Debug, msg))
            {
                LogAndSave("§8[DEBUG] " + msg);
            }
        }

        /// <inheritdoc />
        public override void Error(string msg)
        {
            base.Error(msg);
            if (ErrorEnabled)
                Save(msg);
        }

        /// <inheritdoc />
        public override void Info(string msg)
        {
            base.Info(msg);
            if (InfoEnabled)
                Save(msg);
        }

        /// <inheritdoc />
        public override void Warn(string msg)
        {
            base.Warn(msg);
            if (WarnEnabled)
                Save(msg);
        }
    }
}