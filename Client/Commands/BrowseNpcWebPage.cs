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

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            // set the new page to explore
            var UrlTextId = ryzomClient.GetDatabaseManager().GetProp("SERVER:TARGET:CONTEXT_MENU:WEB_PAGE_URL");

            if (UrlTextId == 0)
                return "Target has no web page attached.";

            ryzomClient.GetStringManager().GetDynString((uint)UrlTextId, out var url, ryzomClient.GetNetworkManager());

            if (url.Trim() == "")
                return "Attached web page is empty.";

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = ClientConfig.WebIgMainDomain + "/" + url.Replace(" ", "/index.php?");
            }

            //ryzomClient.GetActionHandlerManager().GetActionHandler("browse").Execute(this, url);
            ryzomClient.GetLogger().Info($"Browsing {url}");
            ryzomClient.GetWebTransfer().Get(url);

            return "";
        }

        public override IEnumerable<string> GetCmdAliases()
        {
            return new[] { "" };
        }
    }
}
