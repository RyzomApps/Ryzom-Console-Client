using API.Plugins;

namespace AutoTeamJoiner
{
    /// <summary>
    /// Handle events for all Player related events
    /// </summary>
    /// <author>bierdosenhalter</author>
    internal class Listener : ListenerBase
    {
        private readonly Main _plugin;

        public Listener(Main plugin)
        {
            _plugin = plugin;
        }

        public override void OnTeamInvitation(in uint textID)
        {
            _plugin.GetClient().GetINetworkManager().SendMsgToServer("TEAM:JOIN");
        }
    }
}
