using System.Text;
using API.Plugins.Interfaces;

namespace API.Logger
{
    /// <summary>
    /// The PluginLoggerWrapper class is a modified <see cref="ILogger"/> that prepends all
    /// logging calls with the name of the plugin doing the logging. The API for
    /// PluginLoggerWrapper is exactly the same as <see cref="ILogger"/>.
    /// </summary>
    public class PluginLoggerWrapper : ILogger
    {
        private readonly ILogger _logger;
        private readonly string _pluginName;

        /// <summary>
        /// Creates a new PluginLoggerWrapper that extracts the name from a plugin.
        /// </summary>
        /// <param name="context">A reference to the plugin</param>
        /// <param name="logger">Logger that should be wrapped for the plugin</param>
        public PluginLoggerWrapper(IPlugin context, ILogger logger)
        {
            _logger = logger;
            var prefix = context.GetDescription().GetPrefix();
            _pluginName = prefix != null ? new StringBuilder().Append("[").Append(prefix).Append("] ").ToString() : "[" + context.GetDescription().GetName() + "] ";
        }

        /// <inheritdoc/>
        public bool DebugEnabled { get; set; } = false;

        /// <inheritdoc/>
        public bool WarnEnabled { get; set; } = true;

        /// <inheritdoc/>
        public bool InfoEnabled { get; set; } = true;

        /// <inheritdoc/>
        public bool ErrorEnabled { get; set; } = true;

        /// <inheritdoc/>
        public bool ChatEnabled { get; set; } = true;

        /// <inheritdoc/>
        public void Chat(string msg, params object[] args)
        {
            Chat(string.Format(msg, args));
        }

        /// <inheritdoc/>
        public void Chat(object msg)
        {
            Chat(msg.ToString());
        }

        /// <inheritdoc/>
        public void Debug(string msg, params object[] args)
        {
            Debug(string.Format(msg, args));
        }

        /// <inheritdoc/>
        public void Debug(object msg)
        {
            Debug(msg.ToString());
        }

        /// <inheritdoc/>
        public void Error(string msg, params object[] args)
        {
            Error(string.Format(msg, args));
        }

        /// <inheritdoc/>
        public void Error(object msg)
        {
            Error(msg.ToString());
        }

        /// <inheritdoc/>
        public void Info(string msg, params object[] args)
        {
            Info(string.Format(msg, args));
        }

        /// <inheritdoc/>
        public void Info(object msg)
        {
            Info(msg.ToString());
        }

        /// <inheritdoc/>
        public void Warn(string msg, params object[] args)
        {
            Warn(string.Format(msg, args));
        }

        /// <inheritdoc/>
        public void Warn(object msg)
        {
            Warn(msg.ToString());
        }

        /// <inheritdoc/>
        public void Debug(string msg)
        {
            if (DebugEnabled)
                _logger.Debug(_pluginName + msg);
        }

        /// <inheritdoc/>
        public void Info(string msg)
        {
            if (InfoEnabled)
                _logger.Info(_pluginName + msg);
        }

        /// <inheritdoc/>
        public void Warn(string msg)
        {
            if (WarnEnabled)
                _logger.Warn(_pluginName + msg);
        }

        /// <inheritdoc/>
        public void Error(string msg)
        {
            if (ErrorEnabled)
                _logger.Error(_pluginName + msg);
        }

        /// <inheritdoc/>
        public void Chat(string msg)
        {
            if (ChatEnabled)
                _logger.Chat(_pluginName + msg);
        }
    }
}
