using System;
using System.Collections.Generic;
using API;
using API.Commands;

namespace Client.Commands
{
    public class Time : CommandBase
    {
        public override string CmdName => "time";

        public override string CmdUsage => "";

        public override string CmdDesc => "Shows information about the current time";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            var csLocal = DateTime.Now.ToString("HH:mm:ss");
            var csUtc = DateTime.UtcNow.ToString("HH:mm:ss");

            var msg = "Current local time is %local, UTC time is %utc.";

            msg = msg.Replace("%local", csLocal);
            msg = msg.Replace("%utc", csUtc);

            return msg;
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new string[] { };
        }
    }
}