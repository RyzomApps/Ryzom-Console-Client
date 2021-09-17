namespace RCC.Network
{
    /// <summary>
    ///     The states of the connection to the server (if you change them, change ConnectionStateCStr)
    /// </summary>
    internal enum ConnectionState
    {
        /// <summary>
        ///     nothing happened yet
        /// </summary>
        NotInitialised = 0,

        /// <summary>
        ///     init() called
        /// </summary>
        NotConnected,

        /// <summary>
        ///     connect() called, identified by the login server
        /// </summary>
        Authenticate,

        /// <summary>
        ///     connecting to the frontend, sending identification
        /// </summary>
        Login,

        /// <summary>
        ///     connection accepted by the frontend, synchronizing
        /// </summary>
        Synchronize,

        /// <summary>
        ///     synchronized, connected, ready to work
        /// </summary>
        Connected,

        /// <summary>
        ///     connection lost by frontend, probing for response
        /// </summary>
        Probe,

        /// <summary>
        ///     server is stalled
        /// </summary>
        Stalled,

        /// <summary>
        ///     disconnect() called, or timeout, or connection closed by frontend
        /// </summary>
        Disconnect,

        /// <summary>
        ///     quit() called
        /// </summary>
        Quit
    };
}