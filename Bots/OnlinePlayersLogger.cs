using System.Diagnostics;
using MinecraftClient;

namespace RCC.Bots
{
    /// <summary>
    /// Record and save replay file that can be used by the Replay mod (https://www.replaymod.com/)
    /// </summary>
    public class OnlinePlayersLogger : ChatBot
    {
        public OnlinePlayersLogger()
        {

        }

        public override void Initialize()
        {
            RegisterChatBotCommand("info", "bot.OnlinePlayersLogger.cmd", "", Command);
        }

        public override void Update()
        {
            Debug.Print("Updated OnlinePlayersLogger");
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            return false;
        }

        public string Command(string cmd, string[] args)
        {
            return "bot.OnlinePlayersLogger.cmd";
        }
    }
}
