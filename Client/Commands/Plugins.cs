using API;
using API.Chat;
using API.Commands;
using System.Collections.Generic;
using System.Text;

namespace Client.Commands
{
    public class Plugins : CommandBase
    {
        public override string CmdName => "plugins";
        public override string CmdUsage => "";
        public override string CmdDesc => "Gets a list of plugins running on the client";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = $"Plugins {GetPluginList(handler)}";
            return true;
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
            return ["pl"];
        }
    }
}