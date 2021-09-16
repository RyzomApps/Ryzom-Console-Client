using System;
using System.Collections.Generic;
using RCC.Client;
using RCC.Config;
using RCC.Helper;
using RCC.Network;

namespace RCC.Commands
{
    public class SelectCharacter : Command
    {
        public override string CmdName => "SelectCharacter";
        public override string CmdUsage => "SelectCharacter";
        public override string CmdDesc => "cmd.SelectCharacter.desc";

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            if (Connection.SendCharSelection)
            {
                var charSelect = -1;
            
                if (ClientCfg.SelectCharacter != -1)
                    charSelect = ClientCfg.SelectCharacter;
            
                Connection.WaitServerAnswer = false;
            
                // check that the pre selected character is available
                if (Connection.CharacterSummaries[charSelect].People == (int)TPeople.Unknown || charSelect > 4)
                {
                    // BAD ! preselected char does not exist
                    throw new InvalidOperationException("preselected char does not exist");
                }
            
                // Auto-selection for fast launching (dev only)
                //CAHManager::getInstance()->runActionHandler("launch_game", NULL, toString("slot=%d|edit_mode=0", charSelect));
                Connection.SendCharSelection = false;
                CAHLaunchGame.execute("0");
            }
            else
            {
                ConsoleIO.WriteLineFormatted("Cannot send Char Selection in this state.");
            }

            return "";
        }

        public override IEnumerable<string> getCMDAliases()
        {
            return new[] { "" };
        }
    }
}