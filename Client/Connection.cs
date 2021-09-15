using System.Collections.Generic;
using RCC.Network;

namespace RCC.Client
{
    public static class Connection
    {
        public static byte PlayerSelectedSlot = 0;
        public static byte ServerPeopleActive = 255;
        public static byte ServerCareerActive = 255;

        public static List<CCharacterSummary> CharacterSummaries = new List<CCharacterSummary>();
        public static bool WaitServerAnswer;

        public static bool game_exit = false;
                
        public static bool userChar;
        public static bool noUserChar;
        public static bool ConnectInterf;
        public static bool CreateInterf;
        public static bool CharacterInterf;
                
        public static bool CharNameValidArrived;

        // non ryzom variables (for workarounds)
        public static bool SendCharSelection = false;
    }
}
