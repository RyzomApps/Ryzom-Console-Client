using System.Threading;

namespace RCC
{
    public static class NetworkManager
    {
        public static bool serverReceivedReady = false;

        private static int _LastSentCycle;

        /// <summary>
        /// Send
        /// </summary>
        public static void send(int gameCycle)
        {
            // wait till next server is received
            if (_LastSentCycle >= gameCycle)
            {
                //nlinfo ("Try to CNetManager::send(%d) _LastSentCycle=%d more than one time with the same game cycle, so we wait new game cycle to send", gameCycle, _LastSentCycle);
                while (_LastSentCycle >= gameCycle)
                {
                    // Update network.
                    update();

                    // Send dummy info
                    send();

                    // Do not take all the CPU.
                    Thread.Sleep(100);

                    gameCycle = NetworkConnection.getCurrentServerTick();
                }
            }

            NetworkConnection.send(gameCycle);
        }

        /// <summary>
        /// Updates the whole connection with the frontend.
        /// Call this method evently.
        /// </summary>
        /// <returns>'true' if data were sent/received.</returns>
        public static bool update()
        {
            // Update the base class.
            bool result = NetworkConnection.update();

            // Get changes with the update.
            // 	const vector<CChange> &changes = NetMngr.getChanges();
            // TODO: update everyting

            return true;
        }

        /// <summary>
        /// Send
        /// </summary>
        public static void send()
        {
            NetworkConnection.send();
        }

        public static void initializeNetwork()
        {
            // Callbacks
        }
    }
}