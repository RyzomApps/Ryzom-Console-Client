using RCC.Logger;
using System;

namespace RCC
{
    /// <summary>
    /// The main client class, used to connect to a Ryzom server.
    /// </summary>
    public class RyzomClient
    {
        public ILogger Log;

        public ILogger GetLogger() { return this.Log; }

        /// <summary>
        /// Starts the main chat client
        /// </summary>
        public RyzomClient()
        {
            StartClient();
        }

        /// <summary>
        /// Starts the main chat client, wich will login to the server using the RyzomCom class.
        /// </summary>
        private void StartClient()
        {
            Console.WriteLine("Hello World!");
        }

        /// <summary>
        /// Disconnect the client from the server (initiated from MCC)
        /// </summary>
        public void Disconnect()
        {

        }
    }
}
