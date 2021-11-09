using System.Text;
using RCC.Plugins;

namespace RCC.Logger
{
    /// <summary>
    /// The PluginLogger class is a modified <see cref="ILogger"/> that prepends all
    /// logging calls with the name of the plugin doing the logging. The API for
    /// PluginLogger is exactly the same as <see cref="ILogger"/>.
    /// </summary>
    internal class PluginLogger : FilteredLogger
    {
        private readonly string _pluginName;

        /// <summary>
        /// Creates a new PluginLogger that extracts the name from a plugin.
        /// </summary>
        /// <params name="context">A reference to the plugin</params>
        public PluginLogger(IPlugin context)
        {
            var prefix = context.GetDescription().GetPrefix();
            _pluginName = prefix != null ? new StringBuilder().Append("[").Append(prefix).Append("] ").ToString() : "[" + context.GetDescription().GetName() + "] ";
        }

        /// <inheritdoc />
        protected override void Log(string msg)
        {
            base.Log(_pluginName + msg);
        }

        /// <inheritdoc />
        protected override void Log(object msg)
        {
            base.Log(_pluginName + msg);
        }

        /// <inheritdoc />
        protected override void Log(string msg, params object[] args)
        {
            base.Log(_pluginName + string.Format(msg, args));
        }
    }
}
