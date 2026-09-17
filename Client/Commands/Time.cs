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

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            var csLocal = DateTime.Now.ToString("HH:mm:ss");
            var csUtc = DateTime.UtcNow.ToString("HH:mm:ss");

            responseMsg = "Current local time is %local, UTC time is %utc.";

            responseMsg = responseMsg.Replace("%local", csLocal);
            responseMsg = responseMsg.Replace("%utc", csUtc);

            return true;
        }
    }
}