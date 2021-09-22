using System.Collections.Generic;
using RCC.Automata.Internal;

namespace RCC.Automata
{
    internal class AutoTeamJoiner : AutomatonBase
    {
        public override void OnInitialize()
        {
            RyzomClient.GetInstance().GetLogger().Info("Automaton 'AutoTeamJoiner' initialized.");
        }

        public override void OnTeamInvitation(in uint textID)
        {
            Handler.GetNetworkManager().SendMsgToServer("TEAM:JOIN");
        }
    }
}
