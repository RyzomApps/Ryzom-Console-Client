using System.Collections.Generic;
using System.Text;
using API;
using API.Chat;
using API.Commands;

namespace Client.Commands
{
    public class Plugins : CommandBase
    {
        public override string CmdName => "plugins";
        public override string CmdUsage => "";
        public override string CmdDesc => "Gets a list of plugins running on the client";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            return $"Plugins {GetPluginList(handler)}";
        }

        private static string GetPluginList(IClient handler)
        {
            var pluginList = new StringBuilder();
            var plugins = handler.GetPluginManager().GetPlugins();

            foreach (var plugin in plugins)
            {
                if (pluginList.Length > 0)
                {
                    pluginList.Append(ChatColor.WHITE);
                    pluginList.Append(", ");
                }

                pluginList.Append(plugin.IsEnabled() ? ChatColor.GREEN : ChatColor.RED);
                pluginList.Append(plugin.GetDescription().GetName());
            }

            return $"({plugins.Length}): {pluginList}";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] {"pl"};
        }
    }
}