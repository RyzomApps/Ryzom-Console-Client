using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Config;

namespace Client.Commands
{
    public class BrowseNpcWebPage : CommandBase
    {
        public override string CmdName => "BrowseNpcWebPage";

        public override string CmdUsage => "";

        public override string CmdDesc => "Browse a npc web page";

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient ryzomClient)
                throw new Exception("Command handler is not a Ryzom client.");

            // set the new page to explore
            var urlTextId = ryzomClient.GetDatabaseManager().GetProp("SERVER:TARGET:CONTEXT_MENU:WEB_PAGE_URL");

            if (urlTextId == 0)
            {
                responseMsg = "Target has no web page attached.";
                return false;
            }

            ryzomClient.GetStringManager().GetDynString((uint)urlTextId, out var url, ryzomClient.GetNetworkManager());

            if (url.Trim() == "")
            {
                responseMsg = "Attached web page is empty.";
                return false;
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://")) url = $"{ClientConfig.WebIgMainDomain}/{url.Replace(" ", "/index.php?")}";

            //ryzomClient.GetActionHandlerManager().GetActionHandler("browse").Execute(this, url);
            responseMsg = $"Browsing {url}...";
            ryzomClient.GetWebTransfer().Get(url);

            return true;
        }
    }
}