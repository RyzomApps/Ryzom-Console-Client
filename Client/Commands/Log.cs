using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    /// <summary>
    /// Add/Del Positive/Negative Filters for logs
    /// </summary>
    public class Log : CommandBase
    {
        public override string CmdName => "log";

        public override string CmdUsage => "<chat|debug|info|error|warn>";

        public override string CmdDesc => "Toggle Positive/Negative Filters for different log types";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // check args, if there s not the right number of parameter, return bad
            if (args.Length != 1)
                return $"Usage: {CmdUsage}";

            switch (args[0].ToLower())
            {
                case "chat":
                    // Chat system.
                    ryzomClient.Log.ChatEnabled = !ryzomClient.Log.ChatEnabled;
                    return ryzomClient.Log.ChatEnabled ? "enabled" : "disabled";

                case "debug":
                    // Debug log system.
                    ryzomClient.Log.DebugEnabled = !ryzomClient.Log.DebugEnabled;
                    return ryzomClient.Log.DebugEnabled ? "enabled" : "disabled";

                case "info":
                    // Info log system.
                    ryzomClient.Log.InfoEnabled = !ryzomClient.Log.InfoEnabled;
                    return ryzomClient.Log.InfoEnabled ? "enabled" : "disabled";

                case "error":
                    // Error log system.
                    ryzomClient.Log.ErrorEnabled = !ryzomClient.Log.ErrorEnabled;
                    return ryzomClient.Log.ErrorEnabled ? "enabled" : "disabled";

                case "warn":
                case "warning":
                    // Warning log system.
                    ryzomClient.Log.WarnEnabled = !ryzomClient.Log.WarnEnabled;
                    return ryzomClient.Log.WarnEnabled ? "enabled" : "disabled";

                default:
                    // Unknown Log System . return false.
                    return $"Usage: {CmdUsage}";
            }
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}