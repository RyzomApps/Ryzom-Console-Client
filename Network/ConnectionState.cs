namespace RCC.Network
{
    /// The states of the connection to the server (if you change them, change ConnectionStateCStr)
    internal enum ConnectionState
    {
        NotInitialised = 0,     // nothing happened yet
        NotConnected,           // init() called
        Authenticate,           // connect() called, identified by the login server
        Login,                  // connecting to the frontend, sending identification
        Synchronize,            // connection accepted by the frontend, synchronizing
        Connected,              // synchronized, connected, ready to work
        Probe,                  // connection lost by frontend, probing for response
        Stalled,                // server is stalled
        Disconnect,             // disconnect() called, or timeout, or connection closed by frontend
        Quit                    // quit() called
    };
}