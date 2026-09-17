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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            // check args, if there s not the right number of parameter, return bad
            if (args.Length != 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            switch (args[0].ToLower())
            {
                case "chat":
                    // Chat system.
                    ryzomClient.Log.ChatEnabled = !ryzomClient.Log.ChatEnabled;

                    responseMsg = ryzomClient.Log.ChatEnabled ? "enabled" : "disabled";
                    return true;

                case "debug":
                    // Debug log system.
                    ryzomClient.Log.DebugEnabled = !ryzomClient.Log.DebugEnabled;

                    responseMsg = ryzomClient.Log.DebugEnabled ? "enabled" : "disabled";
                    return true;


                case "info":
                    // Info log system.
                    ryzomClient.Log.InfoEnabled = !ryzomClient.Log.InfoEnabled;

                    responseMsg = ryzomClient.Log.InfoEnabled ? "enabled" : "disabled";
                    return true;

                case "error":
                    // Error log system.
                    ryzomClient.Log.ErrorEnabled = !ryzomClient.Log.ErrorEnabled;

                    responseMsg = ryzomClient.Log.ErrorEnabled ? "enabled" : "disabled";
                    return true;

                case "warn":
                case "warning":
                    // Warning log system.
                    ryzomClient.Log.WarnEnabled = !ryzomClient.Log.WarnEnabled;

                    responseMsg = ryzomClient.Log.WarnEnabled ? "enabled" : "disabled";
                    return true;

                default:
                    // Unknown Log System . return false.

                    responseMsg = $"Usage: {CmdUsage}";
                    return true;
            }
        }
    }
}